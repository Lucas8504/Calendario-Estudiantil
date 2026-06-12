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
    /// Controla qué botón se muestra resaltado.
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

    // Glyph codes for FluentUI icons
    private const string CalendarGlyph = "\uE24F";
    private const string TasksGlyph = "\uE35E";
    private const string SubjectsGlyph = "\uF8FE";

    public BottomNavView()
    {
        InitializeComponent();
        InitializeIcons();

        // Marcar el tab inicial como activo sin animación intermedia
        SetTabVisual("Schedule", isActive: true);
    }

    /// <summary>
    /// Crea los FontImageSource UNA SOLA VEZ y los asigna a cada Image.
    /// Desde acá en adelante solo se muta la propiedad Color, no se reemplaza el Source.
    /// </summary>
    private void InitializeIcons()
    {
        ScheduleIcon.Source = CreateIconSource(CalendarGlyph, InactiveColor);
        TasksIcon.Source = CreateIconSource(TasksGlyph, InactiveColor);
        SubjectsIcon.Source = CreateIconSource(SubjectsGlyph, InactiveColor);
    }

    private static FontImageSource CreateIconSource(string glyph, Color color)
    {
        return new FontImageSource
        {
            Glyph = glyph,
            FontFamily = "FluentIcons",
            Color = color,
            Size = 20
        };
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
    /// Aplica el estado visual a un tab sin recrear objetos.
    /// Solo muta propiedades: Color del FontImageSource, TextColor, FontAttributes, visibilidad del indicator.
    /// </summary>
    private void SetTabVisual(string tab, bool isActive)
    {
        var (icon, label, indicator) = GetTabControls(tab);
        var color = isActive ? HighlightColor : InactiveColor;

        // Mutar el Color del FontImageSource existente — NO reemplazar el Source
        if (icon.Source is FontImageSource fis)
            fis.Color = color;

        label.TextColor = color;
        label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
        indicator.IsVisible = isActive;
    }

    /// <summary>
    /// Devuelve los controles visuales de un tab.
    /// </summary>
    private (Image Icon, Label Label, BoxView Indicator) GetTabControls(string tab) => tab switch
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
