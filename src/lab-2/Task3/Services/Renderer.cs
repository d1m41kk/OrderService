using Spectre.Console;
using Task3.Handlers;
using Task3.Models;

namespace Task3.Services;

public class Renderer
{
    private readonly Handler _handlerChain;

    public Renderer()
    {
        _handlerChain = new Base64Handler(
            new FigletHandler(
                new UrlHandler(
                    new TerminalHandler())));
    }

    public async Task Render(DisplayInfo currentValue, CancellationToken token)
    {
        AnsiConsole.Clear();
        await _handlerChain.Handle(currentValue);
    }
}