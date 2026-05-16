using Calendario_Estudiantil.Helpers;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.Views;

public partial class NewTaskPage : ContentPage
{
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;
    private readonly Subject? _preselectedSubject;
    private List<Subject> _subjects = new();

    public NewTaskPage(ISubjectService subjectService, ITaskService taskService, Subject? preselectedSubject = null)
    {
        InitializeComponent();
        _subjectService = subjectService;
        _taskService = taskService;
        _preselectedSubject = preselectedSubject;
        
        // Inicializar fecha por defecto: mañana a las 08:00
        DueDatePicker.Date = DateTime.Today.AddDays(1);
        DueTimePicker.Time = new TimeSpan(8, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSubjectsAsync();
    }

    private async Task LoadSubjectsAsync()
    {
        var subjects = await _subjectService.GetAllAsync();
        _subjects = subjects.ToList();
        
        SubjectPicker.ItemsSource = _subjects;
        if (_subjects.Any())
        {
            // Si hay materia pre-seleccionada, buscarla y seleccionarla
            if (_preselectedSubject != null)
            {
                var index = _subjects.FindIndex(s => s.Id == _preselectedSubject.Id);
                SubjectPicker.SelectedIndex = index >= 0 ? index : 0;
            }
            else
            {
                SubjectPicker.SelectedIndex = 0;
            }
        }
    }

    private async void OnAssignToNextClass(object sender, EventArgs e)
    {
        try
        {
            // Validar que haya materia seleccionada
            if (SubjectPicker.SelectedIndex < 0 || !_subjects.Any())
            {
                await DisplayAlert("Error", "Seleccione una materia primero.", "OK");
                return;
            }

            var selectedSubject = _subjects[SubjectPicker.SelectedIndex];

            // Obtener horarios de la materia
            var schedules = await _subjectService.GetSchedulesAsync(selectedSubject.Id);

            if (!schedules.Any())
            {
                await DisplayAlert("Info", $"La materia '{selectedSubject.Name}' no tiene horarios configurados.", "OK");
                return;
            }

            // Encontrar la próxima clase
            var today = DateTime.Today;
            var currentTime = DateTime.Now.TimeOfDay;

            // Buscar desde hoy hasta dentro de 7 días
            for (int i = 0; i < 7; i++)
            {
                var checkDate = today.AddDays(i);
                var dayOfWeek = DayHelper.FromDayOfWeek(checkDate.DayOfWeek);
                
                var matchingSchedules = schedules
                    .Where(s => s.Day == dayOfWeek)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                // Si es hoy, solo futuros
                if (i == 0)
                {
                    matchingSchedules = matchingSchedules
                        .Where(s => s.StartTime > currentTime)
                        .ToList();
                }

                if (matchingSchedules.Any())
                {
                    var nextSchedule = matchingSchedules.First();
                    var nextDateTime = checkDate.Date + nextSchedule.StartTime;

                    DueDatePicker.Date = nextDateTime.Date;
                    DueTimePicker.Time = nextSchedule.StartTime;

                    await DisplayAlert("Asignado", 
                        $"La tarea vence el {nextDateTime:dd/MM/yyyy} a las {nextSchedule.StartTime:hh\\:mm}\n({DayHelper.GetDayName(nextSchedule.Day)})", 
                        "OK");
                    return;
                }
            }

            await DisplayAlert("Info", "No se encontró ninguna clase programada en los próximos 7 días.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo asignar la fecha: {ex.Message}", "OK");
        }
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSave(object sender, EventArgs e)
    {
        try
        {
            // 1. Validar título
            var title = TitleEntry.Text?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                await DisplayAlert("Error", "El título es obligatorio.", "OK");
                return;
            }

            // 2. Validar materia
            if (SubjectPicker.SelectedIndex < 0 || !_subjects.Any())
            {
                await DisplayAlert("Error", "Debe seleccionar una materia.", "OK");
                return;
            }

            // 3. Validar horas de notificación
            if (!int.TryParse(NotificationHoursEntry.Text, out int notificationHours) || notificationHours < 1)
            {
                await DisplayAlert("Error", "Las horas de notificación deben ser >= 1.", "OK");
                return;
            }

            // 4. Crear la tarea
            var selectedSubject = _subjects[SubjectPicker.SelectedIndex];
            var dueDate = DueDatePicker.Date.Add(DueTimePicker.Time);

            var task = new TaskItem
            {
                SubjectId = selectedSubject.Id,
                Title = title,
                Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                DueDate = dueDate,
                Status = ItemStatus.Pending,
                NotificationHours = notificationHours
            };

            await _taskService.CreateAsync(task);

            await DisplayAlert("Éxito", $"Tarea '{title}' creada para {selectedSubject.Name}.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo crear la tarea: {ex.Message}", "OK");
        }
    }
}