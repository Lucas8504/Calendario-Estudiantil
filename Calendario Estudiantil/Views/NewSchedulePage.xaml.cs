using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.Views;

public partial class NewSchedulePage : ContentPage
{
    private readonly Subject _subject;
    private readonly ISubjectService _subjectService;
    private readonly Schedule? _scheduleToEdit;

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

    // Mapeo inverso: enum a string
    private static readonly Dictionary<DiaSemana, string> ReverseDiaMap = new()
    {
        [DiaSemana.Lunes] = "Lunes",
        [DiaSemana.Martes] = "Martes",
        [DiaSemana.Miércoles] = "Miércoles",
        [DiaSemana.Jueves] = "Jueves",
        [DiaSemana.Viernes] = "Viernes",
        [DiaSemana.Sábado] = "Sábado",
        [DiaSemana.Domingo] = "Domingo"
    };

    /// <summary>
    /// Constructor para crear un nuevo horario.
    /// </summary>
    public NewSchedulePage(Subject subject, ISubjectService subjectService)
    {
        InitializeComponent();
        _subject = subject;
        _subjectService = subjectService;
        _scheduleToEdit = null;

        // Valores por defecto
        DayPicker.SelectedIndex = 0;
        StartTimePicker.Time = new TimeSpan(8, 0, 0);
        EndTimePicker.Time = new TimeSpan(10, 0, 0);
    }

    /// <summary>
    /// Constructor para editar un horario existente.
    /// </summary>
    public NewSchedulePage(Subject subject, Schedule schedule, ISubjectService subjectService)
    {
        InitializeComponent();
        _subject = subject;
        _subjectService = subjectService;
        _scheduleToEdit = schedule;

        // Título y botón para modo edición
        Title = "Editar Horario";
        ((Label)FindByName("PageTitle")).Text = "Editar Horario";
        ((Button)FindByName("SaveButton")).Text = "Guardar";

        // Pre-cargar datos del horario existente
        DayPicker.SelectedItem = ReverseDiaMap[schedule.Day];
        StartTimePicker.Time = schedule.StartTime;
        EndTimePicker.Time = schedule.EndTime;
    }

    private void OnCancel(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void OnSave(object sender, EventArgs e)
    {
        // 1. Validar día
        if (DayPicker.SelectedItem is not string selectedDayName)
        {
            await DisplayAlert("Error", "Seleccione un día.", "OK");
            return;
        }

        // 2. Validar horarios
        var startTime = StartTimePicker.Time;
        var endTime = EndTimePicker.Time;

        if (endTime <= startTime)
        {
            await DisplayAlert("Error", "La hora de fin debe ser mayor a la de inicio.", "OK");
            return;
        }

        // 3. Crear o actualizar el Schedule
        var schedule = _scheduleToEdit ?? new Schedule { SubjectId = _subject.Id };
        schedule.Day = DiaMap[selectedDayName];
        schedule.StartTime = startTime;
        schedule.EndTime = endTime;

        if (_scheduleToEdit != null)
        {
            await _subjectService.UpdateScheduleAsync(schedule);
            await DisplayAlert("Éxito", $"Horario actualizado: {selectedDayName} {startTime:hh\\:mm} - {endTime:hh\\:mm}", "OK");
        }
        else
        {
            await _subjectService.AddScheduleAsync(schedule);
            await DisplayAlert("Éxito", $"Horario agregado: {selectedDayName} {startTime:hh\\:mm} - {endTime:hh\\:mm}", "OK");
        }

        await Navigation.PopAsync();
    }
}