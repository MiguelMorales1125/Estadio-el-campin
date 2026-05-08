using System;

namespace StadiumSystem.UI.Theming;

public static class ThemeManager
{
    public static ThemePalette Current { get; private set; } = ThemePalette.Professional;

    public static void ConfigureFromEnvironment()
    {
        string? themeName = Environment.GetEnvironmentVariable("UI_THEME");
        SetTheme(themeName);
    }

    public static void SetTheme(string? themeName)
    {
        Current = themeName?.Trim().ToLowerInvariant() switch
        {
            "highcontrast" => ThemePalette.HighContrast,
            _ => ThemePalette.Professional,
        };
    }
}
