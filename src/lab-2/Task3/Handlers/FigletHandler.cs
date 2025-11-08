using Spectre.Console;
using Task3.Models;

namespace Task3.Handlers;

public class FigletHandler : Handler
{
    private readonly Handler _next;

    public FigletHandler(Handler next) : base(next)
    {
        _next = next;
    }

    public override async Task Handle(DisplayInfo displayInfo)
    {
        if (displayInfo.InfoType == "figlet")
        {
            Color? color = displayInfo.InfoColor;
            FigletText figlet = new FigletText(displayInfo.Info).Color(color).Centered();
            AnsiConsole.Write(figlet);
        }
        else
        {
            await _next.Handle(displayInfo);
        }
    }
}