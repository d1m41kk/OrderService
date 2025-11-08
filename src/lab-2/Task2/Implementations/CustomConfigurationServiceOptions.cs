namespace Task2.Implementations;

public class CustomConfigurationServiceOptions
{
    public int PageSize { get; set; } = 1;

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(1);
}