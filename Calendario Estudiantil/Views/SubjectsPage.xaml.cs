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
        
        var courseService = IPlatformApplication.Current?.Services
            .GetRequiredService<ICourseService>()
            ?? throw new InvalidOperationException("ICourseService no disponible");
        
        _viewModel = new SubjectViewModel(_subjectService, courseService);
        BindingContext = _viewModel;
        SubjectsList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Subjects)));
    }

    // Constructor con parámetros para navegación programática (mantener compatibilidad)
    public SubjectsPage(ISubjectService subjectService, ICourseService courseService) : this()
    {
        // El constructor sin parámetros ya inicializa todo
        // Este constructor existe para mantener compatibilidad con código existente
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadSubjectsNoCourseAsync();
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
            // Paso 1: Nombre
            var name = await DisplayPromptAsync("Nueva Materia", "Nombre de la materia:", "Crear", "Cancelar", maxLength: 50);
            if (string.IsNullOrWhiteSpace(name)) return;

            // Paso 2: Selección de color con Popup visual
            var popup = new ColorPickerPopup(_subjectService);
            popup.ColorSelected += async (s, color) =>
            {
                try
                {
                    // Crear materia (sin carrera)
                    await _viewModel.CreateSubjectNoCourseAsync(name, color);
                    
                    await DisplayAlert("Materia creada", $"Ahora '{name}' aparece en el calendario.\n\nPara agregar horarios, tocá la materia en la lista.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo crear la materia: {ex.Message}", "OK");
                }
            };
            
            this.ShowPopup(popup);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo crear la materia: {ex.Message}", "OK");
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