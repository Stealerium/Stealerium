using Spectre.Console;

namespace Builder.Modules;

internal sealed class Cli
{
    public static string GetBoolValue(string text)
    {
        Console.Write("(?) " + text + " (y/n): ");
        Console.ForegroundColor = ConsoleColor.White;
        var result = Console.ReadLine();
        return result != null && result.ToUpper() == "Y" ? "1" : "0";
    }

    public static string? GetStringValue(string text)
    {
        AnsiConsole.Write(new Markup("[bold yellow](?)[/] [red]" + text + "[/] \n>>> "));
        return Console.ReadLine();
    }

    public static string GetEncryptedString(string text)
    {
        var result = GetStringValue(text);
        return !string.IsNullOrEmpty(result) ? Crypt.EncryptConfig(result) : "";
    }

    public static void ShowError(string text)
    {
        AnsiConsole.Write(new Markup("[red] (!) " + text + "\n Press any key to exit...[/]"));
        Console.ReadKey();
        Environment.Exit(1);
    }

    public static void ShowInfo(string text)
    {
        AnsiConsole.Write(new Markup("[yellow] (i) " + text + "[/]"));
    }

    public static void ShowSuccess(string text)
    {
        AnsiConsole.Write(new Markup("[green] (+) " + text + "[/]"));
    }
}