using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.Views;

public partial class NewSchedulePage : ContentPage
{
    private readonly Subject _subject;
    private readonly ISubjectService _subjectService;

    // Mapeo de días string a enum
    private static readonly Dictionary<string, DiaSemana> DiaMap = new()
    {
        ["Lunes"] = DiaSemana.Lunes,
        ["Martes"] = DiaSemana.Martes,
        ["Miércoles"] = DiaSemana.Miércoles,
        ["Jueves"] = DiaSemana.Jueves,
        ["Viernes"] = DiaSemana.Viernes,
        ["Sábado"] = DiaSemana.Sábado,
        ["Domingo"] = DiaSemana.Domingo
    };

    public NewSchedulePage(Subject subject, ISubjectService subjectService)
    {
        InitializeComponent();
        _subject = subject;
        _subjectService = subjectService;

        // Valores por defecto
        DayPicker.SelectedIndex = 0;
        StartTimePicker.Time = new TimeSpan(8, 0, 0);
        EndTimePicker.Time = new TimeSpan(10, 0, 0);
    }

    private void OnCancel(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void OnSave(object sender, EventArgs e)
    {
        // 1. Validar día
        if (DayPicker.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Seleccione un día.", "OK");
            return;
        }
        var selectedDayName = DayPicker.Items[DayPicker.SelectedIndex];

        // 2. Validar horarios
        var startTime = StartTimePicker.Time;
        var endTime = EndTimePicker.Time;

        if (endTime <= startTime)
        {
            await DisplayAlert("Error", "La hora de fin debe ser mayor a la de inicio.", "OK");
            return;
        }

        // 3. Crear el Schedule
        var schedule = new Schedule
        {
            SubjectId = _subject.Id,
            Day = DiaMap[selectedDayName],
            StartTime = startTime,
            EndTime = endTime
        };

        await _subjectService.AddScheduleAsync(schedule);

        await DisplayAlert("Éxito", $"Horario agregado: {selectedDayName} {startTime:hh\\:mm} - {endTime:hh\\:mm}", "OK");
        await Navigation.PopAsync();
    }
}