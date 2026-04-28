using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.Views;

public partial class NewTaskPage : ContentPage
{
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;
    private List<Subject> _subjects = new();

    public NewTaskPage(ISubjectService subjectService, ITaskService taskService)
    {
        InitializeComponent();
        _subjectService = subjectService;
        _taskService = taskService;
        
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
            SubjectPicker.SelectedIndex = 0;
        }
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSave(object sender, EventArgs e)
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
}