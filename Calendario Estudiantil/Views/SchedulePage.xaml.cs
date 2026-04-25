using Calendario_Estudiantil.ViewModels;
using Calendario_Estudiantil.Models;
using Microsoft.Maui;
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

    /// <summary>
    /// Construye el grid dinámicamente basándose en los datos cargados.
    /// </summary>
    private void BuildGridUI()
    {
        GridContainer.Children.Clear();
        HeadersRow.Children.Clear();

        var minHour = _viewModel.MinHour;
        var totalHours = _viewModel.TotalHours;
        var daysCount = _viewModel.DayHeaders.Count;
        var cells = _viewModel.Cells;

        // 1. Columna de horas (primera)
        var hoursCol = new VerticalStackLayout { WidthRequest = 50 };
        hoursCol.Children.Add(new Border 
        { 
            BackgroundColor = Color.FromArgb("#EEEEEE"), 
            HeightRequest = 50,
            Content = new Label { Text = "Hora", FontSize = 11, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        
        for (int i = 0; i < totalHours; i++)
        {
            var hour = minHour + i;
            hoursCol.Children.Add(new Border 
            { 
                BackgroundColor = Color.FromArgb("#F5F5F5"), 
                HeightRequest = 50,
                Content = new Label { Text = $"{hour:00}:00", FontSize = 11, TextColor = Color.FromArgb("#666666"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
            });
        }
        GridContainer.Children.Add(hoursCol);

        // 2. Headers y columnas de días
        foreach (var dayHeader in _viewModel.DayHeaders)
        {
            // Header del día
            var headerFrame = new Frame 
            { 
                WidthRequest = 70, 
                Margin = new Thickness(1, 0, 1, 0),
                Padding = new Thickness(5),
                CornerRadius = 8,
                BackgroundColor = dayHeader.IsToday ? Color.FromArgb("#E3F2FD") : Color.FromArgb("#EEEEEE")
            };
            headerFrame.Content = new StackLayout 
            { 
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = dayHeader.DayName, FontSize = 12, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = dayHeader.DayNumber.ToString(), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333333"), HorizontalOptions = LayoutOptions.Center }
                }
            };
            HeadersRow.Children.Add(headerFrame);

            // Columna del día
            var dayCol = new VerticalStackLayout { WidthRequest = 70 };
            
            // Filtrar celdas para este día
            var dayCells = cells.Where(c => c.DayIndex == dayHeader.DayIndex).ToList();
            dayCells = dayCells.OrderBy(c => c.Hour).ToList();

            foreach (var cell in dayCells)
            {
                var bgColor = cell.HasClass 
                    ? Color.FromArgb(cell.Subject!.ColorHex) 
                    : Color.FromArgb("#FAFAFA");
                    
                var content = cell.HasClass 
                    ? (View)new Label 
                    { 
                        Text = cell.Subject!.Name, 
                        FontSize = 10, 
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center, 
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                    : new Label { Text = "", FontSize = 10 };
                    
                dayCol.Children.Add(new Border 
                { 
                    BackgroundColor = bgColor,
                    HeightRequest = 50,
                    Margin = new Thickness(1, 0, 1, 0),
                    Content = content
                });
            }
            
            GridContainer.Children.Add(dayCol);
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