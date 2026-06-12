using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

/// <summary>
/// Página principal que actúa como contenedor de tabs.
/// Contiene una BottomNavView fija y un ContentArea donde se
/// intercambia el contenido de cada tab (Schedule, Tasks, Subjects).
/// 
/// Las tab pages (SchedulePage, TasksPage, SubjectsPage) son ContentPages.
/// Como ContentView.Content solo acepta View (no Page), extraemos el
/// Content de cada página y lo asignamos al ContentArea al cambiar de tab.
/// Cada página mantiene su estado en la ViewModel singleton.
/// El LoadDataAsync() se llama manualmente al activar cada tab.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly SchedulePage _schedulePage;
    private readonly TasksPage _tasksPage;
    private readonly SubjectsPage _subjectsPage;

    public MainPage(SchedulePage schedulePage, TasksPage tasksPage, SubjectsPage subjectsPage)
    {
        InitializeComponent();

        _schedulePage = schedulePage;
        _tasksPage = tasksPage;
        _subjectsPage = subjectsPage;

        // Escuchar cambios de tab desde BottomNavView
        BottomNav.TabSelected += OnTabSelected;

        // Mostrar el tab inicial
        ShowTab("Schedule");
    }

    private void OnTabSelected(object? sender, string tabType)
    {
        ShowTab(tabType);
    }

    private void ShowTab(string tabType)
    {
        ContentPage tabPage;
        Task loadTask;

        switch (tabType)
        {
            case "Tasks":
                tabPage = _tasksPage;
                BottomNav.ActiveTab = "Tasks";
                loadTask = _tasksPage.LoadDataAsync();
                break;
            case "Subjects":
                tabPage = _subjectsPage;
                BottomNav.ActiveTab = "Subjects";
                loadTask = _subjectsPage.LoadDataAsync();
                break;
            default: // "Schedule"
                tabPage = _schedulePage;
                BottomNav.ActiveTab = "Schedule";
                loadTask = _schedulePage.LoadDataAsync();
                break;
        }

        // Extraer el View raíz de la página y forzar su BindingContext.
        // Es crítico: al mover el View fuera de la página padre, pierde el
        // BindingContext heredado. Lo seteamos explícitamente para que los
        // bindings ({Binding Propiedad}) sigan funcionando.
        var tabContent = tabPage.Content;
        tabContent.BindingContext = tabPage.BindingContext;
        ContentArea.Content = tabContent;

        // Cargar datos de la tab seleccionada (fire-and-forget)
        _ = loadTask;
    }
}
