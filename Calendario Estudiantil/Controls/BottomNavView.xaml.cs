using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Views;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Controls;

public partial class BottomNavView : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(
            nameof(ActiveTab), typeof(string), typeof(BottomNavView),
            defaultValue: "Schedule",
            propertyChanged: OnActiveTabChanged);

    /// <summary>
    /// Tab activo: "Schedule", "Tasks" o "Subjects".
    /// Controla qué botón se muestra resaltado.
    /// </summary>
    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public BottomNavView()
    {
        InitializeComponent();
        UpdateActiveTab(ActiveTab);
    }

    private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (BottomNavView)bindable;
        view.UpdateActiveTab((string)newValue);
    }

    private void UpdateActiveTab(string tab)
    {
        // Reset todos
        ResetButton(ScheduleBtn, ScheduleIndicator);
        ResetButton(TasksBtn, TasksIndicator);
        ResetButton(SubjectsBtn, SubjectsIndicator);

        // Resaltar el activo
        switch (tab)
        {
            case "Schedule":
                HighlightButton(ScheduleBtn, ScheduleIndicator);
                break;
            case "Tasks":
                HighlightButton(TasksBtn, TasksIndicator);
                break;
            case "Subjects":
                HighlightButton(SubjectsBtn, SubjectsIndicator);
                break;
        }
    }

    private static void ResetButton(Button btn, BoxView indicator)
    {
        btn.TextColor = Color.FromArgb("#666666");
        btn.FontAttributes = FontAttributes.None;
        indicator.IsVisible = false;
    }

    private static void HighlightButton(Button btn, BoxView indicator)
    {
        btn.TextColor = AppConstants.Theme.Accent;
        btn.FontAttributes = FontAttributes.Bold;
        indicator.IsVisible = true;
    }

    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        if (ActiveTab == "Schedule") return;
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav == null) return;
        await nav.PopToRootAsync();
    }

    private async void OnTasksClicked(object sender, EventArgs e)
    {
        if (ActiveTab == "Tasks") return;
        await NavigateToAsync<TasksPage>();
    }

    private async void OnSubjectsClicked(object sender, EventArgs e)
    {
        if (ActiveTab == "Subjects") return;
        await NavigateToAsync<SubjectsPage>();
    }

    /// <summary>
    /// Navega a la página T, evitando duplicados en el stack.
    /// Si ya existe una instancia, vuelve a ella.
    /// Si no, la resuelve desde DI y reemplaza la página secundaria actual
    /// para mantener el stack siempre en [Schedule, CurrentPage].
    /// </summary>
    private async Task NavigateToAsync<T>() where T : Page
    {
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav == null) return;

        var stack = nav.NavigationStack;

        // Ya estamos en esa página
        if (stack.Count > 0 && stack[^1] is T)
            return;

        // Buscar si ya existe en el stack (excluyendo root) y volver a ella
        for (int i = 1; i < stack.Count; i++)
        {
            if (stack[i] is T)
            {
                while (stack.Count > i + 1)
                    nav.RemovePage(stack[^1]);
                return;
            }
        }

        // No existe: resolver desde DI
        var services = IPlatformApplication.Current?.Services;
        var page = services?.GetService<T>();
        if (page == null) return;

        // Reemplazar la página secundaria actual si existe,
        // para que el stack nunca pase de [Schedule, CurrentPage]
        if (stack.Count >= 2)
        {
            var currentTop = stack[^1];
            nav.InsertPageBefore(page, currentTop);
            nav.RemovePage(currentTop);
        }
        else
        {
            await nav.PushAsync(page);
        }
    }
}
