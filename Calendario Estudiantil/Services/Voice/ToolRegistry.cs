namespace Calendario_Estudiantil.Services.Voice;

using System.Globalization;
using System.Text;
using System.Text.Json;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

/// <summary>
/// Implementación del registro de herramientas del asistente de voz.
/// Expone el schema que Gemini usa para function calling y ejecuta cada operación
/// contra los servicios de la app. Las mutaciones destructivas o de edición
/// (update/delete) devuelven una confirmación pendiente en vez de ejecutarse.
/// </summary>
public sealed class ToolRegistry : IToolRegistry
{
    private const string DefaultColorHex = "#512BD4";

    private readonly ITaskService _tasks;
    private readonly ISubjectService _subjects;
    private readonly IDatabaseService _database;
    private readonly Dictionary<string, ToolDefinition> _tools = new(StringComparer.OrdinalIgnoreCase);

    public ToolRegistry(ITaskService tasks, ISubjectService subjects, IDatabaseService database)
    {
        _tasks = tasks;
        _subjects = subjects;
        _database = database;

        RegisterFindSubject();
        RegisterTaskTools();
        RegisterSubjectTools();
        RegisterScheduleTools();
    }

    private delegate Task<ToolCallResult> ToolHandler(JsonElement args, bool confirmed, CancellationToken ct);

    private sealed record ToolDefinition(FunctionDeclaration Declaration, ToolHandler Handler);

    public List<FunctionDeclaration> GetFunctionDeclarations()
        => _tools.Values.Select(t => t.Declaration).ToList();

    public Task<ToolCallResult> ExecuteAsync(string toolName, string argsJson, CancellationToken ct = default)
        => DispatchAsync(toolName, argsJson, confirmed: false, ct);

    public Task<ToolCallResult> ExecuteConfirmedAsync(string toolName, string argsJson, CancellationToken ct = default)
        => DispatchAsync(toolName, argsJson, confirmed: true, ct);

    private async Task<ToolCallResult> DispatchAsync(string toolName, string argsJson, bool confirmed, CancellationToken ct)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
            return ToolCallResult.Failure(toolName, $"Función desconocida: {toolName}.");

        JsonElement args;
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argsJson) ? "{}" : argsJson);
            args = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return ToolCallResult.Failure(toolName, "Los argumentos de la función no son JSON válido.");
        }

        try
        {
            return await tool.Handler(args, confirmed, ct);
        }
        catch (Exception ex)
        {
            return ToolCallResult.Failure(toolName, $"Error ejecutando {toolName}: {ex.Message}");
        }
    }

    // ==================== Registro de herramientas ====================

    private void Register(string name, string description, Dictionary<string, object> properties, string[] required, ToolHandler handler)
        => _tools[name] = new ToolDefinition(
            FunctionDeclarationBuilder.Create(name, description, properties, required),
            handler);

    private void RegisterFindSubject()
    {
        Register("find_subject",
            "Busca materias existentes por nombre aproximado. Usala para resolver el nombre que dijo el usuario a un subject_id antes de crear, editar o borrar tareas y horarios.",
            new()
            {
                ["name"] = FunctionDeclarationBuilder.Prop("string", "Nombre de la materia, tal como lo dijo el usuario")
            },
            new[] { "name" },
            async (args, _, ct) =>
            {
                string? name = GetString(args, "name");
                if (string.IsNullOrWhiteSpace(name))
                    return ToolCallResult.Failure("find_subject", "Falta el nombre de la materia.");

                var all = await _subjects.GetAllAsync();
                var normalized = Normalize(name);
                if (normalized.Length == 0)
                    return ToolCallResult.Failure("find_subject", $"No encontré ninguna materia que se parezca a '{name}'.");

                var matches = all
                    .Where(s =>
                    {
                        var candidate = Normalize(s.Name);
                        return candidate.Contains(normalized, StringComparison.Ordinal)
                            || normalized.Contains(candidate, StringComparison.Ordinal);
                    })
                    .ToList();

                if (matches.Count == 0)
                    return ToolCallResult.Failure("find_subject", $"No encontré ninguna materia que se parezca a '{name}'.");

                var list = string.Join(", ", matches.Select(m => $"{m.Name} (id {m.Id})"));
                return ToolCallResult.Ok("find_subject", $"Materias encontradas: {list}.", JsonSerializer.Serialize(matches));
            });
    }

    private void RegisterTaskTools()
    {
        Register("add_task",
            "Crea una tarea nueva asociada a una materia existente. La fecha de vencimiento debe ser ISO 8601 (ej. 2026-11-13T18:00:00).",
            new()
            {
                ["title"] = FunctionDeclarationBuilder.Prop("string", "Título de la tarea"),
                ["subject_name"] = FunctionDeclarationBuilder.Prop("string", "Nombre de la materia existente"),
                ["subject_id"] = FunctionDeclarationBuilder.Prop("integer", "Id de la materia, si ya lo conocés"),
                ["due_date"] = FunctionDeclarationBuilder.Prop("string", "Fecha y hora de vencimiento en ISO 8601"),
                ["description"] = FunctionDeclarationBuilder.Prop("string", "Descripción opcional"),
                ["notification_hours"] = FunctionDeclarationBuilder.Prop("integer", "Horas de anticipación del recordatorio (por defecto 24)")
            },
            new[] { "title", "due_date" },
            async (args, _, ct) =>
            {
                string? title = GetString(args, "title");
                if (string.IsNullOrWhiteSpace(title))
                    return ToolCallResult.Failure("add_task", "La tarea necesita un título.");

                if (!TryParseDate(GetString(args, "due_date"), out var dueDate))
                    return ToolCallResult.Failure("add_task", "No pude interpretar la fecha de vencimiento. Pedile al usuario que repita el día y la hora.");

                var subject = await ResolveSubjectAsync(GetInt(args, "subject_id"), GetString(args, "subject_name"));
                if (subject is null)
                    return ToolCallResult.Failure("add_task", "Necesito una materia existente para asociar la tarea. Si la materia no existe, creala primero con add_subject.");

                var task = new TaskItem
                {
                    SubjectId = subject.Id,
                    Title = title.Trim(),
                    Description = GetString(args, "description") ?? string.Empty,
                    DueDate = dueDate,
                    Status = ItemStatus.Pending,
                    NotificationHours = GetInt(args, "notification_hours") ?? 24
                };

                int id = await _tasks.CreateAsync(task);
                return ToolCallResult.Ok("add_task",
                    $"Tarea '{task.Title}' creada en {subject.Name} para el {dueDate:dd/MM/yyyy HH:mm} (id {id}).");
            });

        Register("update_task",
            "Modifica una tarea existente. Requiere task_id y la confirmación explícita del usuario antes de aplicar los cambios.",
            new()
            {
                ["task_id"] = FunctionDeclarationBuilder.Prop("integer", "Id de la tarea a modificar"),
                ["title"] = FunctionDeclarationBuilder.Prop("string", "Nuevo título (opcional)"),
                ["description"] = FunctionDeclarationBuilder.Prop("string", "Nueva descripción (opcional)"),
                ["subject_name"] = FunctionDeclarationBuilder.Prop("string", "Nueva materia por nombre (opcional)"),
                ["subject_id"] = FunctionDeclarationBuilder.Prop("integer", "Nueva materia por id (opcional)"),
                ["due_date"] = FunctionDeclarationBuilder.Prop("string", "Nueva fecha de vencimiento en ISO 8601 (opcional)"),
                ["status"] = FunctionDeclarationBuilder.EnumProp("Nuevo estado", "pending", "in_progress", "completed", "expired"),
                ["notification_hours"] = FunctionDeclarationBuilder.Prop("integer", "Nuevas horas de anticipación (opcional)")
            },
            new[] { "task_id" },
            async (args, confirmed, ct) =>
            {
                int? taskId = GetInt(args, "task_id");
                if (taskId is null)
                    return ToolCallResult.Failure("update_task", "Falta el id de la tarea.");

                var task = await _tasks.GetByIdAsync(taskId.Value);
                if (task is null)
                    return ToolCallResult.Failure("update_task", $"No encontré la tarea con id {taskId}.");

                string? title = GetString(args, "title");
                string? description = GetString(args, "description");
                string? statusRaw = GetString(args, "status");
                int? notificationHours = GetInt(args, "notification_hours");

                DateTime? dueDate = null;
                if (GetString(args, "due_date") is string dueRaw)
                {
                    if (!TryParseDate(dueRaw, out var parsedDue))
                        return ToolCallResult.Failure("update_task", "No pude interpretar la nueva fecha de vencimiento.");
                    dueDate = parsedDue;
                }

                Subject? newSubject = null;
                if (GetInt(args, "subject_id") is not null || !string.IsNullOrWhiteSpace(GetString(args, "subject_name")))
                {
                    newSubject = await ResolveSubjectAsync(GetInt(args, "subject_id"), GetString(args, "subject_name"));
                    if (newSubject is null)
                        return ToolCallResult.Failure("update_task", "No encontré la materia indicada para la tarea.");
                }

                ItemStatus? newStatus = null;
                if (!string.IsNullOrWhiteSpace(statusRaw))
                {
                    if (!TryParseStatus(statusRaw, out var parsedStatus))
                        return ToolCallResult.Failure("update_task", $"Estado inválido: '{statusRaw}'.");
                    newStatus = parsedStatus;
                }

                var changes = new List<string>();
                if (title is not null) changes.Add($"título → '{title}'");
                if (description is not null) changes.Add("descripción");
                if (dueDate is not null) changes.Add($"vence → {dueDate:dd/MM/yyyy HH:mm}");
                if (newStatus is not null) changes.Add($"estado → {newStatus}");
                if (newSubject is not null) changes.Add($"materia → {newSubject.Name}");
                if (notificationHours is not null) changes.Add($"recordatorio → {notificationHours} h antes");

                if (changes.Count == 0)
                    return ToolCallResult.Failure("update_task", "No indicaste qué cambiar de la tarea.");

                if (!confirmed)
                    return ToolCallResult.NeedsConfirmation("update_task",
                        $"Voy a modificar la tarea '{task.Title}': {string.Join(", ", changes)}. ¿Confirmás?");

                if (title is not null) task.Title = title.Trim();
                if (description is not null) task.Description = description;
                if (dueDate is not null) task.DueDate = dueDate.Value;
                if (newStatus is not null) task.Status = newStatus.Value;
                if (newSubject is not null) task.SubjectId = newSubject.Id;
                if (notificationHours is not null) task.NotificationHours = notificationHours.Value;

                await _tasks.UpdateAsync(task);
                return ToolCallResult.Ok("update_task", $"Tarea '{task.Title}' actualizada.");
            });

        Register("delete_task",
            "Borra una tarea existente junto con sus subtareas. Requiere task_id y la confirmación explícita del usuario.",
            new()
            {
                ["task_id"] = FunctionDeclarationBuilder.Prop("integer", "Id de la tarea a borrar")
            },
            new[] { "task_id" },
            async (args, confirmed, ct) =>
            {
                int? taskId = GetInt(args, "task_id");
                if (taskId is null)
                    return ToolCallResult.Failure("delete_task", "Falta el id de la tarea.");

                var task = await _tasks.GetByIdAsync(taskId.Value);
                if (task is null)
                    return ToolCallResult.Failure("delete_task", $"No encontré la tarea con id {taskId}.");

                if (!confirmed)
                    return ToolCallResult.NeedsConfirmation("delete_task",
                        $"Voy a borrar la tarea '{task.Title}' y sus subtareas. ¿Confirmás?");

                await _tasks.DeleteAsync(task);
                return ToolCallResult.Ok("delete_task", $"Tarea '{task.Title}' borrada.");
            });
    }

    private void RegisterSubjectTools()
    {
        Register("add_subject",
            "Crea una materia nueva. No requiere confirmación.",
            new()
            {
                ["name"] = FunctionDeclarationBuilder.Prop("string", "Nombre de la materia"),
                ["color_hex"] = FunctionDeclarationBuilder.Prop("string", "Color hexadecimal opcional, ej. #512BD4")
            },
            new[] { "name" },
            async (args, _, ct) =>
            {
                string? name = GetString(args, "name");
                if (string.IsNullOrWhiteSpace(name))
                    return ToolCallResult.Failure("add_subject", "La materia necesita un nombre.");

                var all = await _subjects.GetAllAsync();
                if (all.Any(s => Normalize(s.Name) == Normalize(name)))
                    return ToolCallResult.Failure("add_subject", $"La materia '{name.Trim()}' ya existe.");

                var subject = new Subject
                {
                    Name = name.Trim(),
                    ColorHex = GetString(args, "color_hex") ?? DefaultColorHex
                };

                int id = await _subjects.CreateAsync(subject);
                return ToolCallResult.Ok("add_subject", $"Materia '{subject.Name}' creada (id {id}).");
            });

        Register("update_subject",
            "Modifica el nombre o el color de una materia existente. Requiere subject_id y la confirmación explícita del usuario.",
            new()
            {
                ["subject_id"] = FunctionDeclarationBuilder.Prop("integer", "Id de la materia"),
                ["name"] = FunctionDeclarationBuilder.Prop("string", "Nuevo nombre (opcional)"),
                ["color_hex"] = FunctionDeclarationBuilder.Prop("string", "Nuevo color hexadecimal (opcional)")
            },
            new[] { "subject_id" },
            async (args, confirmed, ct) =>
            {
                int? subjectId = GetInt(args, "subject_id");
                if (subjectId is null)
                    return ToolCallResult.Failure("update_subject", "Falta el id de la materia.");

                var subject = await _subjects.GetByIdAsync(subjectId.Value);
                if (subject is null)
                    return ToolCallResult.Failure("update_subject", $"No encontré la materia con id {subjectId}.");

                string? name = GetString(args, "name");
                string? colorHex = GetString(args, "color_hex");

                var changes = new List<string>();
                if (!string.IsNullOrWhiteSpace(name)) changes.Add($"nombre → '{name.Trim()}'");
                if (!string.IsNullOrWhiteSpace(colorHex)) changes.Add($"color → {colorHex}");

                if (changes.Count == 0)
                    return ToolCallResult.Failure("update_subject", "No indicaste qué cambiar de la materia.");

                if (!confirmed)
                    return ToolCallResult.NeedsConfirmation("update_subject",
                        $"Voy a modificar la materia '{subject.Name}': {string.Join(", ", changes)}. ¿Confirmás?");

                if (!string.IsNullOrWhiteSpace(name)) subject.Name = name.Trim();
                if (!string.IsNullOrWhiteSpace(colorHex)) subject.ColorHex = colorHex;

                await _subjects.UpdateAsync(subject);
                return ToolCallResult.Ok("update_subject", $"Materia '{subject.Name}' actualizada.");
            });

        Register("delete_subject",
            "Borra una materia existente junto con sus horarios y tareas. Requiere subject_id y la confirmación explícita del usuario.",
            new()
            {
                ["subject_id"] = FunctionDeclarationBuilder.Prop("integer", "Id de la materia a borrar")
            },
            new[] { "subject_id" },
            async (args, confirmed, ct) =>
            {
                int? subjectId = GetInt(args, "subject_id");
                if (subjectId is null)
                    return ToolCallResult.Failure("delete_subject", "Falta el id de la materia.");

                var subject = await _subjects.GetByIdAsync(subjectId.Value);
                if (subject is null)
                    return ToolCallResult.Failure("delete_subject", $"No encontré la materia con id {subjectId}.");

                var schedules = await _subjects.GetSchedulesAsync(subject.Id);
                var tasks = await _tasks.GetBySubjectAsync(subject.Id);

                if (!confirmed)
                    return ToolCallResult.NeedsConfirmation("delete_subject",
                        $"Voy a borrar la materia '{subject.Name}' con {tasks.Count} tarea(s) y {schedules.Count} horario(s). Esta acción no se puede deshacer. ¿Confirmás?");

                await _database.DeleteSchedulesBySubjectAsync(subject.Id);
                await _database.DeleteTasksBySubjectAsync(subject.Id);
                await _subjects.DeleteAsync(subject);

                return ToolCallResult.Ok("delete_subject",
                    $"Materia '{subject.Name}' borrada junto con {tasks.Count} tarea(s) y {schedules.Count} horario(s).");
            });
    }

    private void RegisterScheduleTools()
    {
        Register("add_schedule",
            "Agrega un horario semanal a una materia existente. Los horarios usan formato HH:mm. No requiere confirmación.",
            new()
            {
                ["subject_name"] = FunctionDeclarationBuilder.Prop("string", "Nombre de la materia existente"),
                ["subject_id"] = FunctionDeclarationBuilder.Prop("integer", "Id de la materia, si ya lo conocés"),
                ["day"] = FunctionDeclarationBuilder.Prop("integer", "Día de la semana: 1=Lunes ... 7=Domingo"),
                ["start_time"] = FunctionDeclarationBuilder.Prop("string", "Hora de inicio en formato HH:mm"),
                ["end_time"] = FunctionDeclarationBuilder.Prop("string", "Hora de fin en formato HH:mm")
            },
            new[] { "day", "start_time", "end_time" },
            async (args, _, ct) =>
            {
                if (!TryParseDay(args, out var day))
                    return ToolCallResult.Failure("add_schedule", "No pude interpretar el día de la semana (usá 1=Lunes ... 7=Domingo).");

                if (!TryParseTime(GetString(args, "start_time"), out var start) || !TryParseTime(GetString(args, "end_time"), out var end))
                    return ToolCallResult.Failure("add_schedule", "No pude interpretar el horario. Usá formato HH:mm, por ejemplo 18:00.");

                if (end <= start)
                    return ToolCallResult.Failure("add_schedule", "La hora de fin tiene que ser posterior a la de inicio.");

                var subject = await ResolveSubjectAsync(GetInt(args, "subject_id"), GetString(args, "subject_name"));
                if (subject is null)
                    return ToolCallResult.Failure("add_schedule", "Necesito una materia existente para el horario. Si no existe, creala primero con add_subject.");

                var schedule = new Schedule
                {
                    SubjectId = subject.Id,
                    Day = day,
                    StartTime = start,
                    EndTime = end
                };

                int id = await _subjects.AddScheduleAsync(schedule);
                return ToolCallResult.Ok("add_schedule",
                    $"Horario agregado a {subject.Name}: {DayLabel(day)} de {start:hh\\:mm} a {end:hh\\:mm} (id {id}).");
            });

        Register("update_schedule",
            "Modifica un horario existente (día, hora de inicio o de fin). Requiere schedule_id y la confirmación explícita del usuario.",
            new()
            {
                ["schedule_id"] = FunctionDeclarationBuilder.Prop("integer", "Id del horario"),
                ["day"] = FunctionDeclarationBuilder.Prop("integer", "Nuevo día: 1=Lunes ... 7=Domingo (opcional)"),
                ["start_time"] = FunctionDeclarationBuilder.Prop("string", "Nueva hora de inicio HH:mm (opcional)"),
                ["end_time"] = FunctionDeclarationBuilder.Prop("string", "Nueva hora de fin HH:mm (opcional)")
            },
            new[] { "schedule_id" },
            async (args, confirmed, ct) =>
            {
                int? scheduleId = GetInt(args, "schedule_id");
                if (scheduleId is null)
                    return ToolCallResult.Failure("update_schedule", "Falta el id del horario.");

                var schedule = await FindScheduleAsync(scheduleId.Value);
                if (schedule is null)
                    return ToolCallResult.Failure("update_schedule", $"No encontré el horario con id {scheduleId}.");

                var subject = await _subjects.GetByIdAsync(schedule.SubjectId);
                string subjectName = subject?.Name ?? $"materia {schedule.SubjectId}";

                var newDay = schedule.Day;
                bool dayChanged = false;
                if (HasProperty(args, "day"))
                {
                    if (!TryParseDay(args, out newDay))
                        return ToolCallResult.Failure("update_schedule", "No pude interpretar el nuevo día de la semana.");
                    dayChanged = newDay != schedule.Day;
                }

                var newStart = schedule.StartTime;
                var newEnd = schedule.EndTime;
                if (GetString(args, "start_time") is string startRaw)
                {
                    if (!TryParseTime(startRaw, out newStart))
                        return ToolCallResult.Failure("update_schedule", "No pude interpretar la nueva hora de inicio.");
                }

                if (GetString(args, "end_time") is string endRaw)
                {
                    if (!TryParseTime(endRaw, out newEnd))
                        return ToolCallResult.Failure("update_schedule", "No pude interpretar la nueva hora de fin.");
                }

                if (newEnd <= newStart)
                    return ToolCallResult.Failure("update_schedule", "La hora de fin tiene que ser posterior a la de inicio.");

                bool timeChanged = newStart != schedule.StartTime || newEnd != schedule.EndTime;
                if (!dayChanged && !timeChanged)
                    return ToolCallResult.Failure("update_schedule", "No indicaste qué cambiar del horario.");

                if (!confirmed)
                    return ToolCallResult.NeedsConfirmation("update_schedule",
                        $"Voy a modificar el horario de {subjectName}: {DayLabel(newDay)} de {newStart:hh\\:mm} a {newEnd:hh\\:mm}. ¿Confirmás?");

                schedule.Day = newDay;
                schedule.StartTime = newStart;
                schedule.EndTime = newEnd;

                await _subjects.UpdateScheduleAsync(schedule);
                return ToolCallResult.Ok("update_schedule",
                    $"Horario de {subjectName} actualizado: {DayLabel(newDay)} de {newStart:hh\\:mm} a {newEnd:hh\\:mm}.");
            });

        Register("delete_schedule",
            "Borra un horario existente. Requiere schedule_id y la confirmación explícita del usuario.",
            new()
            {
                ["schedule_id"] = FunctionDeclarationBuilder.Prop("integer", "Id del horario a borrar")
            },
            new[] { "schedule_id" },
            async (args, confirmed, ct) =>
            {
                int? scheduleId = GetInt(args, "schedule_id");
                if (scheduleId is null)
                    return ToolCallResult.Failure("delete_schedule", "Falta el id del horario.");

                var schedule = await FindScheduleAsync(scheduleId.Value);
                if (schedule is null)
                    return ToolCallResult.Failure("delete_schedule", $"No encontré el horario con id {scheduleId}.");

                var subject = await _subjects.GetByIdAsync(schedule.SubjectId);
                string subjectName = subject?.Name ?? $"materia {schedule.SubjectId}";
                string label = $"{DayLabel(schedule.Day)} de {schedule.StartTime:hh\\:mm} a {schedule.EndTime:hh\\:mm}";

                if (!confirmed)
                    return ToolCallResult.NeedsConfirmation("delete_schedule",
                        $"Voy a borrar el horario de {subjectName}: {label}. ¿Confirmás?");

                await _subjects.DeleteScheduleAsync(schedule);
                return ToolCallResult.Ok("delete_schedule", $"Horario de {subjectName} ({label}) borrado.");
            });
    }

    // ==================== Helpers ====================

    private async Task<Subject?> ResolveSubjectAsync(int? subjectId, string? subjectName)
    {
        if (subjectId is int id)
        {
            var byId = await _subjects.GetByIdAsync(id);
            if (byId is not null)
                return byId;
        }

        if (string.IsNullOrWhiteSpace(subjectName))
            return null;

        var all = await _subjects.GetAllAsync();
        var normalized = Normalize(subjectName);
        if (normalized.Length == 0)
            return null;

        return all.FirstOrDefault(s => Normalize(s.Name) == normalized)
            ?? all.FirstOrDefault(s =>
            {
                var candidate = Normalize(s.Name);
                return candidate.Contains(normalized, StringComparison.Ordinal)
                    || normalized.Contains(candidate, StringComparison.Ordinal);
            });
    }

    private async Task<Schedule?> FindScheduleAsync(int scheduleId)
    {
        var all = await _database.GetAllSchedulesAsync();
        return all.FirstOrDefault(s => s.Id == scheduleId);
    }

    private static bool HasProperty(JsonElement args, string name)
        => args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value)
            && value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;

    private static string? GetString(JsonElement args, string name)
    {
        if (!HasProperty(args, name))
            return null;

        var value = args.GetProperty(name);
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static int? GetInt(JsonElement args, string name)
    {
        if (!HasProperty(args, name))
            return null;

        var value = args.GetProperty(name);
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
            return number;

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out int parsed))
            return parsed;

        return null;
    }

    private static bool TryParseDate(string? raw, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            && !DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
            return false;

        // Si la IA manda solo la fecha, vence al final del día.
        date = parsed.TimeOfDay == TimeSpan.Zero
            ? parsed.Date.AddHours(23).AddMinutes(59)
            : parsed;

        return true;
    }

    private static bool TryParseTime(string? raw, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        string[] formats = { "h\\:mm", "hh\\:mm", "h\\:mm\\:ss", "hh\\:mm\\:ss" };
        if (TimeSpan.TryParseExact(raw.Trim(), formats, CultureInfo.InvariantCulture, out time))
            return true;

        return TimeSpan.TryParse(raw.Trim(), CultureInfo.InvariantCulture, out time);
    }

    private static bool TryParseDay(JsonElement args, out DiaSemana day)
    {
        day = default;

        if (GetInt(args, "day") is int numeric)
        {
            if (numeric is < 1 or > 7)
                return false;
            day = (DiaSemana)numeric;
            return true;
        }

        string? raw = GetString(args, "day");
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        day = Normalize(raw) switch
        {
            "lunes" => DiaSemana.Lunes,
            "martes" => DiaSemana.Martes,
            "miercoles" => DiaSemana.Miércoles,
            "jueves" => DiaSemana.Jueves,
            "viernes" => DiaSemana.Viernes,
            "sabado" => DiaSemana.Sábado,
            "domingo" => DiaSemana.Domingo,
            _ => default
        };

        return day != default;
    }

    private static bool TryParseStatus(string raw, out ItemStatus status)
    {
        switch (Normalize(raw))
        {
            case "pending":
            case "pendiente":
                status = ItemStatus.Pending;
                return true;
            case "in progress": // Normalize colapsa el "_" del valor del esquema a espacio
            case "inprogress":
            case "en progreso":
            case "enprogreso":
                status = ItemStatus.InProgress;
                return true;
            case "completed":
            case "complete":
            case "completada":
            case "completado":
                status = ItemStatus.Completed;
                return true;
            case "expired":
            case "vencida":
            case "vencido":
                status = ItemStatus.Expired;
                return true;
            default:
                status = default;
                return false;
        }
    }

    private static string DayLabel(DiaSemana day) => day switch
    {
        DiaSemana.Lunes => "lunes",
        DiaSemana.Martes => "martes",
        DiaSemana.Miércoles => "miércoles",
        DiaSemana.Jueves => "jueves",
        DiaSemana.Viernes => "viernes",
        DiaSemana.Sábado => "sábado",
        DiaSemana.Domingo => "domingo",
        _ => day.ToString()
    };

    /// <summary>
    /// Normaliza texto: minúsculas, sin acentos y con cualquier separador
    /// (espacios, guiones, puntuación) colapsado a un solo espacio.
    /// Así "Físico-Química", "Físico Química" y "Físico - Química" comparan igual.
    /// </summary>
    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var stripped = new StringBuilder(decomposed.Length);

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stripped.Append(c);
        }

        var cleaned = stripped.ToString().Normalize(NormalizationForm.FormC);
        var builder = new StringBuilder(cleaned.Length);
        bool pendingSeparator = false;

        foreach (char c in cleaned)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (pendingSeparator && builder.Length > 0)
                    builder.Append(' ');
                pendingSeparator = false;
                builder.Append(c);
            }
            else
            {
                pendingSeparator = true;
            }
        }

        return builder.ToString();
    }
}
