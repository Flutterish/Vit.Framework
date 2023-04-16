using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Parsing.Binary;

public struct BinaryArrayView<T> : IEnumerable<T> {
	public BinaryViewContext Context;
	public int ElementStride;
	public int Length;

	public BinaryArrayView ( BinaryViewContext context, int elementStride, int length ) {
		Context = context;
		ElementStride = elementStride;
		Length = length;
	}

	public T this[int index] {
		get {
			Debug.Assert( index >= 0 && index < Length );
			return BinaryView<T>.Parse( Context with {
				Offset = Context.Offset + index * ElementStride
			} );
		}
	}

	public RentedArray<T> GetRented () {
		var array = new RentedArray<T>( Length );
		for ( int i = 0; i < Length; i++ ) {
			array[i] = this[i];
		}
		return array;
	}

	public override string ToString () {
		return $"Binary array view (of {Context.TypeInfo.Type.Name}) at offset = 0x{Context.Offset:x}, length = {Length}, stride = {ElementStride} (total size = {Length * ElementStride})";
	}

	public IEnumerator<T> GetEnumerator () {
		for ( int i = 0; i < Length; i++ ) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator () {
		for ( int i = 0; i < Length; i++ ) {
			yield return this[i];
		}
	}
}

public sealed class CachedBinaryView<T> {
	BinaryView<T> view;
	T? cache;

	public ref BinaryViewContext Context => ref view.Context;
	public CachedBinaryView ( BinaryViewContext context, bool allowNullType = false ) {
		view = new( context, allowNullType );
	}

	public T Value => cache ??= view.Value;

	public override string ToString () {
		return $"{view} (cached)";
	}
}

public struct BinaryView<T> {
	public BinaryViewContext Context;
	public BinaryView ( BinaryViewContext context, bool allowNullType = false ) {
		if ( !allowNullType )
			context.TypeInfo.Type ??= typeof( T );
		Context = context;
	}

	public T Value {
		get {
			if ( Context.TypeInfo.Type == null )
				return default!;

			Context.Reader.Value.Stream.Position = Context.Offset;
			return (T)parse( Context, Context.TypeInfo )!;
		}
	}

	public static T Parse ( BinaryViewContext context ) {
		context.TypeInfo.Type ??= typeof( T );
		context.Reader.Value.Stream.Position = context.Offset;
		return (T)parse( context, context.TypeInfo )!;
	}

	static object? parse ( BinaryViewContext context, TypeInfo typeInfo ) {
		context.TypeInfo = typeInfo;
		var type = typeInfo.Type;

		static bool isAnyBinaryView ( Type type ) {
			return isBinaryView( type ) || isBinaryArrayView( type );
		}
		static bool isBinaryView ( Type type ) {
			return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof( BinaryView<> ) || type.GetGenericTypeDefinition() == typeof( CachedBinaryView<> ) );
		}
		static bool isBinaryArrayView ( Type type ) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( BinaryArrayView<> );
		}

		static bool isConstantSize ( Type type, MemberInfo? member, out int size ) {
			if ( type.IsPrimitive ) {
				size = (int)typeof( Marshal ).GetMethod( nameof( Marshal.SizeOf ), 1, Array.Empty<Type>() )!.MakeGenericMethod( type ).Invoke( null, Array.Empty<object?>() )!;
				return true;
			}
			else if ( isBinaryView( type ) ) {
				size = 0;
				return true;
			}
			else if ( isBinaryArrayView( type ) ) {
				if ( member?.GetCustomAttribute<DataOffsetAttribute>() != null ) {
					size = 0;
					return true;
				}
				else {
					size = -1;
					return false;
				}
			}
			else if ( type.IsClass || type.IsValueType ) {
				var members = type.GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
					.Where( x => x is FieldInfo or PropertyInfo { CanWrite: true } );

				size = 0;
				foreach ( var i in members ) {
					if ( !isConstantSize( i is FieldInfo f ? f.FieldType : ( (PropertyInfo)i )!.PropertyType, i, out var memberSize ) ) {
						size = -1;
						return false;
					}

					size += memberSize;
				}

				return true;
			}

			size = -1;
			return false;
		}

		if ( type.IsPrimitive ) {
			return context.Reader.Value.Read( type );
		}
		else if ( type.IsArray ) {
			var elementType = type.GetElementType()!;

			if ( isAnyBinaryView( elementType ) )
				throw new NotImplementedException( "Arrays of binary views are not implemented" );

			var count = typeInfo.MemberInfo?.GetCustomAttribute<SizeAttribute>() is SizeAttribute size
				? size.Ref == null
					? size.Value!.Value
					: (int)(context.GetRef<int>( size.Ref, BinaryViewContext.MethodSource.Member ) * size.Multiplier)
				: 0;
			var array = Array.CreateInstance( elementType, count );

			for ( int i = 0; i < count; i++ ) {
				array.SetValue( parse( context with { Members = null }, new TypeInfo { Type = elementType } ), i );
			}

			return array;
		}
		else if ( isBinaryArrayView( type ) ) {
			var elementType = type.GetGenericArguments()[0];
			
			if ( !isConstantSize( elementType, typeInfo.MemberInfo, out var elementSize ) )
				throw new NotImplementedException( "Unsized binary view is not implemented (the element type is not constant size)" );

			var count = typeInfo.MemberInfo?.GetCustomAttribute<SizeAttribute>() is SizeAttribute size
				? size.Ref == null
					? size.Value!.Value
					: (int)( context.GetRef<int>( size.Ref, BinaryViewContext.MethodSource.Member ) * size.Multiplier )
				: 0;

			long offset;
			if ( typeInfo.MemberInfo?.GetCustomAttribute<DataOffsetAttribute>() is DataOffsetAttribute offsetAttribute ) {
				offset = context.OffsetAnchor + context.GetRef<long>( offsetAttribute.Ref, BinaryViewContext.MethodSource.Member );
			}
			else {
				offset = context.Reader.Value.Stream.Position;

				var totalSize = elementSize * count;
				context.Reader.Value.Stream.Position += totalSize;
			}
			
			return Activator.CreateInstance( type, context with {
				Offset = offset,
				TypeInfo = new TypeInfo { Type = elementType }
			}, elementSize, count );
		}
		else if ( isBinaryView( type ) ) {
			var offset = typeInfo.MemberInfo?.GetCustomAttribute<DataOffsetAttribute>();
			if ( offset == null )
				throw new NotImplementedException( "Sized binary view is not implemented (did you mean to use [DataOffset]?)" );

			var offsetValue = context.GetRef<long>( offset.Ref, BinaryViewContext.MethodSource.Member );
			var selectedType = typeInfo.MemberInfo?.GetCustomAttribute<TypeSelectorAttribute>() is TypeSelectorAttribute typeSelector
				? context.GetRef<Type>( typeSelector.Ref, BinaryViewContext.MethodSource.Member )
				: type.GetGenericArguments()[0];
			return Activator.CreateInstance( type, context with {
				Offset = context.OffsetAnchor + offsetValue,
				TypeInfo = context.TypeInfo with { 
					Type = selectedType
				}
			}, true );
		}
		else if ( type == typeof(BinaryViewContext) ) {
			return context with { TypeInfo = default };
		}
		else if ( type.IsValueType || type.IsClass ) {
			if ( type.GetCustomAttribute<OffsetAnchorAttribute>() != null )
				context.OffsetAnchor = context.Reader.Value.Stream.Position;

			Dictionary<MemberInfo, object?> members = new();
			context.Members = members;
			context.Dependenies = new() { Parent = context.Dependenies };
			var resolvedType = typeof(object);

			void parseType ( Type type ) {
				void parseMembers ( Type type ) {
					if ( type == resolvedType )
						return;

					parseMembers( type.BaseType! );
					var newMembers = type.GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly )
						.Where( x => x is FieldInfo or PropertyInfo { CanWrite: true } && x.GetCustomAttribute<DontParseAttribute>() == null );

					foreach ( var i in newMembers ) {
						var memberType = i is FieldInfo field ? field.FieldType : ( i as PropertyInfo )!.PropertyType;
						members[i] = i.GetCustomAttribute<ParseWithAttribute>() is ParseWithAttribute parseWith
							? (context with { TypeInfo = new() { Type = type, MemberInfo = i } } ).GetRef<object?>( parseWith.Ref, BinaryViewContext.MethodSource.Member )
							: parse( context, new TypeInfo {
								Type = memberType,
								MemberInfo = i
							} );

						if ( i.GetCustomAttribute<CacheAttribute>() != null ) {
							(context.Dependenies!.Cache ??= new()).Add( memberType, members[i] );
						}
					}
				}

				parseMembers( type );
				resolvedType = type;

				if ( resolvedType.GetCustomAttribute<TypeSelectorAttribute>( inherit: false ) is TypeSelectorAttribute typeSelector ) {
					context.TypeInfo.Type = resolvedType;
					var nextType = context.GetRef<Type?>( typeSelector.Ref, BinaryViewContext.MethodSource.Type );
					if ( nextType != null && nextType != resolvedType )
						parseType( nextType );
				}
			}

			parseType( type );
			if ( resolvedType == null )
				return type.IsValueType ? Activator.CreateInstance( type ) : null;

			var instance = Activator.CreateInstance( resolvedType );
			foreach ( var (member, value) in members ) {
				if ( member is FieldInfo field )
					field.SetValue( instance, value );
				else if ( member is PropertyInfo prop )
					prop.SetValue( instance, value );
			}
			return instance;
		}
		else {
			throw new Exception( $"Can not parse {type}" );
		}
	}

	public static implicit operator T ( BinaryView<T> view )
		=> view.Value;

	public override string ToString () {
		if ( Context.TypeInfo.Type == null )
			return $"Null binary view (Default of {typeof(T).Name})";
		return $"Binary view (of {Context.TypeInfo.Type.Name}) at offset = 0x{Context.Offset:x}";
	}
}

public struct BinaryViewContext {
	public required Ref<EndianCorrectingBinaryReader> Reader;
	public long Offset;
	public long OffsetAnchor;
	public Dictionary<MemberInfo, object?>? Members;
	public TypeInfo TypeInfo;
	public Dependency? Dependenies;

	public class Dependency {
		public Dependency? Parent;
		public Dictionary<Type, object?>? Cache;

		public object? Get ( Type type ) {
			if ( Cache?.TryGetValue( type, out var value ) == true )
				return value;

			if ( Parent != null )
				return Parent.Get( type );

			throw new Exception( $"Dependency {type.Name} not found" );
		}
	}

	public long StreamPosition {
		get => Reader.Value.Stream.Position;
		set => Reader.Value.Stream.Position = value;
	}

	public void CacheDependency<T> ( T value ) {
		Dependenies ??= new();
		Dependenies.Cache ??= new();
		Dependenies.Cache.Add( typeof(T), value );
	}
	
	public T ResolveDependency<T> () {
		return (T)Dependenies?.Get( typeof(T) )!;
	}

	MethodInfo? getMethod ( string name, MethodSource methodSource ) {
		return methodSource == MethodSource.Member
			? TypeInfo.MemberInfo?.DeclaringType?.GetMethod( name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
			: TypeInfo.Type.GetMethod( name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
	}

	public enum MethodSource {
		Member,
		Type
	}

	public T GetRef<T> ( string name, MethodSource methodSource ) {
		var lowerName = name.ToLower();
		object? data;
		if ( Members?.Keys.FirstOrDefault( x => x.Name.ToLower() == lowerName ) is MemberInfo member ) {
			data = Members[member];
		}
		else if ( getMethod( name, methodSource ) is MethodInfo method ) {
			var members = Members;
			var reader = Reader;
			var dependenies = Dependenies;
			data = method.Invoke( null, method.GetParameters().Select( x => {
				var type = x.ParameterType;
				if ( x.GetCustomAttribute<ResolveAttribute>() != null ) {
					if ( dependenies == null )
						throw new Exception( $"Dependency {type.Name} not found" );
					else
						return dependenies.Get( type );
				}

				var matching = members!.Where( x => memberType( x.Key ) == type );
				var name = x.Name!.ToLower();
				var candidates = matching.Where( x => x.Key.Name.ToLower() == name ).Select( x => x.Value );
				if ( !candidates.Any() ) {
					var implicits = new object[] { reader, reader.Value };
					candidates = implicits.Where( x => x.GetType() == type );
				}

				if ( candidates.Any() )
					return candidates.First();

				throw new Exception( $"Can not resolve parameter {x}" );
			} ).ToArray() );
		}
		else {
			throw new Exception( $"Could not resolve reference `{name}`" );
		}

		if ( data is T value )
			return value;

		if ( data is null ) {
			return default!;
		}

		if ( data is IConvertible d1 && default( T ) is IConvertible ) {
			return (T)d1.ToType( typeof( T ), CultureInfo.InvariantCulture )!;
		}

		var op = data.GetType().GetMethods( BindingFlags.Static | BindingFlags.Public )
				.Where( x => x.Name is "op_Implicit" or "op_Explicit" && x.ReturnType == typeof( T ) )
				.FirstOrDefault();

		if ( op == null )
			throw new Exception( $"Could not cast reference `{name}` = {{{data}}} to {typeof(T).Name}" );

		return (T)op.Invoke( null, new[] { data } )!;
	}

	static Type memberType ( MemberInfo member ) {
		if ( member is FieldInfo f )
			return f.FieldType;
		else if ( member is PropertyInfo p )
			return p.PropertyType;

		throw new Exception( "Invalid member type" );
	}
}

public struct TypeInfo {
	public required Type Type;
	public MemberInfo? MemberInfo;
}