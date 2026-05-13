using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Calendario_Estudiantil.Models;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly string[] _colors = { "#512BD4", "#E91E63", "#2196F3", "#4CAF50", "#FF9800", "#9C27B0", "#00BCD4", "#795548" };

    // Constructor sin parámetros para XAML/MAUI
    public SubjectsPage()
    {
        InitializeComponent();
        
        // Resolver servicios desde el DI container
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService no disponible");
        
        _viewModel = new SubjectViewModel(_subjectService);
        BindingContext = _viewModel;
        SubjectsList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Subjects)));
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadSubjectsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las materias: {ex.Message}", "OK");
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

    private async void OnSubjectTapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is Frame frame && frame.BindingContext is Subject subject)
            {
                await Navigation.PushAsync(new SubjectSchedulesPage(subject, _subjectService));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la materia: {ex.Message}", "OK");
        }
    }
}