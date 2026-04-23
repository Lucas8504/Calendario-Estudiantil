using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Servicio de notificaciones locales.
/// </summary>
public interface INotificationService
{
    Task ScheduleAsync(int taskId, string title, string body, DateTime notifyAt);
    Task CancelAsync(int taskId);
    Task<bool> ShowCompletionDialogAsync(string taskTitle);
}