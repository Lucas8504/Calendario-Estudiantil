using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectViewModel _viewModel;
    private readonly ISubjectService _subjectService;

    public SubjectsPage()
    {
        InitializeComponent();
        
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService no disponible");
        
        _viewModel = new SubjectViewModel(_subjectService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadSubjectsAsync();
            SubjectsList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Subjects)));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las materias: {ex.Message}", "OK");
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

    private void OnSearchClicked(object sender, EventArgs e)
    {
        SearchBarFrame.IsVisible = !SearchBarFrame.IsVisible;
        if (SearchBarFrame.IsVisible)
        {
            SearchEntry.Focus();
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.Search(e.NewTextValue);
    }

    private void OnClearSearchClicked(object sender, EventArgs e)
    {
        _viewModel.ClearSearch();
        SearchEntry.Text = string.Empty;
        SearchBarFrame.IsVisible = false;
    }

    private void OnNotificationsClicked(object sender, EventArgs e)
    {
        DisplayAlert("Notificaciones", "Funcionalidad en desarrollo", "OK");
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new SettingsPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la configuración: {ex.Message}", "OK");
        }
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
