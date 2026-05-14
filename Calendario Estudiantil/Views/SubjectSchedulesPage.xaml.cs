using Calendario_Estudiantil.Helpers;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using System.Collections.ObjectModel;

namespace Calendario_Estudiantil.Views;

public partial class SubjectSchedulesPage : ContentPage
{
    private readonly Subject _subject;
    private readonly ISubjectService _subjectService;
    private ObservableCollection<ScheduleDisplay> _schedules = new();

    public string SubjectName => _subject.Name;

    public SubjectSchedulesPage(Subject subject, ISubjectService subjectService)
    {
        InitializeComponent();
        _subject = subject;
        _subjectService = subjectService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSchedulesAsync();
    }

    private async Task LoadSchedulesAsync()
    {
        var list = await _subjectService.GetSchedulesAsync(_subject.Id);
        _schedules = new ObservableCollection<ScheduleDisplay>(
            list.Select(s => new ScheduleDisplay { Schedule = s, Subject = _subject })
        );
        SchedulesList.ItemsSource = _schedules;
    }

    private async void OnAddSchedule(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewSchedulePage(_subject, _subjectService));
    }

    private async void OnDeleteSchedule(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ScheduleDisplay display)
        {
            var confirmar = await DisplayAlert("Eliminar", $"Eliminar horario de {display.DayName}?", "Sí", "No");
            if (confirmar)
            {
                await _subjectService.DeleteScheduleAsync(display.Schedule);
                await LoadSchedulesAsync();
            }
        }
    }

    private async void OnSave(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

public class ScheduleDisplay
{
    public Schedule Schedule { get; set; } = null!;
    public Subject Subject { get; set; } = null!;

    public string DayName => DayHelper.GetDayName(Schedule.Day);
    public string TimeDisplay => $"{Schedule.StartTime:hh\\:mm} - {Schedule.EndTime:hh\\:mm}";
}