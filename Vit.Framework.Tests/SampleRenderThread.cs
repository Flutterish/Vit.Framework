using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Input;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Parsing;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Fonts.OpenType;
using Vit.Framework.Windowing;
using Vulkan;
using Image = Vit.Framework.Graphics.Vulkan.Textures.Image;

namespace Vit.Framework.Tests;

public class SampleRenderThread : VulkanRenderThread {
	public SampleRenderThread ( Window window, Host host, string name ) : base( window, host, name ) { }

	ShaderModule vertex = null!;
	ShaderModule fragment = null!;
	ShaderSet basicShader = null!;
	Pipeline pipeline = null!;
	DeviceBuffer<float> vertexBuffer = null!;
	DeviceBuffer<uint> indexBuffer = null!;
	Sampler sampler = null!;
	struct Matrices {
		public Matrix4<float> Model;
		public Matrix4<float> View;
		public Matrix4<float> Projection;
	}
	HostBuffer<Matrices> uniforms = null!;
	Image texture = null!;
	VkClearColorValue bg;

	Font font = null!;
	long indexCount;
	protected override void Initialize () {
		base.Initialize();

		font = new OpenTypeFont( new ReopenableFileStream( @"D:\Main\Solutions\Git\fontineer\sample-fonts\CONSOLA.TTF" ) );
		font.Validate();

		vertex = Device.CreateShaderModule( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec3 inPosition;
			layout(location = 1) in vec3 inColor;
			layout(location = 2) in vec2 inTexCoord;

			layout(location = 0) out vec3 fragColor;
			layout(location = 1) out vec2 fragTexCoord;

			layout(binding = 0) uniform Matrices {
				mat4 model;
				mat4 view;
				mat4 projection;
			} matrices;

			void main() {
				gl_Position = matrices.projection * matrices.view * matrices.model * vec4(inPosition, 1.0);
				fragColor = inColor;
				fragTexCoord = inTexCoord;
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );

		fragment = Device.CreateShaderModule( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec3 fragColor;
			layout(location = 1) in vec2 fragTexCoord;

			layout(location = 0) out vec4 outColor;

			layout(binding = 1) uniform sampler2D texSampler;

			void main() {
				outColor = vec4(fragColor, 1);// texture(texSampler, fragTexCoord) * vec4(fragColor, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );

		basicShader = new ShaderSet( vertex, fragment );
		pipeline = new Pipeline( Device, basicShader, ToScreenRenderPass );

		var rng = new Random();
		bg = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle() );
		bg = new( 0, 0, 0 );

		List<Point2<double>> vertices = new();
		List<uint> indices = new();

		Vector2<double> origin = new Vector2<double>();
		double scale = 0.1 / font.UnitsPerEm;
		void addGlyph ( Glyph glyph ) {
			foreach ( var spline in glyph.Outline.Splines ) {
				var points = spline.GetPoints();
				var last = points.First().ScaleFromOrigin( scale ) + origin;
				foreach ( var p in points.Skip( 1 ) ) {
					var point = p.ScaleFromOrigin( scale ) + origin;
					var delta = ( point - last ).Normalized() * scale * 10;
					var right = delta.Right;

					var a = last + right;
					var b = last - right;
					var c = point + right;
					var d = point - right;
					var ai = vertices.Count;
					vertices.Add( a );
					var bi = vertices.Count;
					vertices.Add( b );
					var ci = vertices.Count;
					vertices.Add( c );
					var di = vertices.Count;
					vertices.Add( d );

					indices.Add( (uint)ai );
					indices.Add( (uint)bi );
					indices.Add( (uint)ci );

					indices.Add( (uint)bi );
					indices.Add( (uint)di );
					indices.Add( (uint)ci );

					last = point;
				}
			}

			origin += new Vector2<double>( glyph.HorizontalAdvance, 0 ) * scale;
		}

		foreach ( var rune in "Yet another insanely fast Hello World program made in C# (⚡ blazingly fast ⚡) 🚀🚀🚀🚀🚀 (super fast 🔥🔥🔥🔥🔥🔥🔥)".EnumerateRunes() ) {
			if ( font.GetGlyph( rune ) is Glyph glyph )
				addGlyph( glyph );
		}

		vertexBuffer = new( Device, VkBufferUsageFlags.VertexBuffer );
		vertexBuffer.AllocateAndTransfer( vertices.SelectMany( x => new[] {
				(float)x.X,
				(float)x.Y,
				0,
				1,
				1,
				1,
				0,
				0
			} ).ToArray(),
			CopyCommandPool, GraphicsQueue
		);

		indexBuffer = new( Device, VkBufferUsageFlags.IndexBuffer );
		indexBuffer.AllocateAndTransfer( indices.ToArray(), CopyCommandPool, GraphicsQueue );
		indexCount = indices.Count;

		uniforms = new( Device, VkBufferUsageFlags.UniformBuffer );
		uniforms.Allocate( 1 );
		basicShader.DescriptorSet.ConfigureUniforms( uniforms, 0 );

		using var image = SixLabors.ImageSharp.Image.Load<Rgba32>( "./viking_room.png" );
		image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		texture = new( Device );
		texture.AllocateAndTransfer( image, CopyCommandPool, GraphicsQueue );

		sampler = new( Device, maxLod: texture.MipMapLevels );
		basicShader.DescriptorSet.ConfigureTexture( texture, sampler, 1 );
	}

	Point3<float> position = new( 0, 0, -5 );
	protected override void Render ( FrameInfo info, FrameBuffer frame ) {
		var commands = info.Commands;

		commands.Reset();
		commands.Begin();
		commands.BeginRenderPass( frame, new VkClearValue { color = bg }, new VkClearValue { depthStencil = { depth = 1 } } );
		commands.BindPipeline( pipeline );
		commands.SetViewPort( new() {
			minDepth = 0,
			maxDepth = 1,
			width = frame.Size.width,
			height = frame.Size.height
		} );
		commands.SetScissor( new() {
			extent = frame.Size
		} );

		commands.BindVertexBuffer( vertexBuffer );
		commands.BindIndexBuffer( indexBuffer );

		var cameraRotation = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitX, ( (float)Window.CursorPosition.Y / Window.Height - 0.5f ).Degrees() * 180 )
			* Matrix4<float>.FromAxisAngle( Vector3<float>.UnitY, ( (float)Window.CursorPosition.X / Window.Width - 0.5f ).Degrees() * 360 );

		var inverseCameraRotation = cameraRotation.Inverse();

		var deltaPosition = new Vector3<float>();
		if ( Keys.IsActive( Key.W ) )
			deltaPosition += Vector3<float>.UnitZ;
		if ( Keys.IsActive( Key.S ) )
			deltaPosition -= Vector3<float>.UnitZ;
		if ( Keys.IsActive( Key.D ) )
			deltaPosition += Vector3<float>.UnitX;
		if ( Keys.IsActive( Key.A ) )
			deltaPosition -= Vector3<float>.UnitX;

		deltaPosition = cameraRotation.Apply( deltaPosition );

		if ( Keys.IsActive( Key.Space ) )
			deltaPosition += Vector3<float>.UnitY;
		if ( Keys.IsActive( Key.LeftShift ) )
			deltaPosition -= Vector3<float>.UnitY;
		if ( deltaPosition != Vector3<float>.Zero ) {
			position += deltaPosition * (float)DeltaTime.TotalSeconds;
		}

		var cameraMatrix = Matrix4<float>.CreateTranslation( -position.X, -position.Y, -position.Z )
			* inverseCameraRotation;

		uniforms.Transfer( new Matrices() {
			Model = Matrix4<float>.Identity,//FromAxisAngle( Vector3<float>.UnitX, -90f.Degrees() ),
			View = cameraMatrix,
			Projection = Renderer.CreateLeftHandCorrectionMatrix<float>() 
				* Matrix4<float>.CreatePerspective( frame.Size.width, frame.Size.height, 0.01f, float.PositiveInfinity )
		} );
		commands.BindDescriptor( pipeline.Layout, basicShader.DescriptorSet );
		commands.DrawIndexed( (uint)indexCount );
		commands.FinishRenderPass();
		commands.Finish();
	}

	protected override void Dispose () {
		pipeline.Dispose();
		fragment.Dispose();
		vertex.Dispose();
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		uniforms.Dispose();
		texture.Dispose();
		sampler.Dispose();
	}
}