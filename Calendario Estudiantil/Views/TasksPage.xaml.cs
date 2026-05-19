using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    // Filtros disponibles
    private static readonly string[] FilterOptions = { "Todos", "Hoy", "Pendientes", "Vencidas" };

    public TasksPage()
    {
        InitializeComponent();
        
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService not available");
        
        _taskService = IPlatformApplication.Current?.Services
            .GetRequiredService<ITaskService>()
            ?? throw new InvalidOperationException("ITaskService not available");
        
        _viewModel = new TasksViewModel(_taskService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            BuildFilterTabs();
            await _viewModel.LoadTasksAsync();
            BuildTaskCards();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
    }

    private void BuildFilterTabs()
    {
        FilterTabsContainer.Children.Clear();

        foreach (var filter in FilterOptions)
        {
            var isSelected = filter == _viewModel.FilterMode;
            var btn = new Button
            {
                Text = filter,
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

            var selectedFilter = filter;
            btn.Clicked += async (s, e) =>
            {
                _viewModel.FilterMode = selectedFilter;
                await _viewModel.LoadTasksAsync();
                BuildFilterTabs();
                BuildTaskCards();
            };

            FilterTabsContainer.Children.Add(btn);
        }
    }

    private async void BuildTaskCards()
    {
        TasksContainer.Children.Clear();

        if (!_viewModel.Tasks.Any())
        {
            TasksContainer.Children.Add(new Label
            {
                Text = "📭 No hay tareas para mostrar",
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
        foreach (var task in _viewModel.Tasks)
        {
            var cardView = CreateTaskCard(task);
            cardViews.Add(cardView);
        }

        // Agregar cards al contenedor
        foreach (var cardView in cardViews)
        {
            TasksContainer.Children.Add(cardView);
        }

        // Animar cards con efecto cascada
        await AnimateCardsAsync(cardViews);
    }

    private Frame CreateTaskCard(TaskDisplay taskDisplay)
    {
        var task = taskDisplay.TaskItem;
        
        // Determinar colores según el tema actual
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var cardBgColor = isDark ? Color.FromArgb("#1E1E1E") : Colors.White;
        var textColor = isDark ? Color.FromArgb("#E0E0E0") : Color.FromArgb("#333333");
        var secondaryTextColor = isDark ? Color.FromArgb("#A0A0A0") : Colors.Gray;
        var completedBgColor = isDark ? Color.FromArgb("#2C2C2C") : Color.FromArgb("#F9FAFB");
        var colorHex = task.SubjectId > 0 ? GetSubjectColor(task.SubjectId) : (isDark ? "#A0A0A0" : "#6B7280");
        
        // Borde de color lateral (usando color de la materia si existe)
        var colorBar = new BoxView
        {
            Color = Color.FromArgb(colorHex),
            WidthRequest = 6,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill
        };

        // Indicador de estado
        var statusIndicator = task.Status == ItemStatus.Completed
            ? new Label
            {
                Text = "✅",
                FontSize = 20,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            }
            : new Label
            {
                Text = taskDisplay.IsShortAnticipation ? "️" : "⏳",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            };

        // Contenido de la card
        var titleLabel = new Label
        {
            Text = task.Title,
            FontSize = 16,
            FontAttributes = task.Status == ItemStatus.Completed ? FontAttributes.None : FontAttributes.Bold,
            TextColor = task.Status == ItemStatus.Completed ? secondaryTextColor : textColor,
            TextDecorations = task.Status == ItemStatus.Completed ? TextDecorations.Strikethrough : TextDecorations.None
        };

        var dueDateLabel = new Label
        {
            Text = $" Vence: {task.DueDate:dd/MM/yyyy}",
            FontSize = 13,
            TextColor = secondaryTextColor
        };

        var subjectLabel = task.SubjectId > 0
            ? new Label
            {
                Text = $" Materia #{task.SubjectId}",
                FontSize = 12,
                TextColor = secondaryTextColor
            }
            : null;

        var contentStack = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(12, 8),
            Children = { titleLabel, dueDateLabel }
        };

        if (subjectLabel != null)
        {
            contentStack.Children.Add(subjectLabel);
        }

        // Layout horizontal: barra de color + contenido
        var cardLayout = new HorizontalStackLayout
        {
            Spacing = 0,
            Children = { colorBar, contentStack }
        };

        // Layout principal con indicador de estado
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
        mainLayout.Children.Add(statusIndicator);
        Grid.SetColumn(statusIndicator, 1);

        // Card con sombra y bordes redondeados
        var cardFrame = new Frame
        {
            Content = mainLayout,
            BackgroundColor = task.Status == ItemStatus.Completed ? completedBgColor : cardBgColor,
            CornerRadius = 12,
            Padding = 0,
            HasShadow = true,
            BorderColor = Colors.Transparent,
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Opacity = 0.15f,
                Offset = new Point(0, 4),
                Radius = 8
            }
        };

        // Hacer que los hijos no intercepten toques
        mainLayout.InputTransparent = false;
        cardLayout.InputTransparent = true;
        contentStack.InputTransparent = true;
        colorBar.InputTransparent = true;
        titleLabel.InputTransparent = true;
        dueDateLabel.InputTransparent = true;
        if (subjectLabel != null)
        {
            subjectLabel.InputTransparent = true;
        }
        statusIndicator.InputTransparent = true;

        // Tap para mostrar opciones
        var gesture = new TapGestureRecognizer();
        gesture.Tapped += async (s, e) =>
        {
            try
            {
                var action = await DisplayActionSheet(
                    task.Title,
                    "Cancelar",
                    null,
                    task.Status == ItemStatus.Completed ? new[] { "Eliminar" } : new[] { "Completar", "Eliminar" });

                if (action == "Completar")
                {
                    await _viewModel.CompleteTaskAsync(taskDisplay);
                    BuildTaskCards();
                }
                else if (action == "Eliminar")
                {
                    var confirm = await DisplayAlert("Confirmar", "¿Eliminar esta tarea?", "Sí", "No");
                    if (confirm)
                    {
                        await _viewModel.DeleteTaskAsync(taskDisplay);
                        BuildTaskCards();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        };
        mainLayout.GestureRecognizers.Add(gesture);

        return cardFrame;
    }

    private string GetSubjectColor(int subjectId)
    {
        // TODO: Obtener color real de la materia desde el servicio
        // Por ahora retornamos un color por defecto
        return "#6B7280";
    }

    /// <summary>
    /// Anima las cards con un efecto de deslizamiento desde abajo.
    /// </summary>
    private async Task AnimateCardsAsync(List<Frame> cards)
    {
        const int staggerDelay = 80;
        const int animationDuration = 300;
        const double slideDistance = 50;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            
            card.Opacity = 0;
            card.TranslationY = slideDistance;

            await Task.Delay(staggerDelay);

            _ = card.TranslateTo(0, 0, (uint)animationDuration, Easing.CubicOut);
            _ = card.FadeTo(1, (uint)animationDuration, Easing.CubicOut);
        }
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
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
        DisplayAlert("Notificaciones", "Funcionalidad en desarrollo", "OK");
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new SettingsPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la configuración: {ex.Message}", "OK");
        }
    }

    private async void OnCreateTask(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new NewTaskPage(_subjectService, _taskService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la página de nueva tarea: {ex.Message}", "OK");
        }
    }
}
