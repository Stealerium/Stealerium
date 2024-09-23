using System.Text.Json;
using Spectre.Console;

namespace BuilderConsole.Modules
{
    internal sealed class Discord
    {
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Asynchronously checks if a given Discord webhook URL is valid by making an HTTP request to it
        /// and checking for a specific JSON property in the response.
        /// </summary>
        /// <param name="token">The webhook URL to be validated.</param>
        /// <returns>A task representing the asynchronous operation, containing true if the webhook is valid, otherwise false.</returns>
        public static async Task<bool> WebhookIsValidAsync(string token)
        {
            try
            {
                // Send a GET request to the supplied webhook URL.
                var response = await _client.GetStringAsync(token);

                // Parse the JSON response into a document.
                using JsonDocument doc = JsonDocument.Parse(response);
                JsonElement root = doc.RootElement;

                // Check for the "type" property in the JSON response.
                if (root.TryGetProperty("type", out JsonElement typeElement))
                {
                    bool isValid = typeElement.GetInt32() == 1;

                    // Display a confirmation message if the webhook is valid.
                    if (isValid)
                    {
                        AnsiConsole.MarkupLine("[green]The Discord webhook is valid.[/]");
                    }

                    return isValid;
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Presenting HttpRequestException using markup with a red background and white foreground.
                AnsiConsole.Write(new Panel($"[white on red]HTTP Request Exception: {httpEx.Message}[/]").RoundedBorder());
            }
            catch (JsonException jsonEx)
            {
                // Presenting JsonException using a yellow stylized text.
                AnsiConsole.MarkupLine($"[yellow]JSON Parsing Exception: {jsonEx.Message}[/]");
            }
            catch (Exception ex)
            {
                // Generic error while checking the webhook, presented within a panel.
                AnsiConsole.Write(new Panel($"[red]Error checking webhook: {ex.Message}[/]").RoundedBorder());
            }

            // Return false by default if any exception occurs or if the webhook is invalid.
            return false;
        }

        /// <summary>
        /// Asynchronously sends a message to a Discord webhook URL with specified text content.
        /// </summary>
        /// <param name="text">The message text to be sent.</param>
        /// <param name="token">The Discord webhook URL to which the message will be sent.</param>
        /// <returns>A task representing the asynchronous operation, containing true if the message was sent successfully, otherwise false.</returns>
        public static async Task<bool> SendMessageAsync(string text, string token)
        {
            try
            {
                // Prepare the values for sending the message including username, avatar, and content.
                var discordValues = new Dictionary<string, string>
                {
                    ["username"] = "Stealerium",
                    ["avatar_url"] = "https://user-images.githubusercontent.com/45857590/138568746-1a5578fe-f51b-4114-bcf2-e374535f8488.png",
                    ["content"] = text
                };

                // Encode the values into x-www-form-urlencoded format for the POST request body.
                var content = new FormUrlEncodedContent(discordValues);

                // Send a POST request to the Discord webhook URL with the encoded content
                var response = await _client.PostAsync(token, content);

                // Return true if the HTTP response status code indicates success.
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log any exceptions with a red markup to indicate an error during message sending.
                AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
            }

            // Return false if an exception occurs or if the POST request did not succeed.
            return false;
        }
    }
}
