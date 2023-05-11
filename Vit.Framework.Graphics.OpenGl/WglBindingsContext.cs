/*
MIT License
Copyright (c) 2019 Eschryn <https://github.com/Eschryn>
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Win32.SafeHandles;
using OpenTK;
using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.OpenGl;

public class WglBindingsContext : IBindingsContext {
	[DllImport( "opengl32.dll", CharSet = CharSet.Ansi )]
	private static extern IntPtr wglGetProcAddress ( [MarshalAs( UnmanagedType.LPStr )] string procName );

	private readonly ModuleSafeHandle _openGlHandle;

	public WglBindingsContext () {
		_openGlHandle = Kernel32.LoadLibrary( "opengl32.dll" );
	}

	public IntPtr GetProcAddress ( string procName ) {
		IntPtr addr = wglGetProcAddress( procName );
		return addr != IntPtr.Zero ? addr : Kernel32.GetProcAddress( _openGlHandle, procName );
	}

	private static class Kernel32 {
		[DllImport( "kernel32", SetLastError = true, CharSet = CharSet.Ansi )]
		public static extern ModuleSafeHandle LoadLibrary ( [MarshalAs( UnmanagedType.LPStr )] string lpFileName );

		[DllImport( "kernel32", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool FreeLibrary ( IntPtr hModule );

		[DllImport( "kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true )]
		public static extern IntPtr GetProcAddress ( ModuleSafeHandle hModule, string procName );
	}

	private class ModuleSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
		public ModuleSafeHandle () : base( true ) {
		}

		protected override bool ReleaseHandle () {
			return Kernel32.FreeLibrary( handle );
		}
	}
}