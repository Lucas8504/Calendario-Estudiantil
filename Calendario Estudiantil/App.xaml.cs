using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Services;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil;

/// <summary>
/// App principal. Servicios resueltos desde el DI container.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Propiedades estáticas para acceso (migración pendiente a DI completa).
    /// </summary>
    public static ICourseService courseService { get; private set; } = null!;
    public static ISubjectService subjectService { get; private set; } = null!;
    public static ITaskService taskService { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        // Resolver servicios desde el DI container de MAUI
        // Esto es necesario porque Application no soporta constructor injection directo
        var services = IPlatformApplication.Current?.Services 
            ?? throw new InvalidOperationException("DI container no disponible");

        courseService = services.GetRequiredService<ICourseService>();
        subjectService = services.GetRequiredService<ISubjectService>();
        taskService = services.GetRequiredService<ITaskService>();

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