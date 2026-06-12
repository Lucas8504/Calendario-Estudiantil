using Calendario_Estudiantil.Configuration;
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
    /// </summary>
    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    /// <summary>
    /// Se dispara cuando el usuario toca un tab.
    /// MainPage se suscribe para cambiar el contenido visible.
    /// </summary>
    public event EventHandler<string>? TabSelected;

    private static readonly Color InactiveColor = Color.FromArgb("#666666");
    private static readonly Color HighlightColor = AppConstants.Theme.Accent;

    public BottomNavView()
    {
        InitializeComponent();

        // Marcar el tab inicial como activo (el Source ya viene del XAML)
        SetTabVisual("Schedule", isActive: true);
    }

    private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (BottomNavView)bindable;
        view.UpdateActiveTab((string)oldValue, (string)newValue);
    }

    /// <summary>
    /// Actualiza SOLO el tab que cambió: resetea el anterior, resalta el nuevo.
    /// No toca los otros 2 tabs, evitando re-render innecesario.
    /// </summary>
    private void UpdateActiveTab(string? oldTab, string newTab)
    {
        if (oldTab == newTab) return;

        if (oldTab != null)
            SetTabVisual(oldTab, isActive: false);

        SetTabVisual(newTab, isActive: true);
    }

    /// <summary>
    /// Aplica el estado visual a un tab cambiando solo TextColor y FontAttributes.
    /// Usa Labels con FontFamily="FluentIcons" — más rápido que FontImageSource
    /// porque renderiza con el text engine nativo sin rasterizar a bitmap.
    /// </summary>
    private void SetTabVisual(string tab, bool isActive)
    {
        var (iconLabel, textLabel, indicator) = GetTabControls(tab);
        var color = isActive ? HighlightColor : InactiveColor;

        iconLabel.TextColor = color;
        textLabel.TextColor = color;
        textLabel.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
        indicator.IsVisible = isActive;
    }

    /// <summary>
    /// Devuelve los controles visuales de un tab.
    /// </summary>
    private (Label IconLabel, Label TextLabel, BoxView Indicator) GetTabControls(string tab) => tab switch
    {
        "Tasks" => (TasksIcon, TasksLabel, TasksIndicator),
        "Subjects" => (SubjectsIcon, SubjectsLabel, SubjectsIndicator),
        _ => (ScheduleIcon, ScheduleLabel, ScheduleIndicator),
    };

    private void OnScheduleClicked(object? sender, TappedEventArgs e)
    {
        if (ActiveTab == "Schedule") return;
        TabSelected?.Invoke(this, "Schedule");
    }

    private void OnTasksClicked(object? sender, TappedEventArgs e)
    {
        if (ActiveTab == "Tasks") return;
        TabSelected?.Invoke(this, "Tasks");
    }

    private void OnSubjectsClicked(object? sender, TappedEventArgs e)
    {
        if (ActiveTab == "Subjects") return;
        TabSelected?.Invoke(this, "Subjects");
    }
}
