using Spectre.Console;
using Task3.Models;

namespace Task3.Handlers;

public class Base64Handler : Handler
{
    private readonly Handler _next;

    public Base64Handler(Handler next) : base(next)
    {
        _next = next;
    }

    public override async Task Handle(DisplayInfo displayInfo)
    {
        if (displayInfo.InfoType == "base64")
        {
            byte[] bytes = Convert.FromBase64String(displayInfo.Info);
            using var stream = new MemoryStream(bytes);
            var image = new CanvasImage(stream);
            AnsiConsole.Write(image);
        }
        else
        {
            await _next.Handle(displayInfo);
        }
    }
}