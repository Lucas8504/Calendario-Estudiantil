using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Services;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil;

/// <summary>
/// App principal. Servicios resueltos desde el DI container.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // NavigationPage como contenedor principal, SchedulePage como página raíz
        var navPage = new NavigationPage(new Views.SchedulePage());
        navPage.BarBackgroundColor = AppConstants.Theme.Accent;
        navPage.BarTextColor = Colors.White;
        MainPage = navPage;
    }

    protected override void OnStart()
    {
        base.OnStart();
        
        // Aplicar tema guardado
        ApplySavedTheme();
        
        // Forzar inicialización de la base de datos
        _ = InitializeDatabaseAsync();
    }

    private void ApplySavedTheme()
    {
        try
        {
            var themeService = IPlatformApplication.Current?.Services
                .GetRequiredService<IThemeService>();
            
            themeService?.ApplyTheme();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error aplicando tema: {ex.Message}");
        }
    }

    private Task InitializeDatabaseAsync()
    {
        try
        {
            // Acceder a la base de datos para forzar la creación de tablas
            var dbService = IPlatformApplication.Current?.Services
                .GetRequiredService<IDatabaseService>();
            if (dbService != null)
            {
                _ = dbService.Database;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error inicializando BD: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}