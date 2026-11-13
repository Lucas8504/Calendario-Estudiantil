using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.Services.Voice;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SettingsPage : ContentPage
{
    private readonly IThemeService _themeService;
    private readonly IApiKeyStore _apiKeyStore;

    public SettingsPage(IThemeService themeService, IApiKeyStore apiKeyStore)
    {
        InitializeComponent();

        _themeService = themeService;
        _apiKeyStore = apiKeyStore;

        // Seleccionar el tema actual
        SelectCurrentTheme();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshApiKeyStatusAsync();
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

    // ==================== Asistente de voz ====================

    private async void OnSaveApiKeyClicked(object sender, EventArgs e)
    {
        string key = GeminiKeyEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(key))
        {
            await DisplayAlert("Asistente de voz", "Pegá una API key antes de guardar.", "OK");
            return;
        }

        try
        {
            await _apiKeyStore.SetGeminiApiKeyAsync(key);
            GeminiKeyEntry.Text = string.Empty;
            await RefreshApiKeyStatusAsync();
            await DisplayAlert("Asistente de voz", "API key guardada en este dispositivo.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Asistente de voz", $"No pude guardar la API key: {ex.Message}", "OK");
        }
    }

    private async void OnClearApiKeyClicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlert(
            "Asistente de voz",
            "¿Borrar la API key guardada en este dispositivo?",
            "Borrar",
            "Cancelar");

        if (!confirmed)
            return;

        await _apiKeyStore.ClearGeminiApiKeyAsync();
        await RefreshApiKeyStatusAsync();
    }

    private async Task RefreshApiKeyStatusAsync()
    {
        string? key = await _apiKeyStore.GetGeminiApiKeyAsync();

        ApiKeyStatusLabel.Text = string.IsNullOrWhiteSpace(key)
            ? "Sin configurar: el micrófono va a pedir la key."
            : "Configurada en este dispositivo.";

        ClearApiKeyButton.IsEnabled = !string.IsNullOrWhiteSpace(key);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
