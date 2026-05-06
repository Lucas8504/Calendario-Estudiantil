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

        // Usar constantes de configuración
        const int H = AppConstants.ScheduleGrid.HourColumnWidth;
        const int D = AppConstants.ScheduleGrid.DayColumnWidth;
        const int R = AppConstants.ScheduleGrid.RowHeight; // Altura base para 60 min
        double pixelsPerMinute = (double)R / 60.0;

        // Calcular altura total
        int totalW = H + 1 + (daysCount * D);
        int totalH = R + (int)((timeMarks.Last().Time - timeMarks.First().Time).TotalMinutes * pixelsPerMinute);
        
        grid.WidthRequest = totalW;
        grid.HeightRequest = totalH;
        grid.Margin = new Thickness(0);
        grid.Padding = new Thickness(0);

        // Columnas: 1 hora + divisor + N días
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(H, GridUnitType.Absolute) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) });
        for (int d = 0; d < daysCount; d++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(D, GridUnitType.Absolute) });

        // Filas: header + intervalos entre TimeMarks
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(R, GridUnitType.Absolute) });
        for (int i = 0; i < timeMarks.Count - 1; i++)
        {
            var duration = (timeMarks[i + 1].Time - timeMarks[i].Time).TotalMinutes;
            var height = Math.Max(20, (int)(duration * pixelsPerMinute));
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height, GridUnitType.Absolute) });
        }

        // ==================== FILA 0: Headers ====================
        // [0,0] - Esquina "Hora" con borde
        var cornerBorder = new Border {
            Content = new Label { Text = "Hora", FontSize = 12, FontAttributes = FontAttributes.Bold, 
                                  HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, 
                                  BackgroundColor = Color.FromArgb("#DDDDDD"), WidthRequest = H, HeightRequest = R },
            Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1, WidthRequest = H, HeightRequest = R
        };
        grid.Children.Add(cornerBorder);
        Grid.SetRow(cornerBorder, 0); Grid.SetColumn(cornerBorder, 0);

        var topDiv = new BoxView { BackgroundColor = Color.FromArgb("#CCCCCC"), WidthRequest = 1, HeightRequest = R };
        grid.Children.Add(topDiv);
        Grid.SetRow(topDiv, 0); Grid.SetColumn(topDiv, 1);

        for (int d = 0; d < daysCount; d++)
        {
            var dh = dayHeaders[d];
            var bg = dh.IsToday ? Color.FromArgb("#E3F2FD") : Color.FromArgb("#DDDDDD");
            var hdrBorder = new Border {
                Content = new Label { Text = dh.DayName + "\n" + dh.DayNumber, FontSize = 14, FontAttributes = FontAttributes.Bold, 
                                      HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, 
                                      BackgroundColor = bg, TextColor = Color.FromArgb("#333333"), WidthRequest = D, HeightRequest = R },
                Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1, WidthRequest = D, HeightRequest = R
            };
            grid.Children.Add(hdrBorder);
            Grid.SetRow(hdrBorder, 0); Grid.SetColumn(hdrBorder, d + 2);
        }

        // ==================== FILAS 1..N: Horarios (Regla proporcional) ====================
        // Usar un Set para rastrear clases ya dibujadas para evitar duplicados por RowSpan
        var drawnSchedules = new HashSet<int>();

        for (int i = 0; i < timeMarks.Count - 1; i++)
        {
            var gridRow = i + 1;
            var currentMark = timeMarks[i];
            var nextMark = timeMarks[i + 1];
            var duration = (nextMark.Time - currentMark.Time).TotalMinutes;
            var rowHeight = Math.Max(20, (int)(duration * pixelsPerMinute));

            // Columna 0: La hora (Siempre dibujar etiqueta para cada marca)
            
            var hourLbl = new Label {
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
            
            var hourBorder = new Border {
                Content = hourLbl, 
                Stroke = Color.FromArgb("#CCCCCC"), 
                StrokeThickness = 1, 
                WidthRequest = H, 
                HeightRequest = rowHeight
            };
            grid.Children.Add(hourBorder);
            Grid.SetRow(hourBorder, gridRow); Grid.SetColumn(hourBorder, 0);

            var div = new BoxView { BackgroundColor = Color.FromArgb("#CCCCCC"), WidthRequest = 1, HeightRequest = rowHeight };
            grid.Children.Add(div);
            Grid.SetRow(div, gridRow); Grid.SetColumn(div, 1);

            // Celdas de cada día
            for (int di = 0; di < daysCount; di++)
            {
                var dh = dayHeaders[di];
                var gridCol = di + 2;
                
                // Buscar celda que empiece exactamente en esta marca de tiempo
                var cell = cells.FirstOrDefault(c => c.DayIndex == dh.DayIndex && c.StartTime == currentMark.Time);
                
                // Verificar si hay una materia que abarca este intervalo (RowSpan)
                var activeSchedule = cells.FirstOrDefault(c => 
                    c.DayIndex == dh.DayIndex && 
                    c.Schedule != null &&
                    c.Schedule.StartTime <= currentMark.Time && 
                    c.Schedule.EndTime >= nextMark.Time)?.Schedule;

                if (cell != null && cell.HasClass && cell.Subject != null)
                {
                    // Verificar si esta clase ya fue dibujada (para manejar RowSpan)
                    if (cell.Schedule != null && drawnSchedules.Contains(cell.Schedule.Id)) continue;

                    var subject = cell.Subject;
                    var schedule = cell.Schedule;
                    if (schedule != null) drawnSchedules.Add(schedule.Id);

                    // Calcular RowSpan basado en las marcas de tiempo
                    int rowSpan = 0;
                    
                    for (int j = i; j < timeMarks.Count - 1; j++)
                    {
                        var markStart = timeMarks[j].Time;
                        var markEnd = timeMarks[j + 1].Time;
                        
                        bool startsAfter = markStart >= schedule.StartTime;
                        bool endsBefore = markEnd <= schedule.EndTime;

                        if (startsAfter && endsBefore)
                        {
                            rowSpan++;
                        }
                        else if (markStart > schedule.StartTime) 
                        {
                            break; 
                        }
                    }
                    
                    if (rowSpan == 0) rowSpan = 1;

                    // NO usamos HeightRequest fijo - dejamos que el Grid maneje la altura con RowSpan
                    var cel = new Label {
                        Text = subject.Name, FontSize = 11, FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center, BackgroundColor = Color.FromArgb(subject.ColorHex),
                        WidthRequest = D,
                        VerticalOptions = LayoutOptions.Fill
                    };
                    
                    var celBorder = new Border {
                        Content = cel, Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1, 
                        WidthRequest = D,
                        VerticalOptions = LayoutOptions.Fill
                    };

                    // Agregar interacción tap
                    var currentSchedule = schedule;
                    var currentSubject = subject;
                    var gesture = new TapGestureRecognizer();
                    gesture.Tapped += (s, e) => {
                        try {
                            var popup = new SubjectPopup(currentSubject, currentSchedule, _subjectService, _taskService);
                            this.ShowPopup(popup);
                        } catch (Exception ex) { DisplayAlert("Error", ex.Message, "OK"); }
                    };
                    cel.GestureRecognizers.Add(gesture);
                    
                    grid.Children.Add(celBorder);
                    Grid.SetRow(celBorder, gridRow); Grid.SetRowSpan(celBorder, rowSpan);
                    Grid.SetColumn(celBorder, gridCol);
                }
                else if (activeSchedule == null)
                {
                    // Solo dibujar celda vacía si NO hay materia activa (RowSpan) cubriendo este espacio
                    var celBorder = new Border {
                        Content = new Label { Text = "", BackgroundColor = Color.FromArgb("#FFFFFF"), WidthRequest = D, HeightRequest = rowHeight },
                        Stroke = Color.FromArgb("#CCCCCC"), StrokeThickness = 1, WidthRequest = D, HeightRequest = rowHeight
                    };
                    grid.Children.Add(celBorder);
                    Grid.SetRow(celBorder, gridRow); Grid.SetColumn(celBorder, gridCol);
                }
                // Si activeSchedule != null, significa que un RowSpan ya cubre este espacio → NO dibujamos nada
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