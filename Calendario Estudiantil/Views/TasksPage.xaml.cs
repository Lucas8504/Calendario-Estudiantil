using Calendario_Estudiantil.Helpers;
using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace Calendario_Estudiantil.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    // Cached colors to avoid repeated Color.FromArgb allocations
    private static readonly Color CachedGray666 = Color.FromArgb("#666666");
    private static readonly Color CachedWhite = Colors.White;
    private static readonly Color CachedDark1E1E1E = Color.FromArgb("#1E1E1E");
    private static readonly Color CachedDarkE0E0E0 = Color.FromArgb("#E0E0E0");
    private static readonly Color CachedLight333333 = Color.FromArgb("#333333");
    private static readonly Color CachedDarkA0A0A0 = Color.FromArgb("#A0A0A0");
    private static readonly Color CachedLightGray = Colors.Gray;
    private static readonly Color CachedDark2C2C2C = Color.FromArgb("#2C2C2C");
    private static readonly Color CachedLightF9FAFB = Color.FromArgb("#F9FAFB");

    // Filtros disponibles
    private static readonly string[] FilterOptions = { "Todos", "Hoy", "Pendientes", "Vencidas" };

    public TasksPage(TasksViewModel viewModel, ISubjectService subjectService, ITaskService taskService)
    {
        InitializeComponent();
        SubtitleLabel.Text = DateTime.Now.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("es-ES")).ToUpperInvariant();
        
        _viewModel = viewModel;
        _subjectService = subjectService;
        _taskService = taskService;
        
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            BuildFilterTabs();
            await _viewModel.LoadSubjectsAsync();
            await _viewModel.LoadTasksAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
    }

    private void BuildFilterTabs()
    {
        FilterTabsContainer.Children.Clear();

        foreach (var filter in FilterOptions)
        {
            var isSelected = filter == _viewModel.FilterMode;
            var btn = new Button
            {
                Text = filter,
                FontSize = 14,
                FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
                Padding = new Thickness(16, 8),
                CornerRadius = 20,
                BackgroundColor = isSelected ? AppConstants.Accent : Colors.Transparent,
                TextColor = isSelected ? Colors.White : Color.FromArgb("#666666"),
                BorderColor = Colors.Transparent,
                BorderWidth = 0,
                HorizontalOptions = LayoutOptions.Start
            };

            var selectedFilter = filter;
            btn.Clicked += async (s, e) =>
            {
                _viewModel.FilterMode = selectedFilter;
                await _viewModel.LoadTasksAsync();
                BuildFilterTabs();
            };

            FilterTabsContainer.Children.Add(btn);
        }
    }

    private async void OnTaskTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is TaskDisplay taskDisplay)
        {
            try
            {
                var task = taskDisplay.TaskItem;
                var options = task.Status == ItemStatus.Completed
                    ? new[] { "Eliminar" }
                    : new[] { "Completar", "Eliminar" };

                var action = await DisplayActionSheet(
                    task.Title,
                    "Cancelar",
                    null,
                    options);

                if (action == "Completar")
                {
                    await _viewModel.CompleteTaskAsync(taskDisplay);
                }
                else if (action == "Eliminar")
                {
                    var confirm = await DisplayAlert("Confirmar", "¿Eliminar esta tarea?", "Sí", "No");
                    if (confirm)
                    {
                        await _viewModel.DeleteTaskAsync(taskDisplay);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = IPlatformApplication.Current?.Services.GetService<SettingsPage>();
            if (settingsPage != null)
                await Navigation.PushAsync(settingsPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la configuración: {ex.Message}", "OK");
        }
    }

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        await NotificationHelper.ShowPendingTasksAsync(this, _taskService, _subjectService);
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
