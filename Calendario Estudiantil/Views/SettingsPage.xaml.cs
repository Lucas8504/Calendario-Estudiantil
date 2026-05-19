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

    private void SelectCurrentTheme()
    {
        var currentTheme = _themeService.SelectedTheme.ToLowerInvariant();
        
        // Encontrar y marcar el RadioButton correcto
        foreach (var child in GetVisualTree())
        {
            if (child is RadioButton rb && rb.GroupName == "ThemeGroup")
            {
                if (rb.Value?.ToString()?.ToLowerInvariant() == currentTheme)
                {
                    rb.IsChecked = true;
                    break;
                }
            }
        }
    }

    private IEnumerable<VisualElement> GetVisualTree()
    {
        var queue = new Queue<VisualElement>();
        queue.Enqueue(this);
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;
            
            if (current is IViewContainer<View> container)
            {
                foreach (var child in container.Children)
                {
                    if (child is VisualElement ve)
                        queue.Enqueue(ve);
                }
            }
        }
    }

    private void OnThemeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton rb)
        {
            _themeService.SelectedTheme = rb.Value?.ToString() ?? "System";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
