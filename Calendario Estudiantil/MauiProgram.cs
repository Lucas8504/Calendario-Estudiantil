using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.ViewModels;
using Calendario_Estudiantil.Views;
using Calendario_Estudiantil.Controls;

namespace Calendario_Estudiantil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // Inicializar CommunityToolkit.MAUI
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ==================== REGISTRO DE SERVICIOS (DI) ====================
            
            // Base de datos (Singleton - una sola instancia para toda la app)
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            
            // Servicios de negocio (Singleton para mantener estado)
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<ISubjectService, SubjectService>();
            builder.Services.AddSingleton<ITaskService, TaskService>();
            builder.Services.AddSingleton<IThemeService, ThemeService>();

            // ViewModels (Singleton - una instancia para toda la app)
            builder.Services.AddSingleton<ScheduleDayViewModel>();
            builder.Services.AddSingleton<TasksViewModel>();
            builder.Services.AddSingleton<SubjectViewModel>();

            // Views/Pages (Singleton - misma instancia en cada navegación)
            builder.Services.AddSingleton<SchedulePage>();
            builder.Services.AddSingleton<TasksPage>();
            builder.Services.AddSingleton<SubjectsPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<NewSchedulePage>();
            builder.Services.AddTransient<NewTaskPage>();

            // Controles
            builder.Services.AddTransient<BottomNavView>();

            // App principal (para que MAUI resuelva las dependencias)
            builder.Services.AddSingleton<App>();

            return builder.Build();
        }
    }
}