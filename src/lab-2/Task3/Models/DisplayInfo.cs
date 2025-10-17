using Spectre.Console;

namespace Task3.Models;

public class DisplayInfo
{
    public string InfoType { get; set; } = "url";

    public string Info { get; set; } = "C:\\Users\\d_sku\\Downloads\\беззубый-джокер-v0-daqunz7dg1af1.png";

    public Color? InfoColor { get; set; } = new Color(0, 255, 0);

    public int UpdateTime { get; set; } = 500;
}