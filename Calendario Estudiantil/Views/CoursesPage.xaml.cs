using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesViewModel _viewModel;
    private readonly ICourseService _courseService;

    // Constructor sin parámetros para XAML/MAUI
    public CoursesPage()
    {
        InitializeComponent();
        
        // Resolver servicios desde el DI container
        _courseService = IPlatformApplication.Current?.Services
            .GetRequiredService<ICourseService>()
            ?? throw new InvalidOperationException("ICourseService no disponible");
        
        _viewModel = new CoursesViewModel(_courseService);
        BindingContext = _viewModel;
        CoursesList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Courses)));
    }

    // Constructor con parámetros para navegación programática (mantener compatibilidad)
    public CoursesPage(ICourseService courseService) : this()
    {
        // El constructor sin parámetros ya inicializa todo
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadCoursesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las carreras: {ex.Message}", "OK");
        }
    }

    private async void OnAddCourse(object sender, EventArgs e)
    {
        try
        {
            var name = await DisplayPromptAsync("Nueva Carrera", "Nombre de la carrera:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _viewModel.CreateCourseAsync(name);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo crear la carrera: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteCourse(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.BindingContext is Models.Course course)
            {
                await _viewModel.DeleteCourseAsync(course);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo eliminar la carrera: {ex.Message}", "OK");
        }
    }
}