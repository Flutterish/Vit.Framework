using System.Globalization;
using System.Reflection;

namespace Vit.Framework.Parsing.Binary;

public static class BinaryFileParser { // TODO some way to parse { length, data[] <- unsized, total byte count = length }
	static readonly Dictionary<TypeInfo, Func<Context, ParsingResult>> parsers = new() { };

	public static T Parse<T> ( EndianCorrectingBinaryReader reader ) {
		var result = parse( new Context {
			Reader = reader,
			Dependencies = new(),
			Cache = new()
		}, typeof( T ) );

		if ( !result.IsCompleted )
			throw new Exception( "Parsing failed" );

		return (T)result.Result!;
	}

	public static T Parse<T> ( Context context ) {
		var result = parse( context, typeof( T ) );

		if ( !result.IsCompleted )
			throw new Exception( "Parsing failed" );

		return (T)result.Result!;
	}

	static ParsingResult parse ( Context context, TypeInfo type ) {
		if ( type.DependsOn is ParsingDependsOnAttribute deps ) {
			if ( !context.HasDependencies( deps.Dependencies ) )
				return new ParsingResult( context, new() { deps.Dependencies }, ctx => parse( ctx, type ) );
		}

		static ParsingResult compute ( Context context, TypeInfo type, Func<Context, ParsingResult> parser ) {
			ParsingResult value;
			
			if ( type.Offset is DataOffsetAttribute offset ) {
				var startingOffset = context.Reader.Stream.Position;
				context.Reader.Stream.Position = offset.GetValue( context );
				value = parser( context );
				context.Reader.Stream.Position = startingOffset;
			}
			else {
				value = parser( context );
			}

			if ( !value.IsCompleted ) {
				foreach ( var i in value.Dependencies! ) {
					if ( context.HasDependencies( i ) ) {
						value = value.Continue();
						return value;
					}
				}
			}

			if ( value.IsCompleted && type.IsDependency )
				context.Dependencies[type.Type] = value.Result;

			return value;
		}

		if ( type.TypeSelector is TypeSelectorAttribute typeSelector ) {
			var newType = typeSelector.GetValue( context );
			if ( newType == null )
				return new ParsingResult( type.Type.IsValueType ? Activator.CreateInstance( type.Type ) : null );
			return parse( context, type.SubstituteType( newType ) );
		}

		if ( type.ParseWith is ParseWithAttribute parseWith ) {
			return new ParsingResult( parseWith.GetValue( context ) );
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

	static Func<Context, ParsingResult>? makePrimitiveParser ( TypeInfo type ) {
		if ( !type.Type.IsPrimitive )
			return null;

		static Func<Context, ParsingResult> generator<T> () where T : unmanaged, IConvertible {
			return ctx => new ParsingResult( ctx.Reader.Read<T>() );
		}

		return apply( type.Type, generator<int> );
	}

	static Func<Context, ParsingResult>? makeStructureParser ( TypeInfo type ) {
		if ( !type.Type.IsValueType && !type.Type.IsClass )
			return null;

		var baseMembers = type.Type.GetMembers( BindingFlags.Public | BindingFlags.Instance )
			.Where( x => x is FieldInfo or PropertyInfo { CanWrite: true } ).ToArray();

		return ctx => {
			Dictionary<MemberInfo, object?> memberValues = new();
			var childContext = ctx.GetChildContext( type, memberValues );
			var members = baseMembers;

			ParsingResult processChildClass ( TypeInfo type ) {
				var previousMember = new ParsingResult() { IsCompleted = true };
				foreach ( var i in members ) {
					var last = previousMember;
					previousMember = ParsingResult.CreateDependantTask(
						ctx,
						new[] { last },
						last => {
							var childTypeInfo = new TypeInfo( i );
							var member = parse( childContext, childTypeInfo );
							return ParsingResult.CreateDependantTask(
								ctx,
								new[] { member },
								member => {
									var value = member[0].Result;
									memberValues[i] = value;
									if ( childTypeInfo.Cache != null ) {
										childContext.Cache.Add( childTypeInfo.Type, value );
									}
									return member[0];
								}
							);
						}
					);
				}

				return ParsingResult.CreateDependantTask(
					ctx,
					new[] { previousMember },
					previousMember => {
						if ( type.AbstractTypeSelector is TypeSelectorAttribute typeSelector ) {
							var newType = typeSelector.GetValue( childContext );
							if ( newType == null )
								return new ParsingResult( Activator.CreateInstance( type.Type ) );

							childContext = childContext with { TypeInfo = newType }; // TODO check if the new type has dependencies
							members = newType.GetMembers( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) // TODO make work for more than 1 layer at a time
								.Where( x => x is FieldInfo or PropertyInfo { CanWrite: true } ).ToArray();
							return processChildClass( newType );
						}
						else {
							return new ParsingResult( Activator.CreateInstance( type.Type ) );
						}
					}
				);
			}

			var instanceTask = processChildClass( type );
			return ParsingResult.CreateDependantTask(
				ctx,
				dependencies: new[] { instanceTask },
				instanceTask => {
					var instance = instanceTask[0].Result!;
					foreach ( var (member, value) in memberValues ) {
						setValue( member, instance, value );
					}
					return new ParsingResult( instance );
				}
			);
		};
	}

	static Func<Context, ParsingResult>? makeArrayParser ( TypeInfo type ) {
		if ( !type.Type.IsArray )
			return null;
		var arrayType = type.Type.GetElementType()!;

		static Func<Context, ParsingResult> emptyGenerator<T> () {
			return _ => new ParsingResult( Array.Empty<T>() );
		}

		if ( type.Size == null )
			return apply( arrayType, emptyGenerator<int> );

		Func<Context, ParsingResult> generator<T> () {
			return ctx => {
				var size = type.Size!.GetValue( ctx );
				var results = new ParsingResult[size];

				foreach ( ref var i in results.AsSpan() )
					i = parse( ctx, typeof(T) )!;

				return ParsingResult.CreateDependantTask( 
					ctx, 
					dependencies: results, 
					_ => new ParsingResult( results.Select( x => (T)x.Result! ).ToArray() ) 
				);
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
		public required Dictionary<Type, object?> Cache { get; init; }

		public Context GetChildContext ( TypeInfo type, Dictionary<MemberInfo, object?>? memberValues = null ) {
			return this with {
				ParentContext = this,
				TypeInfo = type,
				MemberValues = memberValues,
				Offset = type.IsAnchor ? Reader.Stream.Position : Offset,
				Cache = new()
			};
		}

		public object? Resolve ( Type type ) {
			if ( Cache.TryGetValue( type, out var value ) )
				return value;

			return ParentContext?.Resolve( type );
		}

		public T? GetRef<T> ( string name ) {
			object? data;

			if ( MemberValues?.Keys.FirstOrDefault( x => x.Name == name ) is MemberInfo member ) {
				data = MemberValues[member];
			}
			else if ( TypeInfo.Type.GetMethod( name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ) is MethodInfo method ) {
				data = method.Invoke( null, method.GetParameters().Select( x => {
					var type = x.ParameterType;
					if ( x.GetCustomAttribute<ResolveAttribute>() != null ) {
						return Resolve( type );
					}

					var matching = MemberValues!.Where( x => memberType( x.Key ) == type );
					var name = x.Name!.ToLower();
					var candidates = matching.Where( x => x.Key.Name.ToLower() == name ).Select( x => x.Value );
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
		public ParsingDependsOnAttribute? DependsOn;
		public CacheAttribute? Cache;
		public bool IsAnchor;
		public bool IsDependency;
		public Type Type;

		public TypeInfo ( Type type ) {
			Type = type;
			IsAnchor = Type.GetCustomAttribute<OffsetAnchorAttribute>( inherit: true ) != null;
			IsDependency = Type.GetCustomAttribute<ParserDependencyAttribute>( inherit: true ) != null;
			AbstractTypeSelector = Type.GetCustomAttribute<TypeSelectorAttribute>( inherit: false );
			DependsOn = Type.GetCustomAttribute<ParsingDependsOnAttribute>( inherit: true );
			Cache = Type.GetCustomAttribute<CacheAttribute>( inherit: true );
		}

		public TypeInfo ( MemberInfo member ) : this( memberType( member ), member ) { }

		public TypeInfo ( Type type, MemberInfo member ) : this( type ) {
			Size = member.GetCustomAttribute<SizeAttribute>();
			Offset = member.GetCustomAttribute<DataOffsetAttribute>();
			TypeSelector = member.GetCustomAttribute<TypeSelectorAttribute>();
			ParseWith = member.GetCustomAttribute<ParseWithAttribute>();
			IsDependency |= member.GetCustomAttribute<ParserDependencyAttribute>( inherit: true ) != null;
			Cache = member.GetCustomAttribute<CacheAttribute>( inherit: true ) ?? Cache;
		}

		public TypeInfo SubstituteType ( Type type ) {
			Type = type;
			IsAnchor = type.GetCustomAttribute<OffsetAnchorAttribute>( inherit: true ) != null;
			IsDependency = type.GetCustomAttribute<ParserDependencyAttribute>( inherit: true ) != null;
			AbstractTypeSelector = type.GetCustomAttribute<TypeSelectorAttribute>( inherit: false );
			DependsOn = Type.GetCustomAttribute<ParsingDependsOnAttribute>( inherit: true );
			TypeSelector = null;

			return this;
		}

		public static implicit operator TypeInfo ( Type type ) => new( type );
		public static implicit operator TypeInfo ( MemberInfo member ) => new( member );
	}

	public struct ParsingResult {
		public bool IsCompleted;
		public object? Result;

		public Context? Context;
		public List<Type[]>? Dependencies;
		public Func<Context, ParsingResult>? Continuation;
		public long Position;

		public ParsingResult ( object? result ) {
			IsCompleted = true;
			Result = result;
		}

		public ParsingResult ( Context context, List<Type[]> dependencies, Func<Context, ParsingResult> continuation ) {
			IsCompleted = false;
			Context = context;
			Dependencies = dependencies;
			Continuation = continuation;
			Position = context.Reader.Stream.Position;
		}

		public static ParsingResult CreateDependantTask ( Context context, ParsingResult[] dependencies, Func<ParsingResult[], ParsingResult> finalizer ) {
			ParsingResult process ( Context ctx ) {
				foreach ( ref var i in dependencies.AsSpan() ) {
					if ( !i.IsCompleted )
						i = i.Continue();
				}

				if ( !dependencies.All( x => x.IsCompleted ) ) {
					return new ParsingResult( ctx, dependencies.Where( x => !x.IsCompleted ).SelectMany( x => x.Dependencies! ).ToList(), process );
				}

				return finalizer( dependencies );
			}

			return process( context );
		}

		public ParsingResult Continue () {
			var pos = Context!.Reader.Stream.Position;
			Context!.Reader.Stream.Position = Position;
			var result = Continuation!.Invoke( Context );
			Context!.Reader.Stream.Position = pos;

			return result;
		}
	}
}
