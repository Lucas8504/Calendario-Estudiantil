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

    private static readonly Color InactiveColor = Color.FromArgb("#666666");
    private static readonly Color InactiveColorDark = Color.FromArgb("#A0A0A0");

    // Glyph codes for FluentUI icons
    private const string CalendarGlyph = "\uE24F";
    private const string TasksGlyph = "\uE35E";
    private const string SubjectsGlyph = "\uF8FE";

    public BottomNavView()
    {
        InitializeComponent();
        InitializeIcons();
        UpdateActiveTab(ActiveTab);
    }

    private void InitializeIcons()
    {
        ScheduleIcon.Source = CreateIconSource(CalendarGlyph);
        TasksIcon.Source = CreateIconSource(TasksGlyph);
        SubjectsIcon.Source = CreateIconSource(SubjectsGlyph);
    }

    private static FontImageSource CreateIconSource(string glyph, Color? color = null)
    {
        return new FontImageSource
        {
            Glyph = glyph,
            FontFamily = "FluentIcons",
            Color = color ?? InactiveColor,
            Size = 20
        };
    }

    private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (BottomNavView)bindable;
        view.UpdateActiveTab((string)newValue);
    }

    private void UpdateActiveTab(string tab)
    {
        // Reset todos
        ResetTab(ScheduleIcon, ScheduleLabel, ScheduleIndicator);
        ResetTab(TasksIcon, TasksLabel, TasksIndicator);
        ResetTab(SubjectsIcon, SubjectsLabel, SubjectsIndicator);

        // Resaltar el activo
        switch (tab)
        {
            case "Schedule":
                HighlightTab(ScheduleIcon, ScheduleLabel, ScheduleIndicator);
                break;
            case "Tasks":
                HighlightTab(TasksIcon, TasksLabel, TasksIndicator);
                break;
            case "Subjects":
                HighlightTab(SubjectsIcon, SubjectsLabel, SubjectsIndicator);
                break;
        }
    }

    private static void ResetTab(Image icon, Label label, BoxView indicator)
    {
        icon.Source = CreateIconSource(icon.Source is FontImageSource fis ? fis.Glyph : "");
        label.TextColor = InactiveColor;
        label.FontAttributes = FontAttributes.None;
        indicator.IsVisible = false;
    }

    private static void HighlightTab(Image icon, Label label, BoxView indicator)
    {
        icon.Source = CreateIconSource(icon.Source is FontImageSource fis ? fis.Glyph : "", 
                                        AppConstants.Theme.Accent);
        label.TextColor = AppConstants.Theme.Accent;
        label.FontAttributes = FontAttributes.Bold;
        indicator.IsVisible = true;
    }

    private async void OnScheduleClicked(object? sender, TappedEventArgs e)
    {
        if (ActiveTab == "Schedule") return;
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav == null) return;
        await nav.PopToRootAsync();
    }

    private async void OnTasksClicked(object? sender, TappedEventArgs e)
    {
        if (ActiveTab == "Tasks") return;
        await NavigateToAsync<TasksPage>();
    }

    private async void OnSubjectsClicked(object? sender, TappedEventArgs e)
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
