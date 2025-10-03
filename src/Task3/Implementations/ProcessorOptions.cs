namespace Task3.Implementations;

public class ProcessorOptions
{
    public int ChannelCapacity { get; init; }

    public int BatchSize { get; init; }

    public TimeSpan BatchWindow { get; init; }
}