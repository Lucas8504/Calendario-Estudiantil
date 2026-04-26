using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleGridViewModel _viewModel;

    public SchedulePage()
    {
        InitializeComponent();
        _viewModel = new ScheduleGridViewModel(App.subjectService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadGridAsync();
        BuildGridUI();
    }

    private void BuildGridUI()
    {
        var grid = GridContainer;
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        var minHour = _viewModel.MinHour;
        var totalHours = _viewModel.TotalHours;
        var daysCount = _viewModel.DayHeaders.Count;
        var cells = _viewModel.Cells;
        var dayHeaders = _viewModel.DayHeaders;

        // DEBUG: mostrar valores
        System.Diagnostics.Debug.WriteLine($"=== BUILD GRID ===");
        System.Diagnostics.Debug.WriteLine($"minHour={minHour}, totalHours={totalHours}, daysCount={daysCount}");
        System.Diagnostics.Debug.WriteLine($"Cells count={cells.Count}");
        System.Diagnostics.Debug.WriteLine($"DayHeaders: {string.Join(", ", dayHeaders.Select(h => $"DayIndex={h.DayIndex}"))}");
        
        // Contar materias por hora
        for (int h = minHour; h < minHour + totalHours; h++)
        {
            var count = cells.Count(c => c.Hour == h && c.HasClass);
            if (count > 0)
                System.Diagnostics.Debug.WriteLine($"Hour {h}:00 has {count} class(es)");
        }

        if (daysCount == 0 || totalHours == 0) return;

        // Medidas fijas
        const int H = 70;  // columna hora
        const int D = 90;  // columna día
        const int R = 55;  // altura fila

        int totalW = H + (daysCount * D);
        grid.WidthRequest = totalW;
        grid.HeightRequest = (totalHours + 1) * R;

        // Columnas: 1 hora + N días
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(H, GridUnitType.Absolute) });
        for (int d = 0; d < daysCount; d++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(D, GridUnitType.Absolute) });

        // Filas: header + N horas
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(R, GridUnitType.Absolute) });
        for (int h = 0; h < totalHours; h++)
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(R, GridUnitType.Absolute) });

        // ==================== FILA 0: Headers ====================
        // [0,0] - Esquina "Hora"
        var corner = new Label {
            Text = "Hora",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            BackgroundColor = Color.FromArgb("#DDDDDD"),
            WidthRequest = H,
            HeightRequest = R
        };
        grid.Children.Add(corner);
        Grid.SetRow(corner, 0);
        Grid.SetColumn(corner, 0);

        // Headers de días en columnas 1, 2, 3...
        for (int d = 0; d < daysCount; d++)
        {
            var dh = dayHeaders[d];
            var bg = dh.IsToday ? Color.FromArgb("#E3F2FD") : Color.FromArgb("#DDDDDD");
            
            var hdr = new Label {
                Text = dh.DayName + "\n" + dh.DayNumber,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = bg,
                TextColor = Color.FromArgb("#333333"),
                WidthRequest = D,
                HeightRequest = R
            };
            grid.Children.Add(hdr);
            Grid.SetRow(hdr, 0);
            Grid.SetColumn(hdr, d + 1);
        }

        // ==================== FILAS 1..N: Horarios ====================
        for (int hi = 0; hi < totalHours; hi++)
        {
            var hour = minHour + hi;
            var gridRow = hi + 1; // Fila 1, 2, 3...

            // Columna 0: la hora
            var hourLbl = new Label {
                Text = hour + ":00",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#666666"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Color.FromArgb("#F0F0F0"),
                WidthRequest = H,
                HeightRequest = R
            };
            grid.Children.Add(hourLbl);
            Grid.SetRow(hourLbl, gridRow);
            Grid.SetColumn(hourLbl, 0);

            // Celdas de cada día
            for (int di = 0; di < daysCount; di++)
            {
                var dh = dayHeaders[di];
                var gridCol = di + 1; // Columna 1, 2, 3...
                
                // DEBUG: ver qué valores se usan para buscar
                var cell = cells.FirstOrDefault(c => c.DayIndex == dh.DayIndex && c.Hour == hour);
                System.Diagnostics.Debug.WriteLine($"Looking for: DayIndex={dh.DayIndex}, Hour={hour} -> Found={(cell != null)}, HasClass={(cell?.HasClass == true)}, Subject={(cell?.Subject?.Name ?? "null")}");
                
                var hasClass = cell != null && cell.HasClass;
                var bg = hasClass ? Color.FromArgb(cell.Subject.ColorHex) : Color.FromArgb("#FFFFFF");
                var txt = hasClass ? cell.Subject.Name : "";
                var col = hasClass ? Colors.White : Color.FromArgb("#333333");
                
                var cel = new Label {
                    Text = txt,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = col,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    BackgroundColor = bg,
                    WidthRequest = D,
                    HeightRequest = R
                };
                grid.Children.Add(cel);
                Grid.SetRow(cel, gridRow);
                Grid.SetColumn(cel, gridCol);
            }
        }
    }

    private async void OnPreviousWeek(object sender, EventArgs e)
    {
        await _viewModel.PreviousWeekAsync();
        BuildGridUI();
    }

    private async void OnNextWeek(object sender, EventArgs e)
    {
        await _viewModel.NextWeekAsync();
        BuildGridUI();
    }

    private async void OnGoToToday(object sender, EventArgs e)
    {
        await _viewModel.GoToTodayAsync();
        BuildGridUI();
    }
}