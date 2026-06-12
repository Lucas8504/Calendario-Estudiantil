using System.Diagnostics;
using Calendario_Estudiantil.Helpers;
using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace Calendario_Estudiantil.Views;

    public partial class SubjectsPage : ContentPage
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

        private readonly SubjectViewModel _viewModel;
        private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    public SubjectsPage(SubjectViewModel viewModel, ISubjectService subjectService, ITaskService taskService)
    {
        InitializeComponent();
        SubtitleLabel.Text = DateTime.Now.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("es-ES")).ToUpperInvariant();
        
        _viewModel = viewModel;
        _subjectService = subjectService;
        _taskService = taskService;
        
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Carga/refresca los datos de la página.
    /// Llamado por MainPage cuando este tab se vuelve visible.
    /// </summary>
    public Task LoadDataAsync()
    {
        var sw = Stopwatch.StartNew();
        Debug.WriteLine($"[TIMING] SubjectsPage.LoadDataAsync START");

        var task = Task.Run(async () =>
        {
            try
            {
                await _viewModel.LoadSubjectsAsync();
                Debug.WriteLine($"[TIMING]   LoadSubjectsAsync completed: {sw.ElapsedMilliseconds}ms");
                Debug.WriteLine($"[TIMING] SubjectsPage.LoadDataAsync DONE (total async): {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TIMING]   LoadSubjectsAsync FAILED: {ex.Message}");
            }
        });

        Debug.WriteLine($"[TIMING] SubjectsPage.LoadDataAsync returned: {sw.ElapsedMilliseconds}ms");
        return task;
    }

    private async void OnSubjectTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Subject subject)
        {
            try
            {
                await RootNavigation.PushAsync(new SubjectSchedulesPage(subject, _subjectService));
            }
            catch (Exception ex)
            {
                await CurrentPage.DisplayAlert("Error", $"No se pudo abrir la materia: {ex.Message}", "OK");
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

    private async void OnAddSubject(object sender, EventArgs e)
    {
        try
        {
            await RootNavigation.PushAsync(new NewSchedulePage(_subjectService));
        }
        catch (Exception ex)
        {
            await CurrentPage.DisplayAlert("Error", $"No se pudo abrir el formulario: {ex.Message}", "OK");
        }
    }
}
