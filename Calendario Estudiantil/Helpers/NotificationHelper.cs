using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Helpers;

/// <summary>
/// Helper compartido para mostrar notificaciones de tareas pendientes.
/// Elimina ~150 líneas de código duplicado entre SchedulePage, TasksPage y SubjectsPage.
/// </summary>
public static class NotificationHelper
{
    /// <summary>
    /// Muestra las tareas pendientes en un ActionSheet y permite ver detalles.
    /// </summary>
    public static async Task ShowPendingTasksAsync(
        Page page,
        ITaskService taskService,
        ISubjectService subjectService)
    {
        try
        {
            var pendingTasks = await taskService.GetPendingAsync();

            if (!pendingTasks.Any())
            {
                await page.DisplayAlert("Notificaciones", "🎉 No tenés tareas pendientes", "OK");
                return;
            }

            var subjects = await subjectService.GetAllAsync();
            var subjectDict = subjects.ToDictionary(s => s.Id, s => s.Name);

            var taskNames = pendingTasks
                .Take(10)
                .Select(t => $"{t.Title} ({subjectDict.GetValueOrDefault(t.SubjectId, "Sin materia")})")
                .ToArray();

            var options = new[] { "Cancelar" }.Concat(taskNames).ToArray();

            var action = await page.DisplayActionSheet(
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
                    await ShowTaskDetails(page, selectedTask, subjectDict);
                }
            }
        }
        catch (Exception ex)
        {
            await page.DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
    }

    private static async Task ShowTaskDetails(
        Page page,
        TaskItem task,
        Dictionary<int, string> subjectDict)
    {
        var subjectName = subjectDict.GetValueOrDefault(task.SubjectId, "Sin materia");
        var statusEmoji = task.Status switch
        {
            ItemStatus.Pending => "⏳",
            ItemStatus.InProgress => "🔄",
            ItemStatus.Completed => "✅",
            ItemStatus.Expired => "❌",
            _ => "❓"
        };

        var details = $"📚 Materia: {subjectName}\n" +
                      $"📝 Título: {task.Title}\n" +
                      $"📅 Vence: {task.DueDate:dd/MM/yyyy HH:mm}\n" +
                      $"📊 Estado: {statusEmoji} {task.Status}";

        if (!string.IsNullOrEmpty(task.Description))
        {
            details += $"\n📄 Descripción: {task.Description}";
        }

        await page.DisplayAlert("Detalle de Tarea", details, "OK");
    }
}
