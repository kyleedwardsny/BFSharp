using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BFSharp
{
    public class Compiler
	{
        public static MethodInfo JITCompile(string code)
        {
            DynamicMethod method = new DynamicMethod("BFCompiled", null, new Type[]{});
            Compile(method.GetILGenerator(), code);
            return method;
        }

        public static void Compile(ILGenerator generator, string code)
        {
            // Set up the loop stacks.
            Stack<Label> loopStackBegin = new Stack<Label>();
            Stack<Label> loopStackEnd = new Stack<Label>();

            // Get Console methods.
            Type consoleType = typeof(Console);
            MethodInfo openStandardOutput = consoleType.GetMethod("OpenStandardOutput", new Type[]{});
            MethodInfo openStandardInput = consoleType.GetMethod("OpenStandardInput", new Type[]{});

            // Get Stream methods.
            Type streamType = typeof(System.IO.Stream);
            MethodInfo writeByte = streamType.GetMethod("WriteByte", new Type[]{typeof(byte)});
            MethodInfo readByte = streamType.GetMethod("ReadByte", new Type[]{});

            // Set up the local variables.
            LocalBuilder arrayVar = generator.DeclareLocal(typeof(byte[]));
            LocalBuilder indexVar = generator.DeclareLocal(typeof(int));
            LocalBuilder stdoutVar = generator.DeclareLocal(streamType);
            LocalBuilder stdinVar = generator.DeclareLocal(streamType);

            // Initialize stdout and stdin.
            generator.Emit(OpCodes.Call, openStandardOutput);
            generator.Emit(OpCodes.Stloc_S, stdoutVar);
            generator.Emit(OpCodes.Call, openStandardInput);
            generator.Emit(OpCodes.Stloc_S, stdinVar);

            // Initialize an array of size 32768 and start in the middle.
            generator.Emit(OpCodes.Ldc_I4, 32768);
            generator.Emit(OpCodes.Newarr, typeof(byte));
            generator.Emit(OpCodes.Stloc_S, arrayVar);
            generator.Emit(OpCodes.Ldc_I4, 16384);
            generator.Emit(OpCodes.Stloc_S, indexVar);

            // Go through the whole string.
            Label loopStart, loopEnd;
            foreach (char c in code)
			{
                switch (c)
				{
                case '+':
                    // Prepare for upcoming store.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);

                    // Get the value and increment it.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldelem_I1);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Add);

                    // Put it back into the array.
                    generator.Emit(OpCodes.Stelem_I1);
                    break;

                case '-':
                    // Prepare for upcoming store.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);

                    // Get the value and decrement it.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldelem_I1);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Sub);

                    // Put it back into the array.
                    generator.Emit(OpCodes.Stelem_I1);
                    break;

                case '>':
                    // Get the index, increment it, and put it back.
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Add);
                    generator.Emit(OpCodes.Stloc_S, indexVar);
                    break;

                case '<':
                    // Get the index, decrement it, and put it back.
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Sub);
                    generator.Emit(OpCodes.Stloc_S, indexVar);
                    break;

                case '.':
                    // Get the current value and write it to standard output.
                    generator.Emit(OpCodes.Ldloc_S, stdoutVar);
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldelem_I1);
                    generator.Emit(OpCodes.Callvirt, writeByte);
                    break;

                case ',':
                    // Get a value from standard input and store it in the array.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldloc_S, stdinVar);
                    generator.Emit(OpCodes.Callvirt, readByte);
                    generator.Emit(OpCodes.Stelem_I1);
                    break;

                case '[':
                    // Define the labels.
                    loopStart = generator.DefineLabel();
                    loopEnd = generator.DefineLabel();
                    loopStackBegin.Push(loopStart);
                    loopStackEnd.Push(loopEnd);

                    // Find out the current value and branch.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldelem_I1);
                    generator.Emit(OpCodes.Brfalse, loopEnd);
                    generator.MarkLabel(loopStart);
                    break;

                case ']':
                    // Get the labels.
                    loopStart = loopStackBegin.Pop();
                    loopEnd = loopStackEnd.Pop();

                    // Find out the current value and branch.
                    generator.Emit(OpCodes.Ldloc_S, arrayVar);
                    generator.Emit(OpCodes.Ldloc_S, indexVar);
                    generator.Emit(OpCodes.Ldelem_I1);
                    generator.Emit(OpCodes.Brtrue, loopStart);
                    generator.MarkLabel(loopEnd);
                    break;
                }
            }

            // Done.
            generator.Emit(OpCodes.Ret);
        }
    }
}
