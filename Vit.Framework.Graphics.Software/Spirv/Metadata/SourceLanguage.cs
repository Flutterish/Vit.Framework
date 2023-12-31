﻿namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public enum SourceLanguage : uint {
	Unknown = 0,
	ESSL = 1,
	GLSL = 2,
	OpenCL_C = 3,
	OpenCL_CPP = 4,
	HLSL = 5,
	CPP_for_OpenCL = 6,
	SYCL = 7
}
