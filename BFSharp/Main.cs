using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BFSharp
{
    class MainClass
	{
        public static int Main(string[] args)
        {
            // Show help information.
            if (args.Length < 1)
			{
                string programName = Process.GetCurrentProcess().ProcessName;
                System.Console.WriteLine("Usage: " + programName + " [-c <output file>] <input file>");
                System.Console.WriteLine();
                System.Console.WriteLine("Options:");
                System.Console.WriteLine("  -c <output file>      Compile into an executable rather than interpreting.");
                return 1;
            }

            // Parse the command-line arguments.
            string inputFile;
            string outputFile;
            if (args[0] == "-c")
			{
                outputFile = args[1];
                inputFile = args[2];
                AppDomain domain = AppDomain.CurrentDomain;
                AssemblyBuilder assembly = domain.DefineDynamicAssembly(new AssemblyName(outputFile), AssemblyBuilderAccess.Save);
                ModuleBuilder module = assembly.DefineDynamicModule(outputFile, outputFile);
                TypeBuilder type = module.DefineType("BFCompiled");
                MethodBuilder method = type.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), null);
                ILGenerator generator = method.GetILGenerator();
                StreamReader reader = new StreamReader(inputFile);
                Compiler.Compile(generator, reader.ReadToEnd());
                type.CreateType();
                assembly.SetEntryPoint(method);
                assembly.Save(outputFile);
            }
			else
			{
                inputFile = args[0];
                StreamReader reader = new StreamReader(inputFile);
                MethodInfo compiled = Compiler.JITCompile(reader.ReadToEnd());
                compiled.Invoke(null, null);
            }
            return 0;
        }
    }
}
