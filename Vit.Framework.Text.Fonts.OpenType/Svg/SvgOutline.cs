using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public static class SvgOutline {
	public static Outlines.SvgOutline Parse ( ReadOnlySpan<byte> data ) {
		elements ??= new() {
			["svg"] = new SvgRoot(),
			["g"] = new Group(),
			["circle"] = new Circle(),
			["ellipse"] = new Ellipse(),
			["path"] = new Path()
		};

		Outlines.SvgOutline outline = new();
		ByteString byteData = new() { Bytes = data };
		parse( ref byteData, new Context { Outline = outline, Depth = 0, Matrix = Matrix3<double>.Identity } );
		return outline;
	}

	static void parse ( ref ByteString data, Context context ) {
		assert( getNextTagType( ref data ) == ElementType.TagOpen );
		data = data.Slice( 1 );
		parseName( ref data, out var tagName );
		var element = elements[tagName.ToString()];
		element.Open( ref context );

		ElementType type;
		while ( (type = getNextAttributeType( ref data, out var name )) == ElementType.AttributeName ) {
			parseString( ref data, out var value );
			if ( !element.SetAttribute( ref context, name, value ) ) {
				throw new InvalidDataException();
			}
		}
		element.Close( ref context );

		if ( type == ElementType.TagSelfEnd ) {
			return;
		}

		assert( type == ElementType.TagEnd );

		while ( (type = getNextTagType( ref data )) == ElementType.TagOpen ) {
			parse( ref data, context with { Depth = context.Depth + 1 } );
		}

		assert( type == ElementType.TagClose );
		data = data.Slice( 2 );
		parseName( ref data, out var closeTagName );
		assert( tagName == closeTagName );
		assert( getNextAttributeType( ref data, out _ ) == ElementType.TagEnd );
	}

	static ElementType getNextTagType ( scoped ref ByteString data ) {
		while ( data.Length != 0 && char.IsWhiteSpace( data[0] ) ) {
			data = data.Slice( 1 );
		}

		if ( data.Length == 0 ) {
			return ElementType.EOF;
		}

		var first = data[0];
		if ( first == '<' ) {
			ElementType type;
			if ( data[1] == '/' ) {
				type = ElementType.TagClose;
			}
			else {
				type = ElementType.TagOpen;
			}

			return type;
		}

		throw new InvalidDataException( "Expected tag" );
	}
	static void parseName ( scoped ref ByteString data, out ByteString name ) {
		int length = 0;
		name = data;
		while ( char.IsAsciiLetterOrDigit( data[0] ) || data[0] == '-' ) {
			data = data.Slice( 1 );
			length++;
		}
		name = name.Slice( 0, length );
	}
	static ElementType getNextAttributeType ( scoped ref ByteString data, out ByteString name ) {
		while ( data.Length != 0 && char.IsWhiteSpace( data[0] ) ) {
			data = data.Slice( 1 );
		}

		var first = data[0];

		if ( first == '/' ) {
			data = data.Slice( 2 );
			name = default;
			return ElementType.TagSelfEnd;
		}

		if ( first == '>' ) {
			data = data.Slice( 1 );
			name = default;
			return ElementType.TagEnd;
		}

		parseName( ref data, out name );
		assert( data[0] == '=' );
		data = data.Slice( 1 );
		assert( data[0] == '"' );
		return ElementType.AttributeName;
	}
	static void parseString ( scoped ref ByteString data, out ByteString value ) {
		data = data.Slice( 1 );
		int length = 0;
		value = data;

		bool isEscaped = false;
		while ( true ) {
			var c = data[0];
			data = data.Slice( 1 );

			if ( isEscaped ) {
				isEscaped = false;
				length++;
			}
			else if ( c == '\\' ) {
				isEscaped = true;
				length++;
			}
			else if ( c == '"' ) {
				break;
			}
			else {
				length++;
			}
		}

		value = value.Slice( 0, length );
	}

	static void assert ( bool value, [CallerArgumentExpression(nameof(value))] string expr = null! ) {
		if ( !value )
			throw new InvalidDataException( $"Expected {expr} to be true, but it was not." );
	}

	enum ElementType {
		/// <summary>
		/// Next element is `&lt;EOF&gt;` (value is undefined)
		/// </summary>
		EOF,
		/// <summary>
		/// Next element is `&lt;` (value is the tag name)
		/// </summary>
		TagOpen,
		/// <summary>
		/// Next element is `&lt;/` (value is the tag name)
		/// </summary>
		TagClose,
		/// <summary>
		/// Next element is an attribute (value is the attribute name)
		/// </summary>
		AttributeName,
		/// <summary>
		/// Next element is `&gt;` (value is undefined)
		/// </summary>
		TagEnd,
		/// <summary>
		/// Next element is `/&gt;` (value is undefined)
		/// </summary>
		TagSelfEnd,
		/// <summary>
		/// Next element is a string (value is the unescaped string)
		/// </summary>
		String
	}

	public ref struct Context {
		public required Outlines.SvgOutline Outline;
		public required int Depth;
		public required Matrix3<double> Matrix;
	}

	[ThreadStatic, NotNull, MaybeNull]
	static Dictionary<string, SvgElement> elements;
}
