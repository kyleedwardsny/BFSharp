# Introduction
BFSharp is a just-in-time (JIT) compiler, written in C# and targeted for the
.NET runtime, which compiles code written in the
[Brainfuck programming language][wikipedia]. BFSharp does not generate
intermediate C# code; instead, it targets for the .NET runtime directly,
translating the Brainfuck commands directly into CLI opcodes.

To use BFSharp:

1.  Build BFSharp using Visual Studio, MonoDevelop, or `xbuild`.
2.  To run in JIT mode:

    `> BFSharp.exe example.bf`

    or in Mono:

    `$ mono BFSharp.exe example.bf`
3.  To compile a Brainfuck program into a standalone executable:

    `> BFSharp.exe -c example.exe example.bf`

    or in Mono:

    `$ mono BFSharp.exe -c example.exe example.bf`

    and then run it:

    `> example.exe`

    or in Mono:

    `$ mono example.exe`

## Sample Programs
Sample programs can be found in BFSharp/Samples. Overview of sample programs:

* `hello.bf` - Prints "Hello World!". Courtesy of
  [https://en.wikipedia.org/wiki/Brainfuck][wikipedia].
* `reverse.bf` - Reverses every line of input until it encounters a blank line.

# Future Improvements
BFSharp in its current form is very naive and unoptimized. Every single command
copies one or more variables onto the stack, modifies them, and puts them back.

There are some ways this could be optimized. For the first optimization, long
chains of `>`, `<`, `+`, or `-` commands could be optimized by combining them into a
single add or subtract operation. For even further optimization, memory
boundaries could be established at input, output, and loop commands. Between
two memory boundaries, cells would be added and subtracted to their final
values before the next boundary is reached. In addition, cells would be
referenced by a non-zero offset relative to the current pointer, rather than
directly from the current pointer, and the pointer would only be modified once
between memory boundaries.

[wikipedia]: https://en.wikipedia.org/wiki/Brainfuck
