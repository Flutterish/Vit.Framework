using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static Vit.Framework.Parsing.Binary.BinaryFileParser;

namespace Vit.Framework.Parsing.Binary;

public static class BinaryFileParser {
	static readonly Dictionary<TypeInfo, Func<Context, object?>> parsers = new() { };

	public static T Parse<T> ( EndianCorrectingBinaryReader reader ) {
		return (T)parse( new Context { 
			Reader = reader,
			Dependencies = new(),
			DependentTasks = new()
		}, typeof(T) )!;
	}

	static object? parse ( Context context, TypeInfo type ) {
		static object? compute ( Context context, TypeInfo type, Func<Context, object?> parser ) {
			object? value;
			
			if ( type.Offset is DataOffsetAttribute offset ) {
				var startingOffset = context.Reader.Stream.Position;
				context.Reader.Stream.Position = offset.GetValue( context );
				value = parser( context );
				context.Reader.Stream.Position = startingOffset;
			}
			else {
				value = parser( context );
			}

			if ( type.IsDependency )
				context.Dependencies[type.Type] = value;

			return value;
		}

		if ( type.TypeSelector is TypeSelectorAttribute typeSelector ) {
			var newType = typeSelector.GetValue( context );
			if ( newType == null )
				return type.Type.IsValueType ? Activator.CreateInstance( type.Type ) : null;
			return parse( context, type.SubstituteType( newType ) );
		}

		if ( type.ParseWith is ParseWithAttribute parseWith ) {
			return parseWith.GetValue( context );
		}

		if ( parsers.TryGetValue( type, out var parser ) )
			return compute( context, type, parser );

		parser = makePrimitiveParser( type )
				?? makeArrayParser( type )
				?? makeStructureParser( type );

		if ( parser == null )
			throw new Exception( "Could not parse value" );

		parsers[type] = parser;
		return compute( context, type, parser );
	}

	static Func<Context, object?>? makePrimitiveParser ( TypeInfo type ) {
		if ( !type.Type.IsPrimitive )
			return null;

		static Func<Context, object?> generator<T> () where T : unmanaged, IConvertible {
			return ctx => ctx.Reader.Read<T>();
		}

		return apply( type.Type, generator<int> );
	}

	static Func<Context, object?>? makeStructureParser ( TypeInfo type ) {
		if ( !type.Type.IsValueType && !type.Type.IsClass )
			return null;

		var baseMembers = type.Type.GetMembers( BindingFlags.Public | BindingFlags.Instance )
			.Where( x => x is FieldInfo or PropertyInfo { CanWrite: true } ).ToArray();

		return ctx => {
			Dictionary<MemberInfo, object?> memberValues = new();
			var childContext = ctx.GetChildContext( type, memberValues );
			var members = baseMembers;

			object? process ( TypeInfo type ) {
				foreach ( var i in members ) {
					memberValues[i] = parse( childContext, i );
				}

				if ( type.AbstractTypeSelector is TypeSelectorAttribute typeSelector ) {
					var newType = typeSelector.GetValue( childContext );
					if ( newType == null )
						return type.Type.IsValueType ? Activator.CreateInstance( type.Type ) : null;

					childContext = childContext with { TypeInfo = newType };
					members = newType.GetMembers( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
						.Where( x => x is FieldInfo or PropertyInfo { CanWrite: true } ).ToArray();
					return process( newType );
				}
				else {
					return Activator.CreateInstance( type.Type );
				}
			}

			var instance = process( type );
			foreach ( var (member, value) in memberValues ) {
				setValue( member, instance, value );
			}

			return instance;
		};
	}

	static Func<Context, object?>? makeArrayParser ( TypeInfo type ) {
		if ( !type.Type.IsArray )
			return null;
		var arrayType = type.Type.GetElementType()!;

		static Func<Context, object?> emptyGenerator<T> () {
			return _ => Array.Empty<T>();
		}

		if ( type.Size == null )
			return apply( arrayType, emptyGenerator<int> );

		Func<Context, object?> generator<T> () {
			return ctx => {
				var value = new T[type.Size!.GetValue(ctx)];
				foreach ( ref var i in value.AsSpan() )
					i = (T)parse( ctx, typeof(T) )!;

				return value;
			};
		}

		return apply( arrayType, generator<int> );
	}

	static Type memberType ( MemberInfo member ) {
		if ( member is FieldInfo f )
			return f.FieldType;
		else if ( member is PropertyInfo p )
			return p.PropertyType;

		throw new Exception( "Invalid member type" );
	}

	static void setValue ( MemberInfo member, object? target, object? value ) {
		if ( member is FieldInfo f )
			f.SetValue( target, value );
		else if ( member is PropertyInfo p )
			p.SetValue( target, value );
		else
			throw new Exception( "Invalid member type" );
	}

	static T apply<T> ( Type type, Func<T> generator ) {
		return (T)generator.GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod( type ).Invoke( generator.Target, Array.Empty<object?>() )!;
	}

	public record Context {
		public required EndianCorrectingBinaryReader Reader { get; init; }
		public Context? ParentContext { get; init; }
		public TypeInfo TypeInfo { get; init; }
		public Dictionary<MemberInfo, object?>? MemberValues { get; init; }
		public long Offset { get; init; }

		public required Dictionary<Type, object?> Dependencies { get; init; }

		public required List<DependentTask> DependentTasks { get; init; }

		public Context GetChildContext ( TypeInfo type, Dictionary<MemberInfo, object?>? memberValues = null ) {
			return this with {
				ParentContext = this,
				TypeInfo = type,
				MemberValues = memberValues,
				Offset = type.IsAnchor ? Reader.Stream.Position : Offset
			};
		}

		public T? GetRef<T> ( string name ) {
			object? data;

			if ( MemberValues?.Keys.FirstOrDefault( x => x.Name == name ) is MemberInfo member ) {
				data = MemberValues[member];
			}
			else if ( TypeInfo.Type.GetMethod( name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ) is MethodInfo method ) {
				data = method.Invoke( null, method.GetParameters().Select( x => {
					var type = x.ParameterType;
					var matching = MemberValues!.Where( x => memberType( x.Key ) == type );
					var name = x.Name!.ToLower();
					var candidates = matching.Where( x => x.Key.Name.ToLower() == name ).Concat( matching ).Select( x => x.Value );
					if ( !candidates.Any() ) {
						var implicits = new object[] { this, Reader };
						candidates = implicits.Where( x => x.GetType() == type ).Concat( Dependencies.Where( x => x.Key == type ).Select( x => x.Value ) );
					}

					return candidates.FirstOrDefault();
				} ).ToArray() );
			}
			else {
				throw new Exception( "Target member not found" );
			}

			if ( data is T value )
				return value;

			if ( data is null ) {
				return default;
			}

			if ( data is IConvertible d1 && default(T) is IConvertible d2 ) {
				return (T)d1.ToType(typeof(T), CultureInfo.InvariantCulture)!;
			}

			var op = data.GetType().GetMethods( BindingFlags.Static | BindingFlags.Public )
				.Where( x => x.Name is "op_Implicit" or "op_Explicit" && x.ReturnType == typeof( T ) )
				.FirstOrDefault();

			if ( op == null )
				throw new Exception( "Target member is not valid" );

			return (T)op.Invoke( null, new[] { data } )!;
		}

		public bool HasDependencies ( IEnumerable<Type> dependencies ) {
			foreach ( var i in dependencies ) {
				if ( !Dependencies.ContainsKey( i ) )
					return false;
			}

			return true;
		}
	}

	public struct TypeInfo {
		public SizeAttribute? Size;
		public DataOffsetAttribute? Offset;
		public TypeSelectorAttribute? TypeSelector;
		public TypeSelectorAttribute? AbstractTypeSelector;
		public ParseWithAttribute? ParseWith;
		public bool IsAnchor;
		public bool IsDependency;
		public Type Type;

		public TypeInfo ( Type type ) {
			Type = type;
			IsAnchor = Type.GetCustomAttribute<OffsetAnchorAttribute>( inherit: true ) != null;
			IsDependency = Type.GetCustomAttribute<ParserDependencyAttribute>( inherit: true ) != null;
			AbstractTypeSelector = Type.GetCustomAttribute<TypeSelectorAttribute>( inherit: false );
		}

		public TypeInfo ( MemberInfo member ) : this( memberType( member ), member ) { }

		public TypeInfo ( Type type, MemberInfo member ) : this( type ) {
			Size = member.GetCustomAttribute<SizeAttribute>();
			Offset = member.GetCustomAttribute<DataOffsetAttribute>();
			TypeSelector = member.GetCustomAttribute<TypeSelectorAttribute>();
			ParseWith = member.GetCustomAttribute<ParseWithAttribute>();
			IsDependency |= member.GetCustomAttribute<ParserDependencyAttribute>( inherit: true ) != null;
		}

		public TypeInfo SubstituteType ( Type type ) {
			Type = type;
			IsAnchor = type.GetCustomAttribute<OffsetAnchorAttribute>( inherit: true ) != null;
			IsDependency = type.GetCustomAttribute<ParserDependencyAttribute>( inherit: true ) != null;
			AbstractTypeSelector = type.GetCustomAttribute<TypeSelectorAttribute>( inherit: false );
			TypeSelector = null;

			return this;
		}

		public static implicit operator TypeInfo ( Type type ) => new( type );
		public static implicit operator TypeInfo ( MemberInfo member ) => new( member );
	}

	public struct DependentTask {
		public required Type[] Dependencies;
		public required long Position;
		public required Action Action;
	}
}
