using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleGridViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    public SchedulePage()
    {
        InitializeComponent();
        
        // Resolver servicios desde el DI container
        var databaseService = IPlatformApplication.Current?.Services
            .GetRequiredService<IDatabaseService>()
            ?? throw new InvalidOperationException("IDatabaseService no disponible");
        
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService no disponible");
        
        _taskService = IPlatformApplication.Current?.Services
            .GetRequiredService<ITaskService>()
            ?? throw new InvalidOperationException("ITaskService no disponible");
        
        _viewModel = new ScheduleGridViewModel(databaseService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadGridAsync();
            BuildGridUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el calendario: {ex.Message}", "OK");
        }
    }

    private void BuildGridUI()
    {
        var grid = GridContainer;
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        var daysCount = _viewModel.DayHeaders.Count;
        var cells = _viewModel.Cells;
        var dayHeaders = _viewModel.DayHeaders;
        var timeMarks = _viewModel.TimeMarks;

        if (daysCount == 0 || timeMarks.Count < 2) return;

        const int H = AppConstants.ScheduleGrid.HourColumnWidth;
        const int D = AppConstants.ScheduleGrid.DayColumnWidth;
        const int R = AppConstants.ScheduleGrid.RowHeight;
        double pixelsPerMinute = (double)R / 60.0;

        int totalW = H + 1 + (daysCount * D);
        int totalH = R + (int)((timeMarks.Last().Time - timeMarks.First().Time).TotalMinutes * pixelsPerMinute);

        grid.WidthRequest = totalW;
        grid.HeightRequest = totalH;
        grid.Margin = new Thickness(0);
        grid.Padding = new Thickness(0);

        // Columnas: hora | divisor | día₁ | día₂ | ... | díaₙ
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(H, GridUnitType.Absolute) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) });
        for (int d = 0; d < daysCount; d++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(D, GridUnitType.Absolute) });

        // Filas: header + N intervalos
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(R, GridUnitType.Absolute) });
        for (int i = 0; i < timeMarks.Count - 1; i++)
        {
            var duration = (timeMarks[i + 1].Time - timeMarks[i].Time).TotalMinutes;
            var height = Math.Max(20, (int)(duration * pixelsPerMinute));
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height, GridUnitType.Absolute) });
        }

        // ════════════════ FONDO + LÍNEAS DE GRID ════════════════
        // Un solo BoxView blanco de fondo para toda el área de días
        var gridBg = new BoxView { Color = Colors.White };
        grid.Children.Add(gridBg);
        Grid.SetRow(gridBg, 1);
        Grid.SetColumn(gridBg, 2);
        Grid.SetRowSpan(gridBg, timeMarks.Count - 1);
        Grid.SetColumnSpan(gridBg, daysCount);

        // Líneas horizontales entre filas de datos (saltamos row 1 porque headers ya tienen borde inferior)
        for (int i = 2; i < timeMarks.Count; i++)
        {
            var hLine = new BoxView
            {
                BackgroundColor = Color.FromArgb("#CCCCCC"),
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.Start
            };
            grid.Children.Add(hLine);
            Grid.SetRow(hLine, i);
            Grid.SetColumn(hLine, 2);
            Grid.SetColumnSpan(hLine, daysCount);
        }

        // Línea horizontal del borde inferior de la última fila de datos
        var bottomHLine = new BoxView
        {
            BackgroundColor = Color.FromArgb("#CCCCCC"),
            HeightRequest = 1,
            VerticalOptions = LayoutOptions.End
        };
        grid.Children.Add(bottomHLine);
        Grid.SetRow(bottomHLine, timeMarks.Count - 1);
        Grid.SetColumn(bottomHLine, 2);
        Grid.SetColumnSpan(bottomHLine, daysCount);

        // Líneas verticales entre columnas de días (col 3 en adelante = entre day 0/day1, day1/day2, etc.)
        for (int d = 1; d < daysCount; d++)
        {
            var vLine = new BoxView
            {
                BackgroundColor = Color.FromArgb("#CCCCCC"),
                WidthRequest = 1,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Start
            };
            grid.Children.Add(vLine);
            Grid.SetRow(vLine, 0);
            Grid.SetColumn(vLine, d + 2);
            Grid.SetRowSpan(vLine, timeMarks.Count);
        }

        // Borde vertical derecho de la última columna de día
        var rightVLine = new BoxView
        {
            BackgroundColor = Color.FromArgb("#CCCCCC"),
            WidthRequest = 1,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.End
        };
        grid.Children.Add(rightVLine);
        Grid.SetRow(rightVLine, 0);
        Grid.SetColumn(rightVLine, daysCount + 1);
        Grid.SetRowSpan(rightVLine, timeMarks.Count);

        // Divisor vertical único (col 1) entre hora y días — reemplaza los per-row + topDiv
        var divider = new BoxView { BackgroundColor = Color.FromArgb("#CCCCCC") };
        grid.Children.Add(divider);
        Grid.SetRow(divider, 0);
        Grid.SetColumn(divider, 1);
        Grid.SetRowSpan(divider, timeMarks.Count);

        // ════════════════ FILA 0: HEADERS ════════════════
        var cornerBorder = new Border
        {
            Content = new Label
            {
                Text = "Hora", FontSize = 12, FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Color.FromArgb("#DDDDDD"), WidthRequest = H, HeightRequest = R
            },
            Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1, WidthRequest = H, HeightRequest = R
        };
        grid.Children.Add(cornerBorder);
        Grid.SetRow(cornerBorder, 0); Grid.SetColumn(cornerBorder, 0);

        for (int d = 0; d < daysCount; d++)
        {
            var dh = dayHeaders[d];
            var bg = dh.IsToday ? Color.FromArgb("#E3F2FD") : Color.FromArgb("#DDDDDD");
            var hdrBorder = new Border
            {
                Content = new Label
                {
                    Text = dh.DayName + "\n" + dh.DayNumber, FontSize = 14, FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
                    BackgroundColor = bg, TextColor = Color.FromArgb("#333333"), WidthRequest = D, HeightRequest = R
                },
                Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1, WidthRequest = D, HeightRequest = R
            };
            grid.Children.Add(hdrBorder);
            Grid.SetRow(hdrBorder, 0); Grid.SetColumn(hdrBorder, d + 2);
        }

        // ════════════════ FILAS 1..N: CELDAS DE MATERIAS ════════════════
        // Celdas vacías ya no se crean — el fondo blanco + líneas de grid cubren lo visual
        var drawnSchedules = new HashSet<int>();

        for (int i = 0; i < timeMarks.Count - 1; i++)
        {
            var gridRow = i + 1;
            var currentMark = timeMarks[i];
            var nextMark = timeMarks[i + 1];
            var duration = (nextMark.Time - currentMark.Time).TotalMinutes;
            var rowHeight = Math.Max(20, (int)(duration * pixelsPerMinute));

            // Columna 0: etiqueta de hora
            var hourLbl = new Label
            {
                Text = currentMark.Label,
                FontSize = currentMark.IsWholeHour ? 12 : 10,
                FontAttributes = currentMark.IsWholeHour ? FontAttributes.Bold : FontAttributes.None,
                TextColor = currentMark.IsWholeHour ? Color.FromArgb("#333333") : Color.FromArgb("#666666"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Color.FromArgb("#F0F0F0"),
                WidthRequest = H,
                HeightRequest = rowHeight
            };

            var hourBorder = new Border
            {
                Content = hourLbl,
                Stroke = Color.FromArgb("#CCCCCC"),
                StrokeThickness = 1,
                WidthRequest = H,
                HeightRequest = rowHeight
            };
            grid.Children.Add(hourBorder);
            Grid.SetRow(hourBorder, gridRow); Grid.SetColumn(hourBorder, 0);

            // Celdas de cada día — solo dibujar si hay materia (el fondo + líneas ya están)
            for (int di = 0; di < daysCount; di++)
            {
                var dh = dayHeaders[di];
                var gridCol = di + 2;

                var cell = cells.FirstOrDefault(c => c.DayIndex == dh.DayIndex && c.StartTime == currentMark.Time);

                if (cell is { HasClass: true, Subject: not null, Schedule: not null })
                {
                    if (drawnSchedules.Contains(cell.Schedule.Id)) continue;
                    drawnSchedules.Add(cell.Schedule.Id);

                    var subject = cell.Subject;
                    var schedule = cell.Schedule;
                    int rowSpan = cell.RowSpan;

                    var cel = new Label
                    {
                        Text = subject.Name, FontSize = 11, FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center, BackgroundColor = Color.FromArgb(subject.ColorHex),
                        WidthRequest = D,
                        VerticalOptions = LayoutOptions.Fill
                    };

                    var celBorder = new Border
                    {
                        Content = cel, Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1,
                        WidthRequest = D,
                        VerticalOptions = LayoutOptions.Fill
                    };

                    var currentSubject = subject;
                    var currentSchedule = schedule;
                    var gesture = new TapGestureRecognizer();
                    gesture.Tapped += (s, e) =>
                    {
                        try
                        {
                            var popup = new SubjectPopup(currentSubject, currentSchedule, _subjectService, _taskService);
                            this.ShowPopup(popup);
                        }
                        catch (Exception ex) { DisplayAlert("Error", ex.Message, "OK"); }
                    };
                    cel.GestureRecognizers.Add(gesture);

                    grid.Children.Add(celBorder);
                    Grid.SetRow(celBorder, gridRow); Grid.SetRowSpan(celBorder, rowSpan);
                    Grid.SetColumn(celBorder, gridCol);
                }
                // Sin materia: no dibujamos nada. Fondo blanco + líneas de grid cubren lo visual.
            }
        }
    }

/// <summary>
    /// Muestra los detalles de la materia cuando el usuario toca una celda.
    /// </summary>
    private async Task ShowSubjectDetailsAsync(Subject subject, Schedule schedule, string dayName, int hour)
    {
        try
        {
            var startTime = schedule.StartTime.ToString(@"hh\:mm");
            var endTime = schedule.EndTime.ToString(@"hh\:mm");
            
            // Mostrar directamente el ActionSheet con la información en el título
            var action = await DisplayActionSheet(
                $"{subject.Name} - {dayName} {startTime}-{endTime}",
                "Cancelar",
                null,
                new[] { "Editar materia", "Agregar tarea" });
            
            switch (action)
            {
                case "Editar materia":
                    await Navigation.PushAsync(new SubjectSchedulesPage(subject, _subjectService));
                    break;
                case "Agregar tarea":
                    await Navigation.PushAsync(new NewTaskPage(_subjectService, _taskService));
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo mostrar los detalles: {ex.Message}", "OK");
        }
    }

    private async void OnPreviousWeek(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.PreviousWeekAsync();
            BuildGridUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cambiar a la semana anterior: {ex.Message}", "OK");
        }
    }

    private async void OnNextWeek(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.NextWeekAsync();
            BuildGridUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cambiar a la semana siguiente: {ex.Message}", "OK");
        }
    }

    private async void OnGoToToday(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.GoToTodayAsync();
            BuildGridUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo ir a hoy: {ex.Message}", "OK");
        }
    }

}