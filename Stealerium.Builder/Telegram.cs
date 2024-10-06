using System.Net.Http;
using System.Text.RegularExpressions;

namespace Stealerium.Builder
{
    internal sealed class Telegram
    {
        private static readonly string TelegramBotAPI = "https://api.telegram.org/bot";
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Extract message ID from Telegram API response
        /// </summary>
        /// <param name="response">Telegram API response</param>
        /// <returns>Message ID, or -1 if not found or an error occurs</returns>
        private static int GetMessageId(string response)
        {
            Match match = Regex.Match(response, "\"message_id\":(\\d+)");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
           
            return -1; // Return -1 if the message ID was not found or an error occurred
        }

        /// <summary>
        /// Check if the Telegram bot token is valid asynchronously
        /// </summary>
        /// <param name="telegramAPI">Telegram bot token</param>
        /// <returns>Returns a Task<bool> indicating if the token is valid</returns>
        public static async Task<bool> TokenIsValidAsync(string telegramAPI)
        {
            string requestUrl = TelegramBotAPI + telegramAPI + "/getMe";
            HttpResponseMessage response = await _client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody.StartsWith("{\"ok\":true,");
            }
            
            return false; // Return false if there was an error or the token is invalid
        }

        /// <summary>
        /// Send a message to a Telegram chat asynchronously
        /// </summary>
        /// <param name="text">Message text</param>
        /// <param name="telegramAPI">Telegram bot token</param>
        /// <param name="telegramID">Telegram chat ID</param>
        /// <returns>Returns a Task<int> indicating the message ID if successful, or 0 if failed</returns>
        public static async Task<int> SendMessageAsync(string text, string telegramAPI, string telegramID)
        {
            // Construct the request URL
            string requestUrl = TelegramBotAPI + telegramAPI + "/sendMessage" +
                "?chat_id=" + telegramID +
                "&text=" + Uri.EscapeDataString(text) + // Escape special characters in the message
                "&parse_mode=Markdown" + "&disable_web_page_preview=True";
            
            // Send the GET request to Telegram API
            HttpResponseMessage response = await _client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return GetMessageId(responseBody); // Extract message ID from the response
            }
      
            return 0; // Return 0 if there was an error or the request failed
        }
    }
}
