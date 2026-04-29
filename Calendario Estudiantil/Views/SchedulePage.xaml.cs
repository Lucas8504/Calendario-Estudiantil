using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
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

        if (daysCount == 0 || totalHours == 0) return;

        // Usar constantes de configuración
        const int H = AppConstants.ScheduleGrid.HourColumnWidth;
        const int D = AppConstants.ScheduleGrid.DayColumnWidth;
        const int R = AppConstants.ScheduleGrid.RowHeight;

        int totalW = H + (daysCount * D);
        grid.WidthRequest = totalW;
        grid.HeightRequest = (totalHours + 1) * R;
        grid.Margin = new Thickness(0);
        grid.Padding = new Thickness(0);

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
            var gridRow = hi + 1;

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
                var gridCol = di + 1;
                
                var cell = cells.FirstOrDefault(c => c.DayIndex == dh.DayIndex && c.Hour == hour);
                
                // Usar pattern matching para evitar warnings de nulabilidad
                if (cell != null && cell.HasClass && cell.Subject != null)
                {
                    var subject = cell.Subject;
                    var bg = Color.FromArgb(subject.ColorHex);
                    var txt = subject.Name;
                    
                    var cel = new Label {
                        Text = txt,
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        BackgroundColor = bg,
                        WidthRequest = D,
                        HeightRequest = R
                    };
                    
                    // Agregar interacción tap solo si tiene schedule
                    var schedule = cell.Schedule;
                    
                    if (schedule != null)
                    {
                        var gesture = new TapGestureRecognizer();
                        gesture.Tapped += async (s, e) => {
                            await ShowSubjectDetailsAsync(subject, schedule, dh.DayName, hour);
                        };
                        cel.GestureRecognizers.Add(gesture);
                    }
                    
                    grid.Children.Add(cel);
                    Grid.SetRow(cel, gridRow);
                    Grid.SetColumn(cel, gridCol);
                }
                else
                {
                    // Celda vacía
                    var cel = new Label {
                        Text = "",
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#333333"),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        BackgroundColor = Color.FromArgb("#FFFFFF"),
                        WidthRequest = D,
                        HeightRequest = R
                    };
                    grid.Children.Add(cel);
                    Grid.SetRow(cel, gridRow);
                    Grid.SetColumn(cel, gridCol);
                }
            }
        }
    }

    /// <summary>
    /// Muestra los detalles de la materia cuando el usuario toca una celda.
    /// </summary>
    private async Task ShowSubjectDetailsAsync(Subject subject, Schedule schedule, string dayName, int hour)
    {
        var startTime = schedule.StartTime.ToString(@"hh\:mm");
        var endTime = schedule.EndTime.ToString(@"hh\:mm");
        
        // Mostrar info de la materia
        var message = $"Día: {dayName}\nHora: {startTime} - {endTime}";
        
        // Primero mostrar detalles
        await DisplayAlert(subject.Name, message, "Opciones...");
        
        // Luego mostrar acciones
        var action = await DisplayActionSheet(
            "¿Qué deseas hacer?",
            "Cancelar",
            null,
            new[] { "Editar materia", "Agregar tarea" });
        
        switch (action)
        {
            case "Editar materia":
                // Navegar directamente a la página de horarios de esa materia
                await Navigation.PushAsync(new SubjectSchedulesPage(subject, App.subjectService));
                break;
            case "Agregar tarea":
                await Navigation.PushAsync(new TasksPage());
                break;
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