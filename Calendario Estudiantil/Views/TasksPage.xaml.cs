using Calendario_Estudiantil.ViewModels;

namespace Calendario_Estudiantil.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksViewModel _viewModel;

    public TasksPage()
    {
        InitializeComponent();
        _viewModel = new TasksViewModel(App.taskService);
        BindingContext = _viewModel;
        TasksList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Tasks)));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTasksAsync();
    }

    private async void OnFilterAll(object sender, EventArgs e)
    {
        _viewModel.FilterMode = "Todos";
        await _viewModel.LoadTasksAsync();
    }

    private async void OnFilterToday(object sender, EventArgs e)
    {
        _viewModel.FilterMode = "Hoy";
        await _viewModel.LoadTasksAsync();
    }

    private async void OnFilterPending(object sender, EventArgs e)
    {
        _viewModel.FilterMode = "Pendientes";
        await _viewModel.LoadTasksAsync();
    }

    private async void OnCompleteTask(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is TaskDisplay td)
        {
            await _viewModel.CompleteTaskAsync(td);
        }
    }

    private async void OnCreateTask(object sender, EventArgs e)
    {
        await DisplayAlert("Tareas", "Formulario en desarrollo.", "OK");
    }
}