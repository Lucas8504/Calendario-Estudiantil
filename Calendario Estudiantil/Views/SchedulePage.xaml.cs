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

        int totalW = H + 1 + (daysCount * D); // 1 = divisor vertical
        int totalH = (totalHours + 1) * R; // header + horas
        grid.WidthRequest = totalW;
        grid.HeightRequest = totalH;
        grid.Margin = new Thickness(0);
        grid.Padding = new Thickness(0);

        // Columnas: 1 hora + divisor + N días
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(H, GridUnitType.Absolute) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) }); // divisor vertical
        for (int d = 0; d < daysCount; d++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(D, GridUnitType.Absolute) });

        // Filas: header + N horas (sin divisor - las líneas las dibujamos con BoxView)
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(R, GridUnitType.Absolute) });
        for (int h = 0; h < totalHours; h++)
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(R, GridUnitType.Absolute) });

        // ==================== FILA 0: Headers ====================
        
        // [0,0] - Esquina "Hora" con borde
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
        var cornerBorder = new Border {
            Content = corner,
            Stroke = Color.FromArgb("#CCCCCC"),
            StrokeThickness = 1,
            WidthRequest = H,
            HeightRequest = R
        };
        grid.Children.Add(cornerBorder);
        Grid.SetRow(cornerBorder, 0);
        Grid.SetColumn(cornerBorder, 0);

        // Divisor vertical después de la columna de hora
        var topDivider = new BoxView { BackgroundColor = Color.FromArgb("#CCCCCC"), WidthRequest = 1, HeightRequest = R };
        grid.Children.Add(topDivider);
        Grid.SetRow(topDivider, 0);
        Grid.SetColumn(topDivider, 1);

        // Headers de días en columnas 2, 3, 4...
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
            var hdrBorder = new Border {
                Content = hdr,
                Stroke = Color.FromArgb("#CCCCCC"),
                StrokeThickness = 1,
                WidthRequest = D,
                HeightRequest = R
            };
            grid.Children.Add(hdrBorder);
            Grid.SetRow(hdrBorder, 0);
            Grid.SetColumn(hdrBorder, d + 2); // +2 porque columna 1 es divisor
        }

        // ==================== FILAS 1..N: Horarios ====================
        for (int hi = 0; hi < totalHours; hi++)
        {
            var hour = minHour + hi;
            var gridRow = hi + 1;

            // Columna 0: la hora con borde
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
            
            // Borde alrededor de la celda de hora
            var hourBorder = new Border {
                Content = hourLbl,
                Stroke = Color.FromArgb("#CCCCCC"),
                StrokeThickness = 1,
                WidthRequest = H,
                HeightRequest = R
            };
            
            grid.Children.Add(hourBorder);
            Grid.SetRow(hourBorder, gridRow);
            Grid.SetColumn(hourBorder, 0);

            // Divisor vertical
            var vDivider = new BoxView { BackgroundColor = Color.FromArgb("#CCCCCC"), WidthRequest = 1, HeightRequest = R };
            grid.Children.Add(vDivider);
            Grid.SetRow(vDivider, gridRow);
            Grid.SetColumn(vDivider, 1);

            // Celdas de cada día
            for (int di = 0; di < daysCount; di++)
            {
                var dh = dayHeaders[di];
                var gridCol = di + 2; // +2 porque columna 1 es divisor
                
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
                    
                    // Borde para celda con clase
                    var celBorder = new Border {
                        Content = cel,
                        Stroke = Color.FromArgb("#CCCCCC"),
                        StrokeThickness = 1,
                        WidthRequest = D,
                        HeightRequest = R
                    };
                    
// Agregar interacción tap solo si tiene schedule
                    var schedule = cell.Schedule;
                    
                    if (schedule != null)
                    {
                        // Capturar cell en una variable local para el closure
                        var currentCell = cell;
                        var currentDh = dh;
                        var currentHour = hour;
                        
                        var gesture = new TapGestureRecognizer();
                         gesture.Tapped += (s, e) => {
                             try
                             {
                                 // Usar CommunityToolkit Popup en vez de Alert/ActionSheet
                                 var popup = new SubjectPopup(
                                     currentCell.Subject!, 
                                     currentCell.Schedule!, 
                                     _subjectService,
                                     _taskService);
                                 this.ShowPopup(popup);
                             }
                             catch (Exception ex)
                             {
                                 DisplayAlert("Error", $"No se pudo abrir el popup: {ex.Message}", "OK");
                             }
                         };
                        cel.GestureRecognizers.Add(gesture);
                    }
                    
                    grid.Children.Add(celBorder);
                    Grid.SetRow(celBorder, gridRow);
                    Grid.SetColumn(celBorder, gridCol);
                }
                else
                {
                    // Celda vacía con borde
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
                    
                    var celBorder = new Border {
                        Content = cel,
                        Stroke = Color.FromArgb("#CCCCCC"),
                        StrokeThickness = 1,
                        WidthRequest = D,
                        HeightRequest = R
                    };
                    
                    grid.Children.Add(celBorder);
                    Grid.SetRow(celBorder, gridRow);
                    Grid.SetColumn(celBorder, gridCol);
                }
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