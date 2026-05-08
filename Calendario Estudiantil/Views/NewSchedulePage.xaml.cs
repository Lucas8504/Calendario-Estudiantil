using System;
using System.Collections.Generic;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using CommunityToolkit.Maui.Views;

namespace Calendario_Estudiantil.Views;

public partial class NewSchedulePage : ContentPage
{
    private readonly Subject _subject;
    private readonly ISubjectService _subjectService;
    private readonly Schedule? _scheduleToEdit;
    private string? _selectedDay;
    private string _subjectColor;

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
        _subjectColor = subject.ColorHex;

        // Cargar datos de la materia
        SubjectNameEntry.Text = subject.Name;
        ColorPreview.BackgroundColor = Color.FromArgb(_subjectColor);

        // Valores por defecto
        _selectedDay = "Lunes";
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
        _subjectColor = subject.ColorHex;
        _selectedDay = ReverseDiaMap[schedule.Day];

        // Cargar datos de la materia
        SubjectNameEntry.Text = subject.Name;
        ColorPreview.BackgroundColor = Color.FromArgb(_subjectColor);

        // Título y botón para modo edición
        Title = "Editar Horario";
        PageTitle.Text = "Editar Horario";
        SaveButton.Text = "Guardar";

        // Pre-cargar horas
        StartTimePicker.Time = schedule.StartTime;
        EndTimePicker.Time = schedule.EndTime;

        InitializeDayButtons();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeDayButtons();
    }

    private void InitializeDayButtons()
    {
        DayGrid.Children.Clear();
        var days = new List<string> { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
        var fullDays = new List<string> { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

        // Si no hay día seleccionado y es modo creación, seleccionar Lunes por defecto
        if (_selectedDay == null && _scheduleToEdit == null)
        {
            _selectedDay = "Lunes";
        }

        for (int i = 0; i < 7; i++)
        {
            var dayName = days[i];
            var fullDay = fullDays[i];
            var isSelected = _selectedDay == fullDay;

            var btn = new Button
            {
                Text = dayName,
                WidthRequest = 42,
                HeightRequest = 42,
                CornerRadius = 8,
                Margin = new Thickness(2),
                Padding = new Thickness(0),
                FontSize = 12,
                FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
                BackgroundColor = isSelected ? Color.FromArgb("#512BD4") : Color.FromArgb("#F0F0F0"),
                TextColor = isSelected ? Colors.White : Color.FromArgb("#333333"),
                BorderColor = isSelected ? Color.FromArgb("#3A1E99") : Color.FromArgb("#CCCCCC"),
                BorderWidth = 1
            };

            // Capturar variable para closure
            var currentFullDay = fullDay;
            btn.Clicked += (s, e) => SelectDay(btn, currentFullDay, days);

            DayGrid.Children.Add(btn);
        }
    }

    private void SelectDay(Button selectedBtn, string dayName, List<string> allDays)
    {
        _selectedDay = dayName;

        // Actualizar visualmente todos los botones
        foreach (var child in DayGrid.Children)
        {
            if (child is Button btn)
            {
                var isThisBtn = btn.Text == selectedBtn.Text;
                btn.BackgroundColor = isThisBtn ? Color.FromArgb("#512BD4") : Color.FromArgb("#F0F0F0");
                btn.TextColor = isThisBtn ? Colors.White : Color.FromArgb("#333333");
                btn.BorderColor = isThisBtn ? Color.FromArgb("#3A1E99") : Color.FromArgb("#CCCCCC");
                btn.FontAttributes = isThisBtn ? FontAttributes.Bold : FontAttributes.None;
            }
        }
    }

    private void OnColorTapped(object sender, EventArgs e)
    {
        var popup = new ColorPickerPopup(_subjectService);
        popup.ColorSelected += (s, color) =>
        {
            _subjectColor = color;
            ColorPreview.BackgroundColor = Color.FromArgb(color);
        };

        this.ShowPopup(popup);
    }

    private void OnCancel(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void OnSave(object sender, EventArgs e)
    {
        // 1. Validar nombre de la materia
        var newName = SubjectNameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            await DisplayAlert("Error", "El nombre de la materia no puede estar vacío.", "OK");
            return;
        }

        // 2. Validar día
        if (string.IsNullOrEmpty(_selectedDay) || !DiaMap.ContainsKey(_selectedDay))
        {
            await DisplayAlert("Error", "Seleccione un día.", "OK");
            return;
        }

        // 3. Validar horarios
        var startTime = StartTimePicker.Time;
        var endTime = EndTimePicker.Time;

        if (endTime <= startTime)
        {
            await DisplayAlert("Error", "La hora de fin debe ser mayor a la de inicio.", "OK");
            return;
        }

        // 4. Guardar cambios de la materia (nombre + color)
        _subject.Name = newName;
        _subject.ColorHex = _subjectColor;
        await _subjectService.UpdateAsync(_subject);

        // 5. Crear o actualizar el Schedule
        var schedule = _scheduleToEdit ?? new Schedule { SubjectId = _subject.Id };
        schedule.Day = DiaMap[_selectedDay!];
        schedule.StartTime = startTime;
        schedule.EndTime = endTime;

        if (_scheduleToEdit != null)
        {
            await _subjectService.UpdateScheduleAsync(schedule);
            await DisplayAlert("Éxito", $"Horario actualizado: {_selectedDay} {startTime:hh\\:mm} - {endTime:hh\\:mm}", "OK");
        }
        else
        {
            await _subjectService.AddScheduleAsync(schedule);
            await DisplayAlert("Éxito", $"Horario agregado: {_selectedDay} {startTime:hh\\:mm} - {endTime:hh\\:mm}", "OK");
        }

        await Navigation.PopAsync();
    }
}
