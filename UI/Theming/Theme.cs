using Spectre.Console;

namespace StadiumSystem.UI.Theming;

public static class Theme
{
    public static ThemePalette Current => ThemeManager.Current;

    public static string Header(string text) => MarkupLine(Current.Header, text);
    public static string Accent(string text) => MarkupLine(Current.Accent, text);
    public static string Muted(string text) => MarkupLine(Current.Muted, text);
    public static string Success(string text) => MarkupLine(Current.Success, text);
    public static string Warning(string text) => MarkupLine(Current.Warning, text);
    public static string Danger(string text) => MarkupLine(Current.Danger, text);

    public static Color HeaderColor => Current.Header;
    public static Color AccentColor => Current.Accent;
    public static Color MutedColor => Current.Muted;
    public static Color SuccessColor => Current.Success;
    public static Color WarningColor => Current.Warning;
    public static Color DangerColor => Current.Danger;

    private static string MarkupLine(Color color, string text)
    {
        return $"[{color}]{Markup.Escape(text)}[/]";
    }
}
