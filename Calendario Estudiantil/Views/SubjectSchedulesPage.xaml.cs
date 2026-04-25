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

    private readonly string[] _dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

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
        // Paso 1: Elegir día
        var dia = await DisplayActionSheet("Día de clase:", "Cancelar", null, _dias);
        if (string.IsNullOrEmpty(dia) || dia == "Cancelar") return;

        // Paso 2: Hora de inicio
        var horaInicio = await DisplayPromptAsync("Hora de inicio", "Ej: 08:00", "Aceptar", "Cancelar");
        if (string.IsNullOrWhiteSpace(horaInicio)) return;
        if (!TimeSpan.TryParse(horaInicio, out var startTime))
        {
            await DisplayAlert("Error", "Hora inválida. Usá formato 24h: 08:00", "OK");
            return;
        }

        // Paso 3: Hora de fin
        var horaFin = await DisplayPromptAsync("Hora de fin", "Ej: 10:00", "Aceptar", "Cancelar");
        if (string.IsNullOrWhiteSpace(horaFin)) return;
        if (!TimeSpan.TryParse(horaFin, out var endTime))
        {
            await DisplayAlert("Error", "Hora inválida. Usá formato 24h: 10:00", "OK");
            return;
        }

        if (endTime <= startTime)
        {
            await DisplayAlert("Error", "La hora de fin debe ser mayor a la de inicio", "OK");
            return;
        }

        // Crear schedule
        var schedule = new Schedule
        {
            SubjectId = _subject.Id,
            Day = (DiaSemana)(Array.IndexOf(_dias, dia) + 1),
            StartTime = startTime,
            EndTime = endTime
        };

        await _subjectService.AddScheduleAsync(schedule);
        await LoadSchedulesAsync();
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

    public string DayName => Schedule.Day switch
    {
        DiaSemana.Lunes => "Lunes",
        DiaSemana.Martes => "Martes",
        DiaSemana.Miércoles => "Miércoles",
        DiaSemana.Jueves => "Jueves",
        DiaSemana.Viernes => "Viernes",
        DiaSemana.Sábado => "Sábado",
        DiaSemana.Domingo => "Domingo",
        _ => ""
    };

    public string TimeDisplay => $"{Schedule.StartTime:hh\\:mm} - {Schedule.EndTime:hh\\:mm}";
}