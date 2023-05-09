using Vit.Framework.Collections;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics.GeometricAlgebra.Generic;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.SourceGen;

public class MatrixTemplate : ClassTemplate<(int rows, int columns)> {
	public override string GetTypeName ( (int rows, int columns) data ) {
		return data.rows == data.columns ? $"Matrix{data.rows}" : $"Matrix{data.columns}x{data.rows}";
	}

	public override string GetFullTypeName ( (int rows, int columns) data ) {
		return $"{GetTypeName( data )}<T>";
	}

	protected override void GenerateUsings ( (int rows, int columns) data, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
		sb.AppendLine( "using System.Runtime.InteropServices;" );
		sb.AppendLine( "using Vit.Framework.Mathematics.LinearAlgebra.Generic;" );
		sb.AppendLine( "using Vit.Framework.Memory;" );
	}

	protected override string Namespace => "Vit.Framework.Mathematics.LinearAlgebra";
	protected override string ClassType => "struct";

	protected override void GenerateInterfaces ( (int rows, int columns) data, SourceStringBuilder sb ) {
		sb.Append( $" where T : INumber<T>" );
	}

	string memberName ( int x, int y ) {
		return $"M{x}{y}";
	}
	protected override void GenerateClassBody ( (int rows, int columns) data, SourceStringBuilder sb ) {
		var (rows, columns) = data;
		var min = int.Min(rows, columns);
		var minIndices = Enumerable.Range( 0, min );
		var min1Indices = Enumerable.Range( 0, min - 1 );
		var rowIndices = Enumerable.Range( 0, rows );
		var columnIndices = Enumerable.Range( 0, columns );
		var nonGenericType = GetTypeName( data );
		var type = GetFullTypeName( data );

		foreach ( var y in rowIndices ) {
			sb.AppendJoin( " ", columnIndices.Select( x => $"public T {memberName(x,y)};" ) );
			sb.AppendLine();
		}

		sb.AppendLine();
		sb.AppendLine( "#nullable disable" );
		sb.AppendLine( $"public {nonGenericType} ( ReadOnlySpan2D<T> data ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "data.CopyTo( this.AsSpan2D() );" );
		}
		sb.AppendLine( "}" );
		sb.AppendLine( "#nullable restore" );


		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType} (" );
		sb.Append( "\t" );
		foreach ( var y in rowIndices ) {
			foreach ( var x in columnIndices ) {
				sb.Append( $"T {memberName( x, y ).ToLower()}" );
				if ( x != columns - 1 || y != rows - 1 )
					sb.Append( ", " );
			}
			if ( y != rows - 1 ) {
				sb.AppendLine();
				sb.Append( "\t" );
			}
		}
		sb.AppendLine();
		sb.AppendLine( ") {" );
		using ( sb.Indent() ) {
			foreach ( var y in rowIndices ) {
				foreach ( var x in columnIndices ) {
					sb.Append( $"{memberName( x, y )} = {memberName( x, y ).ToLower()}; " );
				}
				sb.AppendLine();
			}
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref {memberName(0, 0)}, {columns * rows} );" );
		sb.AppendLine( $"public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref {memberName( 0, 0 )}, {columns * rows} );" );
		sb.AppendLine( $"public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), {columns}, {rows} );" );
		sb.AppendLine( $"public Span2D<T> AsSpan2D () => new( AsSpan(), {columns}, {rows} );" );

		sb.AppendLine();
		sb.AppendLine( $"public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );" );

		sb.AppendLine();
		sb.AppendLine( $"public static readonly {type} Identity = new() {{" );
		using ( sb.Indent() ) {
			sb.AppendLinePostJoin( ",", minIndices.Select( x => $"{memberName( x, x )} = T.MultiplicativeIdentity" ) );
			sb.AppendLine();
		}
		sb.AppendLine( "};" );

		var axisType = new AxesTemplate() { Path = string.Empty };
		var vectorType = new VectorTemplate() { Path = string.Empty };
		var pointType = new PointTemplate() { Path = string.Empty };
		var sizeType = new SizeTemplate() { Path = string.Empty };

		sb.AppendLine();
		sb.AppendLine( $"public static {type} CreateScale ( {axisType.GetFullTypeName(min)} axes )" );
		sb.Append( $"\t=> CreateScale( " );
		sb.AppendJoin( ", ", minIndices.Select( x => $"axes.{axisType.AxisNames[x]}" ) );
		sb.AppendLine( " );" );
		sb.Append( $"public static {type} CreateScale ( " );
		sb.AppendJoin( ", ", minIndices.Select( x => $"T {axisType.AxisNames[x].ToLower()}" ) );
		sb.AppendLine( " ) => new() {" );
		using ( sb.Indent() ) {
			sb.AppendLinePostJoin( ",", minIndices.Select( x => $"{memberName( x, x )} = {axisType.AxisNames[x].ToLower()}" ) );
			sb.AppendLine();
		}
		sb.AppendLine( "};" );
		sb.AppendLine( $"public static {type} CreateScale ( {axisType.GetFullTypeName( min - 1 )} axes )" );
		sb.Append( $"\t=> CreateScale( " );
		sb.AppendJoin( ", ", min1Indices.Select( x => $"axes.{axisType.AxisNames[x]}" ) );
		sb.AppendLine( " );" );
		sb.Append( $"public static {type} CreateScale ( " );
		sb.AppendJoin( ", ", min1Indices.Select( x => $"T {axisType.AxisNames[x].ToLower()}" ) );
		sb.AppendLine( " ) => new() {" );
		using ( sb.Indent() ) {
			sb.AppendLinePostJoin( ",", min1Indices.Select( x => $"{memberName( x, x )} = {axisType.AxisNames[x].ToLower()}" ) );
			sb.AppendLine( "," );
			sb.AppendLine( $"{memberName( min - 1, min - 1 )} = T.MultiplicativeIdentity" );
		}
		sb.AppendLine( "};" );

		sb.AppendLine();
		sb.AppendLine( $"public static {type} CreateTranslation ( {vectorType.GetFullTypeName( min - 1 )} vector )" );
		sb.Append( $"\t=> CreateTranslation( " );
		sb.AppendJoin( ", ", min1Indices.Select( x => $"vector.{vectorType.AxisNames[x]}" ) );
		sb.AppendLine( " );" );
		sb.Append( $"public static {type} CreateTranslation ( " );
		sb.AppendJoin( ", ", min1Indices.Select( x => $"T {vectorType.AxisNames[x].ToLower()}" ) );
		sb.AppendLine( " ) => new() {" );
		using ( sb.Indent() ) {
			sb.AppendLinePostJoin( ",", minIndices.Select( x => $"{memberName( x, x )} = T.MultiplicativeIdentity" ) );
			sb.AppendLine( "," );
			sb.AppendLinePostJoin( ",", min1Indices.Select( x => $"{memberName(x, min - 1)} = {vectorType.AxisNames[x].ToLower()}" ) );
			sb.AppendLine();
		}
		sb.AppendLine( "};" );

		if ( min is 2 or 3 ) {
			sb.AppendLine();
			sb.AppendLine( $"public static {type} CreateShear ( {axisType.GetFullTypeName(2)} shear )" );
			sb.AppendLine( $"\t=> CreateShear( shear.{axisType.AxisNames[0]}, shear.{axisType.AxisNames[1]} );" );

			sb.AppendLine( $"public static {type} CreateShear ( T {axisType.AxisNames[0].ToLower()}, T {axisType.AxisNames[1].ToLower()} ) => new() {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"{memberName(0, 0)} = T.MultiplicativeIdentity," );
				sb.AppendLine( $"{memberName(1, 1)} = T.MultiplicativeIdentity," );
				if ( min == 3 ) sb.AppendLine( $"{memberName(2, 2)} = T.MultiplicativeIdentity," );
				sb.AppendLine();
				sb.AppendLine( $"{memberName(0, 1)} = {axisType.AxisNames[0].ToLower()}," );
				sb.AppendLine( $"{memberName(1, 0)} = {axisType.AxisNames[1].ToLower()}" );
			}
			sb.AppendLine( "};" );
		}

		if ( min is 3 or 4 ) {
			sb.AppendLine();
			sb.AppendLine( $"public static {type} FromAxisAngle<TAngle> ( {vectorType.GetFullTypeName(3)} axis, TAngle angle ) where TAngle : IAngle<TAngle, T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"T x = axis.{vectorType.AxisNames[0]}, y = axis.{vectorType.AxisNames[1]}, z = axis.{vectorType.AxisNames[2]};" );
				sb.AppendLine( $"T sa = TAngle.Sin( angle ), ca = TAngle.Cos( angle );" );
				sb.AppendLine( $"T xx = x * x, yy = y * y, zz = z * z;" );
				sb.AppendLine( $"T xy = x * y, xz = x * z, yz = y * z;" );
				sb.AppendLine();
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"{memberName(0,0)} = xx + ca * ( T.One - xx )," );
					sb.AppendLine( $"{memberName(1,0)} = xy - ca * xy + sa * z," );
					sb.AppendLine( $"{memberName(2,0)} = xz - ca * xz - sa * y," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(0,1)} = xy - ca * xy - sa * z," );
					sb.AppendLine( $"{memberName(1,1)} = yy + ca * ( T.One - yy )," );
					sb.AppendLine( $"{memberName(2,1)} = yz - ca * yz + sa * x," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(0, 2)} = xz - ca * xz + sa * y," );
					sb.AppendLine( $"{memberName(1, 2)} = yz - ca * yz - sa * x," );
					sb.Append( $"{memberName(2, 2)} = zz + ca * ( T.One - zz )" );
					if ( min == 4 ) {
						sb.AppendLine( "," );
						sb.AppendLine();
						sb.AppendLine( $"{memberName(3,3)} = T.MultiplicativeIdentity" );
					}
					else {
						sb.AppendLine();
					}
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}

		if ( min is 2 or 3 ) {
			sb.AppendLine();
			sb.AppendLine( $"public static {type} CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );" );
				sb.AppendLine();
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"{memberName(0,0)} = cos," );
					sb.AppendLine( $"{memberName(0,1)} = -sin," );
					sb.AppendLine( $"{memberName(1,0)} = sin," );
					sb.AppendLine( $"{memberName(1,1)} = cos," );
					if ( min == 3 ) {
						sb.AppendLine();
						sb.AppendLine( $"{memberName(2,2)} = T.MultiplicativeIdentity" );
					}
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}

		if ( min == 4 ) {
			sb.AppendLine();
			sb.AppendLine( $"public static {type} CreatePerspective ( T width, T height, T nearPlaneDistance, T farPlaneDistance ) {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "var a = T.MultiplicativeIdentity / ( T.MultiplicativeIdentity - nearPlaneDistance / farPlaneDistance );" );
				sb.AppendLine( "var b = nearPlaneDistance * a;" );
				sb.AppendLine();
				sb.AppendLine( "var aspectRatio = width / height;" );
				sb.AppendLine();
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"{memberName(0,0)} = aspectRatio > T.MultiplicativeIdentity ? height / width : T.MultiplicativeIdentity," );
					sb.AppendLine( $"{memberName(1,1)} = aspectRatio < T.MultiplicativeIdentity ? aspectRatio : T.MultiplicativeIdentity," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(2,2)} = a," );
					sb.AppendLine( $"{memberName(2,3)} = -b," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(3,2)} = T.MultiplicativeIdentity" );
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static {nonGenericType}<TNumber> CreateLookAt<TNumber> ( {vectorType.GetTypeName(3)}<TNumber> direction, {vectorType.GetTypeName(3)}<TNumber> upDirection ) " );
			sb.AppendLine( "\twhere TNumber : IFloatingPointIeee754<TNumber> " );
			sb.AppendLine( "{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "var forward = direction.Normalized();" );
				sb.AppendLine( "var right = upDirection.Cross( forward );" );
				sb.AppendLine( "if ( right.LengthSquared <= TNumber.Epsilon ) {" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"if ( TNumber.Abs( upDirection.{vectorType.AxisNames[0]} ) < TNumber.Abs( upDirection.{vectorType.AxisNames[1]} ) )" );
					using ( sb.Indent() )
						sb.AppendLine( $"upDirection.{vectorType.AxisNames[0]} += TNumber.One;" );
					sb.AppendLine( "else" );
					using ( sb.Indent() )
						sb.AppendLine( $"upDirection.{vectorType.AxisNames[1]} += TNumber.One;" );
					sb.AppendLine();
					sb.AppendLine( "right = upDirection.Cross( forward );" );
				}
				sb.AppendLine( "}" );
				sb.AppendLine( "right.Normalize();" );
				sb.AppendLine( "var up = forward.Cross( right );" );
				sb.AppendLine();
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"{memberName(0,2)} = forward.{vectorType.AxisNames[0]}," );
					sb.AppendLine( $"{memberName(1,2)} = forward.{vectorType.AxisNames[1]}," );
					sb.AppendLine( $"{memberName(2,2)} = forward.{vectorType.AxisNames[2]}," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(0,1)} = up.{vectorType.AxisNames[0]}," );
					sb.AppendLine( $"{memberName(1,1)} = up.{vectorType.AxisNames[1]}," );
					sb.AppendLine( $"{memberName(2,1)} = up.{vectorType.AxisNames[2]}," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(0,0)} = right.{vectorType.AxisNames[0]}," );
					sb.AppendLine( $"{memberName(1,0)} = right.{vectorType.AxisNames[1]}," );
					sb.AppendLine( $"{memberName(2,0)} = right.{vectorType.AxisNames[2]}," );
					sb.AppendLine();
					sb.AppendLine( $"{memberName(3,3)} = TNumber.MultiplicativeIdentity" );
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}

		if ( min == 3 ) {
			sb.AppendLine();
			sb.AppendLine( $"public static {type} CreateViewport ( T {vectorType.AxisNames[0].ToLower()}, T {vectorType.AxisNames[1].ToLower()}, T {sizeType.AxisNames[0].ToLower()}, T {sizeType.AxisNames[1].ToLower()} ) {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"return CreateTranslation( -{vectorType.AxisNames[0].ToLower()}, -{vectorType.AxisNames[1].ToLower()} ) * CreateScale( T.MultiplicativeIdentity / {sizeType.AxisNames[0].ToLower()}, T.MultiplicativeIdentity / {sizeType.AxisNames[1].ToLower()} );" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static {nonGenericType}<TNumber> CreateLookAt<TNumber> ( {vectorType.GetTypeName(2)}<TNumber> direction ) where TNumber : IFloatingPointIeee754<TNumber> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"return {nonGenericType}<TNumber>.CreateRotation( direction.GetAngle() );" );
			}
			sb.AppendLine( "}" );
		}

		sb.AppendLine();
		sb.AppendLine( $"public {GetFullTypeName((columns, rows))} Transposed => new() {{" );
		using ( sb.Indent() ) {
			foreach ( var x in columnIndices ) {
				foreach ( var y in rowIndices ) {
					sb.AppendLine( $"{memberName(y, x)} = {memberName(x, y)}," );
				}
			}
		}
		sb.AppendLine( "};" );

		var labels = new List<string>();
		foreach ( var y in rowIndices ) {
			foreach ( var x in columnIndices ) {
				labels.Add( $"M[{( x + y * columns ).ToString().PadLeft( ( rows * columns ).ToString().Length, ' ' )}]" );
			}
		}
		var generic = Matrix<float>.GenerateLabelMatrix( null, columns, rows, labels );

		var cofactors = generic.CofactorCheckerboard();
		sb.AppendLine();
		sb.AppendLine( $"public {GetFullTypeName( (rows, columns) )} CofactorCheckerboard {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "get {" );
			using ( sb.Indent() ) {
				sb.AppendLine( "var M = AsReadOnlySpan();" );

				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					foreach ( var x in columnIndices ) {
						foreach ( var y in rowIndices ) {
							sb.Append( $"{memberName( x, y )} = " );
							appendNumber( cofactors[x, y], sb, multiline: false );
							sb.AppendLine( "," );
						}
					}
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}
		sb.AppendLine( "}" );

		if ( columns == rows ) {
			var minors = generic.GetMinors();
			sb.AppendLine();
			sb.AppendLine( $"public {GetFullTypeName( (columns, rows) )} Minors {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "get {" );
				using ( sb.Indent() ) {
					sb.AppendLine( "var M = AsReadOnlySpan();" );

					sb.AppendLine( "return new() {" );
					using ( sb.Indent() ) {
						foreach ( var y in columnIndices ) {
							foreach ( var x in rowIndices ) {
								sb.Append( $"{memberName( x, y )} = " );
								appendNumber( minors[x, y], sb, multiline: false );
								sb.AppendLine( "," );
							}
						}
					}
					sb.AppendLine( "};" );
				}
				sb.AppendLine( "}" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( "public T Determinant {" );
			using ( sb.Indent() ) {
				sb.AppendLine( "get {" );
				using ( sb.Indent() ) {
					sb.AppendLine( "var M = AsReadOnlySpan();" );
					sb.Append( "return " );
					appendNumber( generic.GetDeterminant(), sb, multiline: true );
					sb.AppendLine( ";" );
				}
				sb.AppendLine( "}" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public {GetFullTypeName( (columns, rows) )} Inversed {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "get {" );
				using ( sb.Indent() ) {
					sb.AppendLine( "var M = AsReadOnlySpan();" );
					sb.AppendLine( "var invDet = T.MultiplicativeIdentity / Determinant;" );
					var inv = generic.GetMinors().CofactorCheckerboard().Transposed();

					sb.AppendLine( "return new() {" );
					using ( sb.Indent() ) {
						foreach ( var y in columnIndices ) {
							foreach ( var x in rowIndices ) {
								sb.Append( $"{memberName( x, y )} = (" );
								appendNumber( inv[x, y], sb, multiline: false );
								sb.AppendLine( ") * invDet," );
							}
						}
					}
					sb.AppendLine( "};" );
				}
				sb.AppendLine( "}" );
			}
			sb.AppendLine( "}" );
		}

		sb.AppendLine();
		generateMultiply( data, data, sb );

		sb.AppendLine();
		generateMultiply( data, columns, vectorType, sb );
		sb.AppendLine();
		generateMultiply( data, columns, pointType, sb );

		sb.AppendLine();
		generateApply( data, columns - 1, vectorType, sb );
		sb.AppendLine();
		generateApply( data, columns - 1, pointType, sb );

		sb.AppendLine();
		sb.AppendLine( $"public static implicit operator Span2D<T> ( {GetFullTypeName( data )} matrix )" );
		sb.AppendLine( "\t=> matrix.AsSpan2D();" );
		sb.AppendLine( $"public static implicit operator ReadOnlySpan2D<T> ( {GetFullTypeName( data )} matrix )" );
		sb.AppendLine( "\t=> matrix.AsReadOnlySpan2D();" );

		sb.AppendLine();
		sb.AppendLine( "public override string ToString () {" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return Matrix<T>.ToString( AsSpan2D() );" );
		}
		sb.AppendLine( "}" );
	}

	void generateMultiply ( (int rows, int columns) left, (int rows, int columns) right, SourceStringBuilder sb ) {
		var leftLabels = new List<string>();
		var (rows, columns) = left;
		foreach ( var y in Enumerable.Range( 0, rows ) ) {
			foreach ( var x in Enumerable.Range( 0, columns ) ) {
				leftLabels.Add( $"A[{( x + y * columns ).ToString().PadLeft( ( rows * columns ).ToString().Length, ' ' )}]" );
			}
		}
		var rightLabels = new List<string>();
		(rows, columns) = right;
		foreach ( var y in Enumerable.Range( 0, rows ) ) {
			foreach ( var x in Enumerable.Range( 0, columns ) ) {
				rightLabels.Add( $"B[{( x + y * columns ).ToString().PadLeft( ( rows * columns ).ToString().Length, ' ' )}]" );
			}
		}

		var leftMatrix = Matrix<float>.GenerateLabelMatrix( null, left.columns, left.rows, leftLabels );
		var rightMatrix = Matrix<float>.GenerateLabelMatrix( null, right.columns, right.rows, rightLabels );
		var result = leftMatrix * rightMatrix;

		sb.AppendLine( $"public static {GetFullTypeName((result.Rows, result.Columns))} operator * ( {GetFullTypeName(left)} left, {GetFullTypeName(right)} right ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "var A = left.AsSpan();" );
			sb.AppendLine( "var B = right.AsSpan();" );
			sb.AppendLine();
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() ) {
				for ( int y = 0; y < result.Rows; y++ ) {
					for ( int x = 0; x < result.Columns; x++ ) {
						sb.Append( $"{memberName(x, y)} = " );
						appendNumber( result[x, y], sb, multiline: false );
						sb.AppendLine( "," );
					}
				}
			}
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	void generateMultiply ( (int rows, int columns) matrix, int length, SpanLikeTemplate template, SourceStringBuilder sb ) {
		var matrixLabels = new List<string>();
		var (rows, columns) = matrix;
		foreach ( var y in Enumerable.Range( 0, rows ) ) {
			foreach ( var x in Enumerable.Range( 0, columns ) ) {
				matrixLabels.Add( $"M[{( x + y * columns ).ToString().PadLeft( ( rows * columns ).ToString().Length, ' ' )}]" );
			}
		}
		var vecLabels = new List<string>();
		foreach ( var i in Enumerable.Range( 0, length ) ) {
			vecLabels.Add( $"V[{i}]" );
		}

		var generic = Matrix<float>.GenerateLabelMatrix( null, columns, rows, matrixLabels );
		var vec = Matrix<float>.GenerateLabelMatrix( null, length, 1, vecLabels );

		var result = vec * generic;
		sb.AppendLine( $"public {template.GetFullTypeName( length )} Apply ( {template.GetFullTypeName( length )} value )" );
		sb.AppendLine( $"\t=> value * this;" );
		sb.AppendLine( $"public static {template.GetFullTypeName( length )} operator * ( {template.GetFullTypeName( length )} left, {GetFullTypeName( matrix )} right ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "var M = right.AsSpan();" );
			sb.AppendLine( "var V = left.AsSpan();" );
			sb.AppendLine();
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() ) {
				foreach ( var i in Enumerable.Range( 0, length ) ) {
					sb.Append( $"{template.AxisNames[i]} = " );
					appendNumber( result[i, 0], sb, multiline: false );
					sb.AppendLine( "," );
				}
			}
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	void generateApply ( (int rows, int columns) matrix, int length, SpanLikeTemplate template, SourceStringBuilder sb ) {
		var matrixLabels = new List<string>();
		var (rows, columns) = matrix;
		foreach ( var y in Enumerable.Range( 0, rows ) ) {
			foreach ( var x in Enumerable.Range( 0, columns ) ) {
				matrixLabels.Add( $"M[{( x + y * columns ).ToString().PadLeft( ( rows * columns ).ToString().Length, ' ' )}]" );
			}
		}
		var vecLabels = new List<string>();
		foreach ( var i in Enumerable.Range( 0, length ) ) {
			vecLabels.Add( $"V[{i}]" );
		}

		var generic = Matrix<float>.GenerateLabelMatrix( null, columns, rows, matrixLabels );
		var vec = Matrix<float>.GenerateLabelMatrix( null, length, 1, vecLabels );
		List<MultiVector<float>> values = new( vec.Components.GetRow(0).ToArray() );
		values.Add( 1 );
		vec = new( new Span2D<MultiVector<float>>( values.AsSpan(), length + 1, 1 ) );

		var result = vec * generic;
		sb.AppendLine( $"public {template.GetFullTypeName( length )} Apply ( {template.GetFullTypeName( length )} value ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "var M = this.AsSpan();" );
			sb.AppendLine( "var V = value.AsSpan();" );
			sb.AppendLine();
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() ) {
				foreach ( var i in Enumerable.Range( 0, length ) ) {
					sb.Append( $"{template.AxisNames[i]} = " );
					appendNumber( result[i, 0], sb, multiline: false );
					sb.AppendLine( "," );
				}
			}
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	void appendNumber ( MultiVector<float> value, SourceStringBuilder sb, bool multiline ) {
		bool first = true;
		foreach ( var i in value.Components ) {
			if ( first ) {
				if ( i.Scale < 0 )
					sb.Append( "-" );
			}
			else if ( i.Scale < 0 ) {
				if ( multiline ) {
					sb.AppendLine();
					sb.Append( "\t- " );
				}
				else {
					sb.Append( " - " );
				}
			}
			else {
				if ( multiline ) {
					sb.AppendLine();
					sb.Append( "\t+ " );
				}
				else {
					sb.Append( " + " );
				}
			}
			first = false;

			sb.AppendJoin( " * ", i.Bases );
		}
	}
}
