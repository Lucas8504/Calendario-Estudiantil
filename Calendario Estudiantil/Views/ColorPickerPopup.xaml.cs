using Calendario_Estudiantil.Services;
using CommunityToolkit.Maui.Views;

namespace Calendario_Estudiantil.Views;

public partial class ColorPickerPopup : Popup
{
    private readonly ISubjectService _subjectService;
    private readonly string[] _colors = {
        // Rojos/Naranjas
        "#8E1B2D", "#E91E63", "#FF5722", "#FF9800",
        // Amarillos/Verdes
        "#FFC107", "#8BC34A", "#4CAF50", "#009688",
        // Azules/Cianes
        "#2196F3", "#03A9F4", "#00BCD4", "#1FE8EA",
        // Violetas/Rosas
        "#9C27B0", "#E040FB", "#FF4081", "#673AB7",
        // Grises/Marrones
        "#D1EBFC", "#9E9E9E", "#607D8B", "#455A64",
        // Colores extra
        "#00796B", "#CDDC39", "#8E89EA", "#FFCEB8"
    };

    public event EventHandler<string>? ColorSelected;

    public ColorPickerPopup(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        InitializeComponent();
        LoadColors();
    }

    private void LoadColors()
    {
        for (int i = 0; i < _colors.Length; i++)
        {
            var color = _colors[i];
            var button = new Button
            {
                BackgroundColor = Color.FromArgb(color),
                WidthRequest = 50,
                HeightRequest = 50,
                CornerRadius = 8,
                BorderColor = Colors.Gray,
                BorderWidth = 1
            };

            var col = i % 4;
            var row = i / 4;

            button.Clicked += (s, e) =>
            {
                ColorSelected?.Invoke(this, color);
                Close();
            };

            ColorsGrid.Add(button, col, row);
        }
    }

    private async void OnRandomColor(object sender, EventArgs e)
    {
        // Obtener colores ya usados
        var subjects = await _subjectService.GetAllAsync();
        var usedColors = subjects.Select(s => s.ColorHex.ToUpper()).ToHashSet();

        // Filtrar colores disponibles
        var availableColors = _colors.Where(c => !usedColors.Contains(c.ToUpper())).ToList();

        // Si no hay disponibles, usar cualquiera
        if (!availableColors.Any())
            availableColors = _colors.ToList();

        // Elegir color aleatorio
        var random = new Random();
        var randomColor = availableColors[random.Next(availableColors.Count)];

        ColorSelected?.Invoke(this, randomColor);
        Close();
    }

    private void OnCancel(object sender, EventArgs e)
    {
        Close();
    }
}
