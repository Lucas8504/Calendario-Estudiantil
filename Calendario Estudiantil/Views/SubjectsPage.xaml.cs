using Calendario_Estudiantil.ViewModels;

namespace Calendario_Estudiantil.Views;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectViewModel _viewModel;

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
        await _viewModel.LoadSubjectsAsync();
    }

    private async void OnAddSubject(object sender, EventArgs e)
    {
        if (App.courseService.GetActiveAsync().Result == null)
        {
            await DisplayAlert("Atención", "Primero seleccioná una carrera.", "OK");
            return;
        }

        var name = await DisplayPromptAsync("Nueva Materia", "Nombre:");
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _viewModel.CreateSubjectAsync(name);
        }
    }
}