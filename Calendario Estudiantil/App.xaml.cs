using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil;

/// <summary>
/// App principal. Inicializa servicios y base de datos.
/// </summary>
public partial class App : Application
{
    public static ICourseService courseService { get; private set; } = null!;
    public static ISubjectService subjectService { get; private set; } = null!;
    public static ITaskService taskService { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        // Inicializar servicios (singleton para toda la app)
        var notificationService = new NotificationService();
        courseService = new CourseService();
        subjectService = new SubjectService();
        taskService = new TaskService(notificationService);

        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        // Forzar inicialización de la base de datos
        _ = DatabaseService.Database;
    }
}