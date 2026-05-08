using Spectre.Console;

namespace StadiumSystem.UI.Theming;

public sealed record ThemePalette(
    Color Header,
    Color Accent,
    Color Muted,
    Color Success,
    Color Warning,
    Color Danger)
{
    public static ThemePalette Professional { get; } = new(
        Color.Navy,
        Color.SteelBlue1,
        Color.Grey70,
        Color.Green3,
        Color.Gold1,
        Color.Red3);

    public static ThemePalette HighContrast { get; } = new(
        Color.Blue,
        Color.Cyan1,
        Color.Grey84,
        Color.Green1,
        Color.Yellow1,
        Color.Red1);
}
