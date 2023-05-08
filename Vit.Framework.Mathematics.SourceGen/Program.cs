using Vit.Framework.Mathematics.SourceGen;

var path = "./../../../../Vit.Framework/Mathematics";
var axis = new AxesTemplate { Path = path };
var point = new PointTemplate { Path = path };
var size = new SizeTemplate { Path = path };
var box = new AxisAlignedBoxTemplate { Path = path };

path = "./../../../../Vit.Framework/Mathematics/LinearAlgebra";
var vector = new VectorTemplate { Path = path };
var matrix = new MatrixTemplate() { Path = path };

for ( int i = 1; i <= 4; i++ ) {
	axis.Apply( i );
	point.Apply( i );
	size.Apply( i );
	box.Apply( i );
	vector.Apply( i );
}

for ( int i = 2; i <= 4; i++ ) {
	matrix.Apply( (i, i) );
}