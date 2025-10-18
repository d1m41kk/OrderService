using Spectre.Console;
using Task3.Models;

namespace Task3.Handlers;

public class TerminalHandler : Handler
{
    public TerminalHandler() : base(null) { }

    public override async Task Handle(DisplayInfo displayInfo)
    {
        AnsiConsole.MarkupLine($"[red]Unsupported info type: {displayInfo.InfoType}[/]");
        await Task.CompletedTask;
    }
}