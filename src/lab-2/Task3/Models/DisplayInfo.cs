using Spectre.Console;

namespace Task3.Models;

public class DisplayInfo
{
    public string InfoType { get; set; } = "url";

    public string Info { get; set; } = "https://preview.redd.it/%D0%B1%D0%B5%D0%B7%D0%B7%D1%83%D0%B1%D1%8B%D0%B9-%D0%B4%D0%B6%D0%BE%D0%BA%D0%B5%D1%80-v0-daqunz7dg1af1.jpeg?width=196&auto=webp&s=84a91c8654bd255f2411648ac87afa1a9effd2f3";

    public Color? InfoColor { get; set; } = new Color(0, 255, 0);

    public int UpdateTime { get; set; } = 500;
}