using Microsoft.Maui.Storage;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Servicio para gestionar los temas de la aplicación.
/// </summary>
public class ThemeService : IThemeService
{
    private const string ThemeKey = "app_theme";
    private const string DefaultTheme = "System";

    public string SelectedTheme
    {
        get => Preferences.Get(ThemeKey, DefaultTheme);
        set
        {
            Preferences.Set(ThemeKey, value);
            ApplyTheme();
        }
    }

    public void ApplyTheme()
    {
        var theme = SelectedTheme.ToLowerInvariant();
        
        switch (theme)
        {
            case "light":
                Application.Current!.UserAppTheme = AppTheme.Light;
                break;
            case "dark":
                Application.Current!.UserAppTheme = AppTheme.Dark;
                break;
            case "system":
            default:
                Application.Current!.UserAppTheme = AppTheme.Unspecified;
                break;
        }
    }

    public AppTheme GetCurrentTheme()
    {
        var theme = SelectedTheme.ToLowerInvariant();
        
        return theme switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => Application.Current!.RequestedTheme
        };
    }
}
