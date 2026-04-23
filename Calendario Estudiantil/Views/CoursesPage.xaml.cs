using Calendario_Estudiantil.ViewModels;

namespace Calendario_Estudiantil.Views;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesViewModel _viewModel;

    public CoursesPage()
    {
        InitializeComponent();
        _viewModel = new CoursesViewModel(App.courseService);
        BindingContext = _viewModel;
        CoursesList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Courses)));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCoursesAsync();
    }

    private async void OnAddCourse(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Nueva Carrera", "Nombre de la carrera:");
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _viewModel.CreateCourseAsync(name);
        }
    }

    private async void OnDeleteCourse(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Models.Course course)
        {
            await _viewModel.DeleteCourseAsync(course);
        }
    }
}