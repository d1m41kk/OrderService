using Spectre.Console;
using Task3.Models;

namespace Task3.Services;

public class Renderer
{
    public async Task Render(DisplayInfo currentValue, CancellationToken token)
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
                using var httpClient = new HttpClient();
                byte[] imageBytes = await httpClient.GetByteArrayAsync(currentValue.Info, token);
                using var stream = new MemoryStream(imageBytes);
                var image = new CanvasImage(stream);
                AnsiConsole.Write(image);
                break;
            }

            default:
                AnsiConsole.Write("Unknown display type");
                break;
        }

        await Task.CompletedTask;
    }
}