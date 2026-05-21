using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Helpers;
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

    public SchedulePage(ScheduleDayViewModel viewModel, ISubjectService subjectService, ITaskService taskService)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        _subjectService = subjectService;
        _taskService = taskService;
        
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
                FontSize = 14,
                FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
                Padding = new Thickness(16, 8),
                CornerRadius = 20,
                BackgroundColor = isSelected ? AppConstants.Accent : Colors.Transparent,
                TextColor = isSelected ? Colors.White : Color.FromArgb("#666666"),
                BorderColor = Colors.Transparent,
                BorderWidth = 0,
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

    private async void BuildCards()
    {
        CardsContainer.Children.Clear();

        if (_viewModel.HasNoClasses)
        {
            CardsContainer.Children.Add(new Label
            {
                Text = " No hay clases para este día",
                FontSize = 16,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });
            return;
        }

        // Crear todas las cards primero
        var cardViews = new List<Frame>();
        foreach (var card in _viewModel.Cards)
        {
            var cardView = CreateCard(card);
            cardViews.Add(cardView);
        }

        // Agregar cards al contenedor
        foreach (var cardView in cardViews)
        {
            CardsContainer.Children.Add(cardView);
        }

        // Animar cards con efecto cascada
        await CardAnimationHelper.AnimateCardsAsync(cardViews);
    }

    private Frame CreateCard(ScheduleCard card)
    {
        // Determinar colores según el tema actual
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var cardBgColor = isDark ? Color.FromArgb("#1E1E1E") : Colors.White;
        var textColor = isDark ? Color.FromArgb("#E0E0E0") : Color.FromArgb("#333333");
        var secondaryTextColor = isDark ? Color.FromArgb("#A0A0A0") : Colors.Gray;
        var nowBgColor = isDark ? Color.FromArgb("#FF5252") : Color.FromArgb("#E53E3E");
        var cardBgNowColor = isDark ? Color.FromArgb("#2C1A1A") : Color.FromArgb("#FFF5F5");
        var borderColor = isDark ? Color.FromArgb("#FF5252") : Color.FromArgb("#E53E3E");

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
                Text = " AHORA",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = nowBgColor,
                Padding = new Thickness(8, 2),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            };
        }

        // Contenido de la card
        var timeLabel = new Label
        {
            Text = $" {card.TimeRange}",
            FontSize = 13,
            TextColor = secondaryTextColor,
            FontAttributes = FontAttributes.None
        };

        var durationLabel = new Label
        {
            Text = $" {card.Duration}",
            FontSize = 12,
            TextColor = secondaryTextColor,
            FontAttributes = FontAttributes.None
        };

        var subjectLabel = new Label
        {
            Text = card.SubjectName,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = textColor
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

        // Card con sombra pronunciada y bordes redondeados
        var cardFrame = new Frame
        {
            Content = mainLayout,
            BackgroundColor = card.IsHappeningNow ? cardBgNowColor : cardBgColor,
            CornerRadius = 12,
            Padding = 0,
            HasShadow = true,
            BorderColor = card.IsHappeningNow ? borderColor : Colors.Transparent,
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Opacity = 0.15f,
                Offset = new Point(0, 4),
                Radius = 8
            }
        };

        // Tap para mostrar detalles - agregar al layout principal para evitar que hijos intercepten toques
        var gesture = new TapGestureRecognizer();
        gesture.Tapped += async (s, e) =>
        {
            try
            {
                var popup = new SubjectPopup(card.Subject, card.Schedule, _subjectService, _taskService);
                this.ShowPopup(popup);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        };
        mainLayout.GestureRecognizers.Add(gesture);
        
        // Hacer que los hijos no intercepten toques
        mainLayout.InputTransparent = false;
        cardLayout.InputTransparent = true;
        contentStack.InputTransparent = true;
        timeRow.InputTransparent = true;
        colorBar.InputTransparent = true;
        timeLabel.InputTransparent = true;
        durationLabel.InputTransparent = true;
        subjectLabel.InputTransparent = true;
        if (nowIndicator != null)
        {
            nowIndicator.InputTransparent = true;
        }

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
        SearchHelper.ToggleSearch(SearchBarFrame, SearchEntry);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchHelper.UpdateSearch(_viewModel, (vm, q) => vm.Search(q), e.NewTextValue);
    }

    private void OnClearSearchClicked(object sender, EventArgs e)
    {
        SearchHelper.ClearSearch(_viewModel, (vm, _) => vm.ClearSearch(), SearchEntry, SearchBarFrame);
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = IPlatformApplication.Current?.Services.GetService<SettingsPage>();
            if (settingsPage != null)
                await Navigation.PushAsync(settingsPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la configuración: {ex.Message}", "OK");
        }
    }

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        await NotificationHelper.ShowPendingTasksAsync(this, _taskService, _subjectService);
    }
}
