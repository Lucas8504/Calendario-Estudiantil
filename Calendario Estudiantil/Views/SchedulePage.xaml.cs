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
    private readonly ScheduleDayViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    public SchedulePage()
    {
        InitializeComponent();
        
        var databaseService = IPlatformApplication.Current?.Services
            .GetRequiredService<IDatabaseService>()
            ?? throw new InvalidOperationException("IDatabaseService no disponible");
        
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService no disponible");
        
        _taskService = IPlatformApplication.Current?.Services
            .GetRequiredService<ITaskService>()
            ?? throw new InvalidOperationException("ITaskService no disponible");
        
        _viewModel = new ScheduleDayViewModel(databaseService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            BuildDayTabs();
            await _viewModel.LoadDayAsync();
            BuildCards();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el calendario: {ex.Message}", "OK");
        }
    }

    private void BuildDayTabs()
    {
        DayTabsContainer.Children.Clear();

        foreach (var day in _viewModel.DayTabs)
        {
            var isSelected = day == _viewModel.SelectedDay;
            var btn = new Button
            {
                Text = day,
                FontSize = 13,
                FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
                Padding = new Thickness(12, 6),
                CornerRadius = 8,
                BackgroundColor = isSelected ? AppConstants.Accent : Colors.White,
                TextColor = isSelected ? Colors.White : AppConstants.Accent,
                BorderColor = AppConstants.Accent,
                BorderWidth = isSelected ? 0 : 1,
                HorizontalOptions = LayoutOptions.Start
            };

            // Capturar el día para el evento
            var selectedDay = day;
            btn.Clicked += async (s, e) =>
            {
                await _viewModel.SelectDayAsync(selectedDay);
                BuildDayTabs(); // Rebuild to update selected state
                BuildCards();
            };

            DayTabsContainer.Children.Add(btn);
        }
    }

    private void BuildCards()
    {
        CardsContainer.Children.Clear();

        if (_viewModel.HasNoClasses)
        {
            CardsContainer.Children.Add(new Label
            {
                Text = "📭 No hay clases para este día",
                FontSize = 16,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });
            return;
        }

        foreach (var card in _viewModel.Cards)
        {
            var cardView = CreateCard(card);
            CardsContainer.Children.Add(cardView);
        }
    }

    private Frame CreateCard(ScheduleCard card)
    {
        // Borde de color lateral
        var colorBar = new BoxView
        {
            Color = Color.FromArgb(card.ColorHex),
            WidthRequest = 6,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill
        };

        // Indicador "ahora" si la clase está en curso
        View? nowIndicator = null;
        if (card.IsHappeningNow)
        {
            nowIndicator = new Label
            {
                Text = "🔴 AHORA",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#E53E3E"),
                Padding = new Thickness(8, 2),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            };
        }

        // Contenido de la card
        var timeLabel = new Label
        {
            Text = $"🕐 {card.TimeRange}",
            FontSize = 13,
            TextColor = Colors.Gray,
            FontAttributes = FontAttributes.None
        };

        var durationLabel = new Label
        {
            Text = $"⏱ {card.Duration}",
            FontSize = 12,
            TextColor = Color.FromArgb("#666666"),
            FontAttributes = FontAttributes.None
        };

        var subjectLabel = new Label
        {
            Text = card.SubjectName,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#333333")
        };

        // Layout superior: tiempo + duración
        var timeRow = new HorizontalStackLayout
        {
            Spacing = 12,
            Children = { timeLabel, durationLabel }
        };

        var contentStack = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(12, 8),
            Children = { timeRow, subjectLabel }
        };

        // Layout horizontal: barra de color + contenido
        var cardLayout = new HorizontalStackLayout
        {
            Spacing = 0,
            Children = { colorBar, contentStack }
        };

        // Layout principal con indicador "ahora"
        var mainLayout = new Grid
        {
            ColumnDefinitions = 
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        mainLayout.Children.Add(cardLayout);
        Grid.SetColumn(cardLayout, 0);
        
        if (nowIndicator != null)
        {
            mainLayout.Children.Add(nowIndicator);
            Grid.SetColumn(nowIndicator, 1);
        }

        // Card con sombra y bordes redondeados (usando Frame para compatibilidad con .NET 8)
        var cardFrame = new Frame
        {
            Content = mainLayout,
            BackgroundColor = card.IsHappeningNow ? Color.FromArgb("#FFF5F5") : Colors.White,
            CornerRadius = 12,
            Padding = 0,
            HasShadow = true,
            BorderColor = card.IsHappeningNow ? Color.FromArgb("#E53E3E") : Colors.Transparent
        };

        // Tap para mostrar detalles
        var gesture = new TapGestureRecognizer();
        gesture.Tapped += (s, e) =>
        {
            try
            {
                var popup = new SubjectPopup(card.Subject, card.Schedule, _subjectService, _taskService);
                this.ShowPopup(popup);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "OK");
            }
        };
        cardFrame.GestureRecognizers.Add(gesture);

        return cardFrame;
    }

    private async void OnAddScheduleClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new NewSchedulePage(_subjectService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el formulario: {ex.Message}", "OK");
        }
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
        // Mostrar/ocultar barra de búsqueda
        SearchBarFrame.IsVisible = !SearchBarFrame.IsVisible;
        if (SearchBarFrame.IsVisible)
        {
            SearchEntry.Focus();
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.Search(e.NewTextValue);
    }

    private void OnClearSearchClicked(object sender, EventArgs e)
    {
        _viewModel.ClearSearch();
        SearchEntry.Text = string.Empty;
        SearchBarFrame.IsVisible = false;
    }

    private void OnNotificationsClicked(object sender, EventArgs e)
    {
        _ = ShowPendingTasksAsync();
    }

    private async Task ShowPendingTasksAsync()
    {
        try
        {
            var pendingTasks = await _taskService.GetPendingAsync();
            
            if (!pendingTasks.Any())
            {
                await DisplayAlert("Notificaciones", "🎉 No tenés tareas pendientes", "OK");
                return;
            }

            // Mostrar las tareas próximas en un ActionSheet
            var taskNames = pendingTasks
                .Take(10) // Limitar a 10 para no saturar
                .Select(t => $"{t.Title} - {t.DueDate:dd/MM}")
                .ToArray();

            var options = new[] { "Cancelar" }.Concat(taskNames).ToArray();
            
            var action = await DisplayActionSheet(
                $"📋 Tareas pendientes ({pendingTasks.Count})",
                "Cerrar",
                null,
                options);

            // Si seleccionó una tarea, navegar a ella
            if (action != "Cancelar" && !string.IsNullOrEmpty(action))
            {
                var selectedTask = pendingTasks.FirstOrDefault(t => 
                    $"{t.Title} - {t.DueDate:dd/MM}" == action);
                
                if (selectedTask != null)
                {
                    // TODO: Navegar al detalle de la tarea
                    await DisplayAlert("Tarea", $"Seleccionaste: {selectedTask.Title}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
    }
}
