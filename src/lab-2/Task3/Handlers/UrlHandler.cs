using Spectre.Console;
using Task3.Models;

namespace Task3.Handlers;

public class UrlHandler : Handler
{
    private readonly Handler _next;

    public UrlHandler(Handler next) : base(next)
    {
        _next = next;
    }

    public override async Task Handle(DisplayInfo displayInfo)
    {
        if (displayInfo.InfoType == "url")
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(displayInfo.Info);
            using var stream = new MemoryStream(imageBytes);
            var image = new CanvasImage(stream);
            AnsiConsole.Write(image);
        }
        else
        {
            await _next.Handle(displayInfo);
        }
    }
}