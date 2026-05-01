using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;

namespace Calendario_Estudiantil.Views;

public partial class SubjectPopup : Popup
{
    private readonly Subject _subject;
    private readonly Schedule _schedule;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    public SubjectPopup(Subject subject, Schedule schedule, 
                         ISubjectService subjectService, ITaskService taskService)
    {
        InitializeComponent();
        _subject = subject;
        _schedule = schedule;
        _subjectService = subjectService;
        _taskService = taskService;

        // Cargar datos en UI
        TitleLabel.Text = subject.Name;
        DayLabel.Text = $"Día: {GetDayName(schedule.Day)}";
        TimeLabel.Text = $"Hora: {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}";

        // Cargar tareas de la materia
        LoadTasksAsync();
    }

    private async void LoadTasksAsync()
    {
        var tasks = await _taskService.GetBySubjectAsync(_subject.Id);
        TasksList.ItemsSource = new ObservableCollection<TaskItem>(tasks);
    }

    private string GetDayName(DiaSemana day) => day switch
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

    private void OnEdit(object sender, EventArgs e)
    {
        // Cerrar popup y navegar a SubjectSchedulesPage
        Close();
        Application.Current?.MainPage?.Navigation.PushAsync(
            new SubjectSchedulesPage(_subject, _subjectService));
    }

    private void OnAddTask(object sender, EventArgs e)
    {
        // Cerrar popup y navegar a NewTaskPage
        Close();
        Application.Current?.MainPage?.Navigation.PushAsync(
            new NewTaskPage(_subjectService, _taskService));
    }

    private void OnCancel(object sender, EventArgs e)
    {
        Close();
    }
}