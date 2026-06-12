using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil;

/// <summary>
/// App principal. Servicios resueltos desde el DI container.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resolver MainPage desde DI (inyecta SchedulePage, TasksPage, SubjectsPage como singletons)
        var mainPage = _services.GetRequiredService<MainPage>();
        
        // Pre-cargar datos de materias en caché para que aparezcan al instante
        var subjectService = _services.GetService<ISubjectService>();
        if (subjectService != null)
            _ = subjectService.GetAllAsync();

        var navPage = new NavigationPage(mainPage);
        navPage.BarBackgroundColor = AppConstants.Theme.Accent;
        navPage.BarTextColor = Colors.White;
        MainPage = navPage;
        
        return base.CreateWindow(activationState);
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
            var themeService = _services.GetService<IThemeService>();
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
            var dbService = _services.GetService<IDatabaseService>();
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
