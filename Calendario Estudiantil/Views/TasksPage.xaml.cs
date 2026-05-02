using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    // Constructor sin parámetros para XAML/MAUI
    public TasksPage()
    {
        InitializeComponent();
        
        // Resolver servicios desde el DI container
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService not available");
        
        _taskService = IPlatformApplication.Current?.Services
            .GetRequiredService<ITaskService>()
            ?? throw new InvalidOperationException("ITaskService not available");
        
        _viewModel = new TasksViewModel(_taskService);
        BindingContext = _viewModel;
        TasksList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Tasks)));
    }

    // Constructor con parámetros para navegación programática
    public TasksPage(ITaskService taskService) : this()
    {
        // El constructor sin parámetros ya inicializa todo
        // Este constructor existe para mantener compatibilidad
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadTasksAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
    }

    private async void OnFilterAll(object sender, EventArgs e)
    {
        try
        {
            _viewModel.FilterMode = "Todos";
            await _viewModel.LoadTasksAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo aplicar el filtro: {ex.Message}", "OK");
        }
    }

    private async void OnFilterToday(object sender, EventArgs e)
    {
        try
        {
            _viewModel.FilterMode = "Hoy";
            await _viewModel.LoadTasksAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo aplicar el filtro: {ex.Message}", "OK");
        }
    }

    private async void OnFilterPending(object sender, EventArgs e)
    {
        try
        {
            _viewModel.FilterMode = "Pendientes";
            await _viewModel.LoadTasksAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo aplicar el filtro: {ex.Message}", "OK");
        }
    }

    private async void OnCompleteTask(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.BindingContext is TaskDisplay td)
            {
                await _viewModel.CompleteTaskAsync(td);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo completar la tarea: {ex.Message}", "OK");
        }
    }

    private async void OnCreateTask(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new NewTaskPage(_subjectService, _taskService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la página de nueva tarea: {ex.Message}", "OK");
        }
    }
}