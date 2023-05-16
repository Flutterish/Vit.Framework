﻿using Vit.Framework.Mathematics.SourceGen;
using Vit.Framework.Mathematics.SourceGen.Layout;

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

for ( int x = 2; x <= 4; x++ ) {
	for ( int y = 2; y <= 4; y++ ) {
		matrix.Apply( (x, y) );
	}
}

path = "./../../../../Vit.Framework.Graphics.TwoD/Layout";
var flowSize = new FlowSizeTemplate { Path = path };
var flowPoint = new FlowPointTemplate { Path = path };
var flowVector = new FlowVectorTemplate { Path = path };
var flowAxes = new FlowAxesTemplate { Path = path };
flowSize.Apply( 2 );
flowPoint.Apply( 2 );
flowVector.Apply( 2 );
flowAxes.Apply( 2 );