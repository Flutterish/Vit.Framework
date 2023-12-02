# Vit.Framework
Vit is a freeform app framework with minimal bloat.

## Structure
The framework is separated into the main project(s) which contains utility code and interfaces, and add-on projects which implement specific functionality, such as the SDL window host, BASS audio, a specific rendering backend such as OpenGL, Direct3D11, Vulkan or specialised renderers for 2D or 3D scenes. You are free to include only the ones you need.

## Philosophy
Vit is focused on 4 things:
* Correctness - it is more or less enforced that you use correct units such as points, unitless axes and vectors. They have operations applicable to the type, which means that if your code starts looking ugly with casts, it means you are doing something wrong. Also, through the use of debug-conditional code, the easier to mess up parts such as shader data alignment is automatically checked for you. The add-on projects generally should also enforce correctness in their own ways.
* Freedom - the framework is a tool for you to use, not a guideline for how you should code. You are outside the framework, not inside it. There is no hidden "framework-internal" functionality, every single thing the framework can do, so can you. The only time you will see the "internal" keyword is when it should have been a "friend" keyword.
* Reusability - both code and data is reusable. Take the 2D UI add-on for exaple - you can load and unload components at will, and the graphics backend can even switch the graphics API at runtime without needing to restart the app.
* Performance - whether graphics or logic, you can expect performance and minimal allocations. We use structs, spans, pooling, pointers, JIT-time templates and even custom allocators when applicable.

## Licensing
The Vit framework itself is free to use under the MIT license, however add-ons which use licensed libraries might be subject to a separate license. The following add-ons use potentially licensed components (please read the original license, this is only a short summary):
* Vit.Framework.Audio.Bass uses the BASS audio library, which is free to use for non-commercial and personal use, but requires a license, which you need to acquire yourself from [radio42](http://bass.radio42.com/bass_register.html) for commercial use.