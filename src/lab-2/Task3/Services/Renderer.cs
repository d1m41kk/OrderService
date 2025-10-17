using Spectre.Console;
using Task3.Models;

namespace Task3.Services;

public class Renderer
{
    public Task Render(DisplayInfo currentValue, CancellationToken token)
    {
        AnsiConsole.Clear();
        switch (currentValue.InfoType)
        {
            case "figlet":
            {
                Color? color = currentValue.InfoColor;
                FigletText figlet = new FigletText(currentValue.Info).Color(color).Centered();
                AnsiConsole.Write(figlet);
                break;
            }

            case "base64":
            {
                byte[] bytes = Convert.FromBase64String(currentValue.Info);
                using var stream = new MemoryStream(bytes);
                var image = new CanvasImage(stream);
                AnsiConsole.Write(image);
                break;
            }

            case "url":
            {
                var image = new CanvasImage(currentValue.Info);
                AnsiConsole.Write(image);
                break;
            }

            default:
                AnsiConsole.Write("Unknown display type");
                break;
        }

        return Task.CompletedTask;
    }
}