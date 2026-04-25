using Calendario_Estudiantil.ViewModels;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Views;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectViewModel _viewModel;

    // Colores disponibles
    private readonly string[] _colors = { "#512BD4", "#E91E63", "#2196F3", "#4CAF50", "#FF9800", "#9C27B0", "#00BCD4", "#795548" };

    public SubjectsPage()
    {
        InitializeComponent();
        _viewModel = new SubjectViewModel(App.subjectService, App.courseService);
        BindingContext = _viewModel;
        SubjectsList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Subjects)));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSubjectsNoCourseAsync();
    }

    private async void OnAddSubject(object sender, EventArgs e)
    {
        // Paso 1: Nombre
        var name = await DisplayPromptAsync("Nueva Materia", "Nombre de la materia:", "Crear", "Cancelar", maxLength: 50);
        if (string.IsNullOrWhiteSpace(name)) return;

        // Paso 2: Selección de color
        var selectedColor = await DisplayActionSheet("Elegir color:", "Cancelar", null, _colors);
        if (string.IsNullOrEmpty(selectedColor) || selectedColor == "Cancelar") return;

        // Crear materia (sin carrera)
        await _viewModel.CreateSubjectNoCourseAsync(name, selectedColor);
        
        await DisplayAlert("Materia creada", $"Ahora '{name}' aparece en el calendario.\n\nPara agregar horarios,tocá la materia en la lista.", "OK");
    }

    private async void OnSubjectTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Subject subject)
        {
            await Navigation.PushAsync(new SubjectSchedulesPage(subject, App.subjectService));
        }
    }
}