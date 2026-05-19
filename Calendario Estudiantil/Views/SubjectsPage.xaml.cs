using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.ViewModels;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectViewModel _viewModel;
    private readonly ISubjectService _subjectService;
    private readonly ITaskService _taskService;

    public SubjectsPage()
    {
        InitializeComponent();
        
        _subjectService = IPlatformApplication.Current?.Services
            .GetRequiredService<ISubjectService>()
            ?? throw new InvalidOperationException("ISubjectService no disponible");
        
        _taskService = IPlatformApplication.Current?.Services
            .GetRequiredService<ITaskService>()
            ?? throw new InvalidOperationException("ITaskService no disponible");
        
        _viewModel = new SubjectViewModel(_subjectService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _viewModel.LoadSubjectsAsync();
            SubjectsList.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(_viewModel.Subjects)));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las materias: {ex.Message}", "OK");
        }
    }

    private async void OnSubjectTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Subject subject)
        {
            try
            {
                await Navigation.PushAsync(new SubjectSchedulesPage(subject, _subjectService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir la materia: {ex.Message}", "OK");
            }
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

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        try
        {
            var pendingTasks = await _taskService.GetPendingAsync();
            
            if (!pendingTasks.Any())
            {
                await DisplayAlert("Notificaciones", "🎉 No tenés tareas pendientes", "OK");
                return;
            }

            var subjects = await _subjectService.GetAllAsync();
            var subjectDict = subjects.ToDictionary(s => s.Id, s => s.Name);

            var taskNames = pendingTasks
                .Take(10)
                .Select(t => $"{t.Title} ({subjectDict.GetValueOrDefault(t.SubjectId, "Sin materia")})")
                .ToArray();

            var options = new[] { "Cancelar" }.Concat(taskNames).ToArray();
            
            var action = await DisplayActionSheet(
                $"📋 Tareas pendientes ({pendingTasks.Count})",
                "Cerrar",
                null,
                options);

            if (action != "Cancelar" && !string.IsNullOrEmpty(action))
            {
                var selectedTask = pendingTasks.FirstOrDefault(t => 
                    $"{t.Title} ({subjectDict.GetValueOrDefault(t.SubjectId, "Sin materia")})" == action);
                
                if (selectedTask != null)
                {
                    var subjectName = subjectDict.GetValueOrDefault(selectedTask.SubjectId, "Sin materia");
                    var statusEmoji = selectedTask.Status switch
                    {
                        ItemStatus.Pending => "⏳",
                        ItemStatus.InProgress => "🔄",
                        ItemStatus.Completed => "✅",
                        ItemStatus.Expired => "❌",
                        _ => "❓"
                    };
                    
                    var details = $"📚 Materia: {subjectName}\n" +
                                  $"📝 Título: {selectedTask.Title}\n" +
                                  $"📅 Vence: {selectedTask.DueDate:dd/MM/yyyy HH:mm}\n" +
                                  $"📊 Estado: {statusEmoji} {selectedTask.Status}";
                    
                    if (!string.IsNullOrEmpty(selectedTask.Description))
                    {
                        details += $"\n📄 Descripción: {selectedTask.Description}";
                    }

                    await DisplayAlert("Detalle de Tarea", details, "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
    }

    private async void OnAddSubject(object sender, EventArgs e)
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
}
