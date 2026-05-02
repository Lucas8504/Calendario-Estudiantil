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

        // Los servicios se resuelven desde el DI container cuando se necesitan
        // Las páginas resuelven sus propios servicios en sus constructores
        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        // Forzar inicialización de la base de datos
        _ = InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
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
    }
}