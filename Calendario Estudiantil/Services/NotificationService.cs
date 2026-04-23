using Plugin.LocalNotification;
using Microsoft.Maui.ApplicationModel;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de notificaciones.
/// </summary>
public class NotificationService : INotificationService
{
    public Task ScheduleAsync(int taskId, string title, string body, DateTime notifyAt)
    {
        var request = new NotificationRequest
        {
            NotificationId = taskId,
            Title = title,
            Description = body,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyAt
            }
        };

        NotificationCenter.Current.Show(request);
        return Task.CompletedTask;
    }

    public Task CancelAsync(int taskId)
    {
        NotificationCenter.Current.Cancel(taskId);
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