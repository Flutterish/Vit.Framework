using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class MaskedFragment {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Masked Fragment" };
	static SpirvBytecode? spirv;

	public const string IncludeGetMaskingAlpha = @"
		const uint OP_TEST = 1;
		const uint OP_CONST_TRUE = 2;
		const uint OP_CONST_FALSE = 3;
		const uint OP_AND = 4;
		const uint OP_OR = 5;
		const uint OP_NOT = 6;
		const uint OP_EXCEPT = 7;
		const uint OP_XOR = 8;
		const uint OP_IF = 9;
		struct Instruction {
			uint op;
			uint[3] args;
		};
		struct MaskingParams {
			mat3 toMaskingSpace;
			vec2[4] radii;
			float[4] exponents;
		};
		layout(binding = 1, set = 0) readonly buffer InstructionBuffer {
			Instruction instructions[];
		} instructionBuffer;
		layout(binding = 2, set = 0) readonly buffer ParamBuffer {
			MaskingParams params[];
		} paramBuffer;

		float maskingTest ( uint maskingPtr, vec2 inModelSpace ) {
			MaskingParams params = paramBuffer.params[(maskingPtr - 6)/6];
			vec2 maskingSpace = (params.toMaskingSpace * vec3(inModelSpace, 1)).xy;
			uint index = maskingSpace.x < 0 ? 0 : 1;
			index += maskingSpace.y < 0 ? 2 : 0;
			maskingSpace = abs(maskingSpace);
			vec2 radius = params.radii[index];
			float exponent = params.exponents[index];

			float value;
			if ( maskingSpace.x <= 1 - radius.x || maskingSpace.y <= 1 - radius.y ) {
				value = max( maskingSpace.x, maskingSpace.y );
			}
			else {
				value = pow( (maskingSpace.x - 1 + radius.x) / radius.x, exponent ) + pow( (maskingSpace.y - 1 + radius.y) / radius.y, exponent );
			}

			return value <= 1 ? 1 : 0;
		}

		struct InstructionTracker {
			uint op;
			uint argsNeeded;
			uint argsPushed;
		};

		float getMaskingAlpha ( uint maskingPtr, vec2 inModelSpace ) {
			if ( maskingPtr == 0 )
				return 1;

			const uint STACK_SIZE = 16;
			float[STACK_SIZE] argumentStack;
			uint[STACK_SIZE] argumentPtrStack;
			InstructionTracker[STACK_SIZE] instructionStack;

			uint instructionStackPtr = 0;
			uint argumentStackPtr = 0;
			uint argumentPtrStackPtr = 1;
			argumentPtrStack[0] = maskingPtr;

			float value = 0;
			while ( argumentPtrStackPtr != 0 || instructionStackPtr != 0 ) {
				if ( instructionStackPtr != 0 ) {
					InstructionTracker currentInstruction = instructionStack[instructionStackPtr - 1];
					if ( currentInstruction.argsNeeded == currentInstruction.argsPushed ) {
						if ( currentInstruction.op == OP_AND ) {
							value = argumentStack[--argumentStackPtr];
							value *= argumentStack[--argumentStackPtr];
							if ( currentInstruction.argsNeeded == 3 )
								value *= argumentStack[--argumentStackPtr];
						}
						else if ( currentInstruction.op == OP_OR ) {
							value = argumentStack[--argumentStackPtr];
							value = max( value, argumentStack[--argumentStackPtr] );
							if ( currentInstruction.argsNeeded == 3 )
								value = max( value, argumentStack[--argumentStackPtr] );
						}
						else if ( currentInstruction.op == OP_NOT ) {
							value = 1 - argumentStack[--argumentStackPtr];
						}
						else if ( currentInstruction.op == OP_EXCEPT ) {
							value = argumentStack[--argumentStackPtr];
							value -= argumentStack[--argumentStackPtr];
							if ( currentInstruction.argsNeeded == 3 )
								value -= argumentStack[--argumentStackPtr];
							value = max(value, 0);
						}
						else if ( currentInstruction.op == OP_XOR ) {
							value = argumentStack[--argumentStackPtr];
							value = abs(value - argumentStack[--argumentStackPtr]);
							if ( currentInstruction.argsNeeded == 3 )
								value = abs( value - argumentStack[--argumentStackPtr]);
						}
						else { // if ( currentInstruction.op == OP_IF ) {
							value = argumentStack[--argumentStackPtr];
							float ifTrue = argumentStack[--argumentStackPtr];
							float ifFalse = argumentStack[--argumentStackPtr];
							value = value * ifTrue + (1 - value) * ifFalse;
						}

						if ( --instructionStackPtr == 0 )
							return value;

						argumentStack[argumentStackPtr++] = value;
						instructionStack[instructionStackPtr - 1].argsPushed++;
						continue;
					}
				}

				maskingPtr = argumentPtrStack[--argumentPtrStackPtr];
				Instruction instruction = instructionBuffer.instructions[maskingPtr];
				if ( instruction.op == OP_TEST ) {
					value = maskingTest( maskingPtr, inModelSpace );
				}
				else if ( instruction.op == OP_CONST_TRUE ) {
					value = 1;
				}
				else if ( instruction.op == OP_CONST_FALSE ) {
					value = 0;
				}
				else {
					uint argsNeeded = 0;
					for ( uint i = 0; i < 3; i++ ) {
						if ( instruction.args[i] != 0 ) {
							argumentPtrStack[argumentPtrStackPtr++] = instruction.args[i];
							argsNeeded++;
						}
					}

					instructionStack[instructionStackPtr++] = InstructionTracker( instruction.op, argsNeeded, 0 );
					continue;
				}

				if ( instructionStackPtr == 0 )
					return value;

				argumentStack[argumentStackPtr++] = value;
				instructionStack[instructionStackPtr - 1].argsPushed++;
			}

			return argumentStack[argumentStackPtr - 1];
		}
	";

	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec2 inUv;
		layout(location = 1) in vec2 inModelSpace;

		layout(location = 0) out vec4 outColor;

		layout(binding = 1, set = 1) uniform sampler2D texSampler;
		layout(binding = 0, set = 1) uniform Uniforms {
			mat3 model;
			vec4 tint;
			uint maskingPtr;
		} uniforms;

		" + IncludeGetMaskingAlpha + @"

		void main () {
			outColor = texture( texSampler, inUv ) * uniforms.tint;
			float maskingAlpha = getMaskingAlpha( uniforms.maskingPtr, inModelSpace );
			outColor *= maskingAlpha; // multiply the whole thing because we use premultiplied alpha
		}
	", ShaderLanguage.GLSL, ShaderPartType.Fragment );
}
