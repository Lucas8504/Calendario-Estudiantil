using System.Diagnostics;
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
        /// <summary>
        /// Navigation del NavigationPage raíz (MainPage está embebida, no tiene navegación propia).
        /// Siempre disponible porque App.CreateWindow asigna MainPage antes del primer navegación.
        /// </summary>
        private INavigation RootNavigation => Application.Current!.MainPage!.Navigation;

        /// <summary>
        /// Página actual visible en el NavigationPage (la única conectada al árbol visual).
        /// Necesaria para DisplayAlert y popups de CommunityToolkit.Maui.
        /// </summary>
        private Page CurrentPage => Application.Current!.MainPage is NavigationPage nav
            ? nav.CurrentPage
            : Application.Current!.MainPage!;

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
        BuildFilterTabs();
    }

    /// <summary>
    /// Carga/refresca los datos de la página.
    /// Llamado por MainPage cuando este tab se vuelve visible.
    /// </summary>
    public Task LoadDataAsync()
    {
        var sw = Stopwatch.StartNew();
        Debug.WriteLine($"[TIMING] TasksPage.LoadDataAsync START");

        var task = Task.Run(async () =>
        {
            try
            {
                await _viewModel.LoadSubjectsAsync();
                Debug.WriteLine($"[TIMING]   LoadSubjectsAsync: {sw.ElapsedMilliseconds}ms");

                await _viewModel.LoadTasksAsync();
                Debug.WriteLine($"[TIMING]   LoadTasksAsync: {sw.ElapsedMilliseconds}ms");

                Debug.WriteLine($"[TIMING] TasksPage.LoadTasksDataAsync DONE: {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TIMING]   EXCEPTION: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                    _ = CurrentPage.DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK"));
            }
        });

        Debug.WriteLine($"[TIMING] TasksPage.LoadDataAsync returned: {sw.ElapsedMilliseconds}ms");
        return task;
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

                var action = await CurrentPage.DisplayActionSheet(
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
                    var confirm = await CurrentPage.DisplayAlert("Confirmar", "¿Eliminar esta tarea?", "Sí", "No");
                    if (confirm)
                    {
                        await _viewModel.DeleteTaskAsync(taskDisplay);
                    }
                }
            }
            catch (Exception ex)
            {
                await CurrentPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = IPlatformApplication.Current?.Services.GetService<SettingsPage>();
            if (settingsPage != null)
                await RootNavigation.PushAsync(settingsPage);
        }
        catch (Exception ex)
        {
            await CurrentPage.DisplayAlert("Error", $"No se pudo abrir la configuración: {ex.Message}", "OK");
        }
    }

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        await NotificationHelper.ShowPendingTasksAsync(CurrentPage, _taskService, _subjectService);
    }

    private async void OnCreateTask(object sender, EventArgs e)
    {
        try
        {
            await RootNavigation.PushAsync(new NewTaskPage(_subjectService, _taskService));
        }
        catch (Exception ex)
        {
            await CurrentPage.DisplayAlert("Error", $"No se pudo abrir la página de nueva tarea: {ex.Message}", "OK");
        }
    }
}
