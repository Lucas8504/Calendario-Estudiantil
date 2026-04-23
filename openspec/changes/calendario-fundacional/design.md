# Design: Calendario Estudiantil

## Technical Approach

Aplicación Android/iOS MAUI con arquitectura MVVM usando CommunityToolkit.Mvvm. Persistencia local SQLite con sqlite-net-pcl. Las entidades principales (Carrera, Materia, Horario, Tarea, Subtarea) se mapean 1:1 a tablas SQLite. Navigation via Shell de MAUI con Flyout menu.

## Architecture Decisions

### Decision: MVVM vs Clean Architecture simplificado

**Choice**: Arquitectura MVVM simplificada (no full Clean Architecture layers)
**Alternatives considered**: Clean Architecture (Domain/Application/Infrastructure/Presentation)
**Rationale**: MAUI es naturalmente MVVM-friendly. Con ViewModels + Services + Repositories cubrimos todo sin overhead. Si el proyecto escala, se puede refactorizar después.

### Decision: Paquetes NuGet

| Paquete | Propósito | Alternativa rechazada |
|--------|---------|---------------------|
| CommunityToolkit.Mvvm | MVVM declarativo con [ObservableProperty] | MvvmCross (más pesado) |
| sqlite-net-pcl | Persistencia SQLite | EF Core (overkill para mobile) |
| Plugin.LocalNotification | Notificaciones locales | Rg.Plugins.Popup (no es para notificaciones) |

### Decision: Estructura de carpetas

```
Calendario Estudiantil/
├── Models/              # Entidades + Enums (sin lógica)
├── Services/             # Lógica de negocio (notificaciones, calendario)
├── Data/                 # DatabaseService + Repositorios
├── ViewModels/           # Uno por View (MVVM)
├── Views/                 # XAML Pages
├── Converters/           # Value Converters para bindings
└── Helpers/              # Extensions, utilities
```

**No** Domain/Application/Infrastructure separados. Todo en raíz del proyecto para simplicidad.

### Decision: Estado de carrera activa

**Choice**: Preferences.GetInt("ActiveCourseId", -1) para persistir carrera activa
**Alternatives considered**: Service singleton con ObservableProperty
**Rationale**: Persiste entre sesiones sin сложности. Se lee al iniciar App y se inyecta en ViewModels.

## Data Flow

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│    View    │ ←── │  ViewModel   │ ←── │  Service  │
│  (XAML)   │ ──→ │ (Commands)   │ ──→ │(Database) │
└─────────────┘     └──────────────┘     └───────────┘
                         │
                         ↓
                  ┌──────────────┐
                  │Notification │
                  │  Service    │
                  └─────────────┘
```

**Flujo creación tarea:**
1. User llena formulario en View
2. Command en ViewModel valida y llama TaskService
3. TaskService.Inspect() persiste en SQLite + programa notificación
4. ViewModel obtiene resultado y actualiza UI via ObservableCollection

## Entidades (Models)

```csharp
// Models/Course.cs
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; }  // #RRGGBB
}

// Models/Subject.cs
public class Subject
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; }
}

// Models/Schedule.cs
public class Schedule
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public DayOfWeek Day { get; set; }      // 1=Lunes, 7=Domingo
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

// Models/TaskItem.cs
public enum TaskStatus { Pending, InProgress, Completed, Expired }
public class TaskItem
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; }
    public int NotificationHours { get; set; }  // anticipación
}

// Models/Subtask.cs
public class Subtask
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}
```

## Interfaces / Contratos

```csharp
// Services/ITaskService.cs
public interface ITaskService
{
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<List<TaskItem>> GetTasksBySubjectAsync(int subjectId);
    Task<List<TaskItem>> GetTasksForTodayAsync();
    Task UpdateStatusAsync(int taskId, TaskStatus status);
    Task ScheduleNotificationAsync(TaskItem task);
    Task CancelNotificationAsync(int taskId);
}

// Services/INotificationService.cs
public interface INotificationService
{
    Task ScheduleAsync(int taskId, string title, DateTime notifyAt);
    Task CancelAsync(int taskId);
    Task<bool> ShowCompletionDialogAsync(string taskTitle);
}
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `Models/Course.cs` | Create | Entidad Carrera |
| `Models/Subject.cs` | Create | Entidad Materia |
| `Models/Schedule.cs` | Create | Entidad Horario |
| `Models/TaskItem.cs` | Create | Entidad Tarea + Enum TaskStatus |
| `Models/Subtask.cs` | Create | Entidad Subtarea |
| `Models/DayOfWeek.cs` | Create | Enum: Lunes=1 .. Domingo=7 |
| `Data/DatabaseService.cs` | Create | sqlite-net-pcl wrapper |
| `Services/ITaskService.cs` | Create | Interfaz TaskService |
| `Services/TaskService.cs` | Create | Implementación |
| `Services/NotificationService.cs` | Create | Wrapper Plugin.LocalNotification |
| `ViewModels/CoursesViewModel.cs` | Create | Lista + CRUD carreras |
| `ViewModels/ScheduleViewModel.cs` | Create | Grid semanal |
| `ViewModels/TasksViewModel.cs` | Create | Lista + CRUD tareas |
| `ViewModels/MainViewModel.cs` | Create | Shell, navegación |
| `Views/CoursesPage.xaml/.cs` | Create | Página gestión carreras |
| `Views/SchedulePage.xaml/.cs` | Create | Grid semanal |
| `Views/TasksPage.xaml/.cs` | Create | Lista de tareas |
| `Converters/TimeSpanToStringConverter.cs` | Create | Para bindings de hora |
| `MauiProgram.cs` | Modify | Agregar DI services |
| `App.xaml/.cs` | Modify | Configurar tema, Init DB |
| `AppShell.xaml/.cs` | Modify | Flyout menu con tabs |

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | Services (CreateTask, ScheduleNotification logic) | Unit tests con Moq |
| Unit | ViewModels (Commands, Validation) | CommunityToolkit.Mvvm testing |
| Integration | SQLite CRUD operations | sqlite-net-pcl in-memory |

**Strict TDD Mode**: disabled — sin test runner configurado en proyecto.

## Migration / Rollback

No migration required. Proyecto nuevo sin datos existentes.

Rollback: eliminar todo excepto `/Platforms/` y `Resources/`. Volver a plantilla MAUI.

## Open Questions

- [ ] ¿Usar SfScheduler de Syncfusion o grid custom? SfScheduler es de pago con licencia community limitada.
- [ ] ¿Widgets Android requieren implementación nativa o podemos usar tercer que soporte MAUI?
- [ ] ¿Integración con calendario nativo del sistema requiere permiso especial en Android 13+?
- [ ] ¿Prefiere Syncfusion.Maui.Scheduler para el grid o implementar custom grid con CollectionView?

## NuGet Packages a agregar

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="sqlite-net-pcl" Version="1.8.116" />
<PackageReference Include="Plugin.LocalNotification" Version="6.3.2" />
```

---

**Documento v1.0** — 2026-04-23