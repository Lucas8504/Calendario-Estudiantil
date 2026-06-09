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

    protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            _ = _viewModel.LoadSubjectsAsync();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error al cargar materias: {ex.Message}", "OK");
        }
    }

    private async void OnSubjectTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Subject subject)
        {
            try
            {
                await Navigation.PushAsync(new SubjectSchedulesPage(subject, _subjectService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir la materia: {ex.Message}", "OK");
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

    private async void OnAddSubject(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new NewSchedulePage(_subjectService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el formulario: {ex.Message}", "OK");
        }
    }
}
