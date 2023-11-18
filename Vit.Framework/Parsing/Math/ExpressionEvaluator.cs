using System.Globalization;
using System.Text.RegularExpressions;
using Vit.Framework.Collections;

namespace Vit.Framework.Parsing.Math;

public static class ExpressionEvaluator {
	[ThreadStatic]
	static List<Token>? tokens;

	public static unsafe double? Evaluate ( string expression, IFormatProvider? format = null ) {
		format ??= CultureInfo.InvariantCulture;

		var numberFormat = (NumberFormatInfo?)format.GetFormat( typeof( NumberFormatInfo ) ) ?? NumberFormatInfo.InvariantInfo;

		tokens ??= new( 64 );
		var span = expression.AsSpan();

		if ( !tokenize( span, tokens, numberFormat ) ) {
			tokens.Clear();
			return null;
		}

		var ast = Parse( tokens, span, format );
		tokens.Clear();
		if ( ast == null ) {
			return null;
		}

		return ast.Evaluate();
	}

	const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Singleline;
	static Regex symbolRegex = new( "^[a-z]+", regexOptions );
	static bool tokenize ( ReadOnlySpan<char> span, List<Token> tokens, NumberFormatInfo numberFormat ) {
		string decimalSeparator = Regex.Escape( numberFormat.NumberDecimalSeparator );
		string digitGroupSeparator = Regex.Escape( numberFormat.NumberGroupSeparator );

		string digitSequenceRegex = $"[0-9]+({digitGroupSeparator}[0-9]+)*";
		string numberRegex = $"^(({digitSequenceRegex}({decimalSeparator}{digitSequenceRegex})?)|({decimalSeparator}{digitSequenceRegex}))";

		for ( int i = 0; i < span.Length; ) {
			var c = span[i];

			if ( c is '/' or '*' or '+' or '-' or '^' or '(' or ')' ) {
				tokens.Add( new() {
					Type = c is '(' ? TokenType.OpenParenthesis : c is ')' ? TokenType.CloseParenthesis : TokenType.Operator,
					Length = 1,
					Offset = i
				} );

				i++;
				continue;
			}

			if ( char.IsWhiteSpace( c ) ) {
				i++;
				continue;
			}

			var symbolMatch = symbolRegex.EnumerateMatches( span[i..] );
			if ( symbolMatch.MoveNext() ) {
				var match = symbolMatch.Current;
				tokens.Add( new() {
					Type = TokenType.Symbol,
					Offset = i,
					Length = match.Length
				} );

				i += match.Length;
				continue;
			}

			var numberMatch = Regex.EnumerateMatches( span[i..], numberRegex, regexOptions );
			if ( numberMatch.MoveNext() ) {
				var match = numberMatch.Current;
				tokens.Add( new() {
					Type = TokenType.Number,
					Offset = i,
					Length = match.Length
				} );

				i += match.Length;
				continue;
			}

			// parsing error
			return false;
		}

		return true;
	}

	struct Token {
		public required TokenType Type;
		public required int Offset;
		public required int Length;

		public ReadOnlySpan<char> Slice ( ReadOnlySpan<char> source )
			=> source.Slice( Offset, Length );

		public override string ToString () {
			return $"{Type} @ {Offset}:{Offset+Length}";
		}
	}

	enum TokenType {
		Number,
		Symbol,
		OpenParenthesis,
		CloseParenthesis,
		Operator
	}

	static Expression? Parse ( List<Token> tokens, ReadOnlySpan<char> span, IFormatProvider format ) {
		int i = 0;

		Token? nextToken () {
			if ( tokens.Count > i ) {
				return tokens[i++];
			}

			return null;
		}
		Expression? parseExpression ( ReadOnlySpan<char> span ) {
			return (Expression?)parseUnaryOperator( span )
				?? (Expression?)parseParenthesisedExpression( span )
				?? (Expression?)parseFunctionCall( span )
				?? (Expression?)parseOperatorGroup( span )
				?? (Expression?)parseSymbolLiteral( span )
				?? (Expression?)parseNumberLiteral( span );
		}
		Expression? parseSingleExpression ( ReadOnlySpan<char> span ) {
			return (Expression?)parseUnaryOperator( span )
				?? (Expression?)parseParenthesisedExpression( span )
				?? (Expression?)parseFunctionCall( span )
				?? (Expression?)parseSymbolLiteral( span )
				?? (Expression?)parseNumberLiteral( span );
		}
		UnaryOperator? parseUnaryOperator ( ReadOnlySpan<char> span ) {
			var stack = i;
			if ( nextToken() is not { Type: TokenType.Operator } op || op.Slice( span ) is not (['+'] or ['-']) ) {
				i = stack;
				return null;
			}

			if ( parseSingleExpression( span ) is not Expression arg ) {
				i = stack;
				return null;
			}

			return new() {
				Type = op.Slice( span ) switch {
					['-'] => UnaryOperatorType.UnaryMinus,
					['+'] or _ => UnaryOperatorType.UnaryPlus
				},
				Expression = arg
			};
		}
		Expression? parseOperatorGroup ( ReadOnlySpan<char> span ) {
			List<Expression> expressions = new();
			List<BinaryOperatorType> operators = new();
			var stack = i;

			if ( parseSingleExpression( span ) is not Expression first ) {
				i = stack;
				return null;
			}
			expressions.Add( first );

			bool isLastNumber = first is NumberLiteral;
			while ( true ) {
				// cases:
				// <expression> <op> <expression> (ex: 10 + 10)
				// <expression> <expression> (ex: (1+2)(3+4))
				// <number> <non-number> (ex: 10(1 + 2), 10pi, 10sin(1.23), (1 + 2)10)
				var opStack = i;
				if ( nextToken() is { Type: TokenType.Operator } op ) {
					if ( parseSingleExpression( span ) is not Expression next ) {
						i = stack;
						return null;
					}

					operators.Add( op.Slice( span ) switch {
						['+'] => BinaryOperatorType.Add,
						['-'] => BinaryOperatorType.Substract,
						['*'] => BinaryOperatorType.Multiply,
						['/'] => BinaryOperatorType.Divide,
						['^'] or _ => BinaryOperatorType.Exponentiate
					} );
					expressions.Add( next );
					isLastNumber = next is NumberLiteral;
				}
				else {
					i = opStack;

					if ( parseSingleExpression( span ) is not Expression next ) {
						break;
					}
					var prev = expressions[^1];
					var isNumber = next is NumberLiteral;
					if ( isLastNumber && isNumber ) {
						i = stack;
						return null;
					}

					isLastNumber = isNumber;
					expressions[^1] = new BinaryOperator { Lhs = prev, Rhs = next, Type = BinaryOperatorType.Multiply };
				}
			}

			while ( expressions.Count != 1 ) {
				var maxPrecedence = operators.Max( x => operatorPrecedence[x] );
				for ( int i = 0; i < operators.Count; ) {
					var op = operators[i];
					var precedence = operatorPrecedence[op];
					if ( precedence != maxPrecedence ) {
						i++;
						continue;
					}

					expressions[i] = new BinaryOperator {
						Lhs = expressions[i],
						Rhs = expressions[i + 1],
						Type = op
					};
					expressions.RemoveAt( i + 1 );
					operators.RemoveAt( i );
				}
			}

			return expressions[0];
		}
		FunctionCall? parseFunctionCall ( ReadOnlySpan<char> span ) {
			var stack = i;
			if ( nextToken() is not { Type: TokenType.Symbol } name ) {
				i = stack;
				return null;
			}
			if ( !functions.TryGetValue( name.Slice( span ), out var function ) ) {
				i = stack;
				return null;
			}
			if ( nextToken() is not { Type: TokenType.OpenParenthesis } ) {
				i = stack;
				return null;
			}
			if ( parseExpression( span ) is not Expression arg ) {
				i = stack;
				return null;
			}
			if ( nextToken() is not { Type: TokenType.CloseParenthesis } ) {
				i = stack;
				return null;
			}

			return new() {
				Function = function,
				Argument = arg
			};
		}
		ParenthesisedExpression? parseParenthesisedExpression ( ReadOnlySpan<char> span ) {
			var stack = i;
			if ( nextToken() is not { Type: TokenType.OpenParenthesis } ) {
				i = stack;
				return null;
			}
			if ( parseExpression( span ) is not Expression expr ) {
				i = stack;
				return null;
			}
			if ( nextToken() is not { Type: TokenType.CloseParenthesis } ) {
				i = stack;
				return null;
			}
			return new() {
				Expression = expr
			};
		}
		NumberLiteral? parseNumberLiteral ( ReadOnlySpan<char> span ) {
			var stack = i;
			if ( nextToken() is not { Type: TokenType.Number } number ) {
				i = stack;
				return null;
			}

			return new() {
				Value = double.Parse( number.Slice( span ), format )
			};
		}
		SymbolLiteral? parseSymbolLiteral ( ReadOnlySpan<char> span ) {
			var stack = i;
			if ( nextToken() is not { Type: TokenType.Symbol } symbol ) {
				i = stack;
				return null;
			}
			if ( !symbols.TryGetValue( symbol.Slice( span ), out var value ) ) {
				i = stack;
				return null;
			}

			return new() {
				Value = value
			};
		}

		var exp = parseExpression( span );
		return i == tokens.Count ? exp : null;
	}

	abstract class AstNode {

	}
	abstract class Expression : AstNode {
		public abstract double Evaluate ();
	}

	class BinaryOperator : Expression {
		public required BinaryOperatorType Type;
		public required Expression Lhs;
		public required Expression Rhs;

		public override double Evaluate () {
			var left = Lhs.Evaluate();
			var right = Rhs.Evaluate();

			return Type switch {
				BinaryOperatorType.Add => left + right,
				BinaryOperatorType.Substract => left - right,
				BinaryOperatorType.Multiply => left * right,
				BinaryOperatorType.Divide => left / right,
				BinaryOperatorType.Exponentiate or _ => double.Pow( left, right ),
			};
		}

		public override string ToString () {
			return $"({Lhs}{Type switch {
				BinaryOperatorType.Add => " + ",
				BinaryOperatorType.Substract => " - ",
				BinaryOperatorType.Multiply => " * ",
				BinaryOperatorType.Divide => " / ",
				BinaryOperatorType.Exponentiate or _ => "^",
			}}{Rhs})";
		}
	}
	static readonly Dictionary<BinaryOperatorType, int> operatorPrecedence = new() {
		[BinaryOperatorType.Add] = 1,
		[BinaryOperatorType.Substract] = 1,
		[BinaryOperatorType.Multiply] = 2,
		[BinaryOperatorType.Divide] = 2,
		[BinaryOperatorType.Exponentiate] = 3
	};
	enum BinaryOperatorType {
		Add,
		Substract,
		Multiply,
		Divide,
		Exponentiate
	}

	class FunctionCall : Expression {
		public required Func<double, double> Function;
		public required Expression Argument;

		public override double Evaluate () {
			return Function( Argument.Evaluate() );
		}

		public override string ToString () {
			return $"{Function.Method.Name}({Argument})";
		}
	}
	static readonly ByteCharPrefixTree<Func<double, double>> functions = new() {
		["sin"] = double.Sin,
		["cos"] = double.Cos,
		["tan"] = double.Tan,
		["sqrt"] = double.Sqrt
	};

	class ParenthesisedExpression : Expression {
		public required Expression Expression;

		public override double Evaluate () {
			return Expression.Evaluate();
		}

		public override string ToString () {
			return $"({Expression})";
		}
	}

	class NumberLiteral : Expression {
		public required double Value;

		public override double Evaluate () {
			return Value;
		}

		public override string ToString () {
			return $"{Value}";
		}
	}

	class SymbolLiteral : Expression {
		public required double Value;

		public override double Evaluate () {
			return Value;
		}

		public override string ToString () {
			return $"{Value}";
		}
	}
	static readonly ByteCharPrefixTree<double> symbols = new() {
		["pi"] = double.Pi,
		["tau"] = double.Tau,
		["e"] = double.E,
		["deg"] = double.Tau / 360,
		["degrees"] = double.Tau / 360
	};

	class UnaryOperator : Expression {
		public required UnaryOperatorType Type;
		public required Expression Expression;

		public override double Evaluate () {
			var arg = Expression.Evaluate();
			return Type switch {
				UnaryOperatorType.UnaryMinus => -arg,
				UnaryOperatorType.UnaryPlus or _ => arg
			};
		}

		public override string ToString () {
			return $"{Type switch {
				UnaryOperatorType.UnaryMinus => "-",
				UnaryOperatorType.UnaryPlus or _ => "",
			}}{Expression}";
		}
	}
	enum UnaryOperatorType {
		UnaryMinus,
		UnaryPlus
	}
}
