using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Services;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SettingsPage : ContentPage
{
    private readonly IThemeService _themeService;

    public SettingsPage()
    {
        InitializeComponent();
        
        _themeService = IPlatformApplication.Current?.Services
            .GetRequiredService<IThemeService>()
            ?? throw new InvalidOperationException("IThemeService no disponible");
        
        // Seleccionar el tema actual
        SelectCurrentTheme();
    }

    private void OnThemeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string theme)
        {
            _themeService.SelectedTheme = theme;
            UpdateButtonSelection(theme);
        }
    }

    private void UpdateButtonSelection(string selectedTheme)
    {
        var theme = selectedTheme.ToLowerInvariant();
        var accentColor = AppConstants.Accent;
        var transparentColor = Colors.Transparent;
        
        // Reset all buttons
        BtnLight.BorderColor = transparentColor;
        BtnDark.BorderColor = transparentColor;
        BtnSystem.BorderColor = transparentColor;
        
        // Set selected button
        switch (theme)
        {
            case "light":
                BtnLight.BorderColor = accentColor;
                break;
            case "dark":
                BtnDark.BorderColor = accentColor;
                break;
            case "system":
                BtnSystem.BorderColor = accentColor;
                break;
        }
    }

    private void SelectCurrentTheme()
    {
        var currentTheme = _themeService.SelectedTheme.ToLowerInvariant();
        UpdateButtonSelection(currentTheme);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
