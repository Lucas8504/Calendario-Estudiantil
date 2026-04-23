namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación stub del servicio de notificaciones.
/// </summary>
public class NotificationService : INotificationService
{
    public Task ScheduleAsync(int taskId, string title, string body, DateTime notifyAt)
    {
        // Stub - implementar más tarde con Plugin.LocalNotification correctamente configurado
        return Task.CompletedTask;
    }

    public Task CancelAsync(int taskId)
    {
        // Stub
        return Task.CompletedTask;
    }

    public async Task<bool> ShowCompletionDialogAsync(string taskTitle)
    {
        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return false;

        var result = await mainPage.DisplayAlert(
            "Completar Tarea",
            $"¿Completaste '{taskTitle}'?",
            "Sí",
            "No");

        return result;
    }
}