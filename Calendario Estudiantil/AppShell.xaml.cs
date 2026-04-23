namespace Calendario_Estudiantil;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private async void OnNavigateSchedule(object sender, EventArgs e)
        => await GoToAsync("//SchedulePage");

    private async void OnNavigateTasks(object sender, EventArgs e)
        => await GoToAsync("//TasksPage");

    private async void OnNavigateSubjects(object sender, EventArgs e)
        => await GoToAsync("//SubjectsPage");

    private async void OnNavigateCourses(object sender, EventArgs e)
        => await GoToAsync("//CoursesPage");
}