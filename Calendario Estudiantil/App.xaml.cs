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
        MainPage = new NavigationPage(new Views.SchedulePage());
    }

    protected override void OnStart()
    {
        base.OnStart();
        // Forzar inicialización de la base de datos
        _ = InitializeDatabaseAsync();
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