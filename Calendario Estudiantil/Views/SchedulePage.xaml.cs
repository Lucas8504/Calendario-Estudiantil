using Calendario_Estudiantil.ViewModels;

namespace Calendario_Estudiantil.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _viewModel;

    public SchedulePage()
    {
        InitializeComponent();
        _viewModel = new ScheduleViewModel(App.subjectService, App.courseService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSchedulesAsync();
    }

    private async void OnPreviousWeek(object sender, EventArgs e)
        => await _viewModel.PreviousWeekAsync();

    private async void OnNextWeek(object sender, EventArgs e)
        => await _viewModel.NextWeekAsync();

    private async void OnGoToToday(object sender, EventArgs e)
        => await _viewModel.GoToTodayAsync();
}