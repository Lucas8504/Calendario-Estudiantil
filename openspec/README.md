# Calendario Estudiantil — Documentación del Proyecto

> Proyecto de calendario estudiantil para organización de materias, horarios y tareas.

## Índice

1. [Proposal](proposal.md) — Visión y alcance del proyecto
2. [Specs](specs/) — Especificaciones formales por dominio
3. [Design](design.md) — Decisiones técnicas
4. [Tasks](tasks.md) — Desglose de implementación

---

## Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|-------------|---------|
| Framework | .NET MAUI | 8.0 |
| MVVM | CommunityToolkit.Mvvm | 8.2.2 |
| Persistencia | sqlite-net-pcl | 1.8.116 |
| Notificaciones | Plugin.LocalNotification | 6.3.2 |

## Arquitectura

```
Calendario Estudiantil/
├── Models/           # Entidades (Course, Subject, Schedule, TaskItem, Subtask)
├── Services/         # Lógica de negocio
├── Data/             # DatabaseService
├── ViewModels/       # MVVM ViewModels
├── Views/            # XAML Pages
├── Converters/       # Value Converters
└── Helpers/         # Utilities
```

## Dominios

| Dominio | Descripción |
|---------|------------|
| `courses` | Gestión de múltiples carreras |
| `subjects` | Materias con horarios (automático/manual) |
| `schedule-grid` | Vista semanal con grid de franjas horarias |
| `tasks` | Sistema de tareas con subtareas |
| `notifications` | Notificaciones inteligentes con warning |
| `system-integration` | Dark mode, widgets, calendario nativo |

## Estados

| Estado | Descripción |
|--------|------------|
| ✅ Proposal | Completo |
| ✅ Specs | Completo (6 dominios) |
| ✅ Design | Completo |
| ✅ Tasks | Completo (~40 tareas en 6 fases) |
| 🔲 Implementación | Pendiente |

---

## Roadmap de Implementación

### Phase 1: Infrastructure
- NuGet packages
- Models (todas las entidades)
- DatabaseService

### Phase 2: Services
- CourseService, SubjectService, TaskService
- NotificationService

### Phase 3: ViewModels
- MainViewModel, CoursesViewModel, ScheduleViewModel, TasksViewModel

### Phase 4: Views
- Pages: Courses, Subjects, Schedule, Tasks, TaskDetail

### Phase 5: Notifications
- Programación, warning rojo (<12h), dialog "¿completaste?"

### Phase 6: System Integration
- Dark mode, indicador hora actual, persistencia ActiveCourse

---

## Decisiones Clave

| Decisión | Elección | Justificación |
|----------|---------|---------------|
| Arquitectura | MVVM simplificada | MAUI es naturalmente MVVM-friendly |
| Grid semanal | Custom CollectionView | NO Syncfusion (gratis vs pago) |
| Widgets | Postergar v2 | Requiere implementación nativa Android |
| Calendario nativo | Postergar v2 | Permisos Android 13+ complejos |

---

## Features Prioritarios (v1)

### Core
- [x] Vista semanal grid con franjas horarias
- [x] CRUD carreras con colores
- [x] CRUD materias con horarios
- [x] Horario automático (días de semana) o manual
- [x] CRUD tareas con subtareas
- [x] Asignar tarea a próxima clase automáticamente
- [x] Estados de tarea: Pendiente, En Progreso, Completada, Vencida
- [x] Notificaciones locales con anticipación configurable
- [x] Warning rojo si anticipación < 12 horas
- [x] Dialog "¿completaste?" al cancelar notificación
- [x] Auto-vencimiento de tareas

### Extras (v2)
- [ ] Widgets en home del celular
- [ ] Integración con calendario nativo
- [ ] Sincronización en la nube

---

## Estructura de Carpetas del Proyecto

```
Calendario Estudiantil/
├── Models/
│   ├── Course.cs
│   ├── Subject.cs
│   ├── Schedule.cs
│   ├── TaskItem.cs
│   ├── Subtask.cs
│   └── DayOfWeek.cs (enum)
├── Services/
│   ├── ICourseService.cs
│   ├── CourseService.cs
│   ├── ISubjectService.cs
│   ├── SubjectService.cs
│   ├── ITaskService.cs
│   ├── TaskService.cs
│   ├── INotificationService.cs
│   └── NotificationService.cs
├── Data/
│   └── DatabaseService.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── CoursesViewModel.cs
│   ├── ScheduleViewModel.cs
│   ├── TasksViewModel.cs
│   └── SubjectViewModel.cs
├── Views/
│   ├── CoursesPage.xaml/.cs
│   ├── SubjectsPage.xaml/.cs
│   ├── SchedulePage.xaml/.cs
│   ├── TasksPage.xaml/.cs
│   └── TaskDetailPage.xaml/.cs
├── Converters/
│   └── Converters.cs
├── Helpers/
│   └── Helpers.cs
└── Resources/
    ├── Styles/
    ├── Fonts/
    ├── Images/
    └── Splash/
```

---

## Entidades

### Course
```csharp
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; } // #RRGGBB
}
```

### Subject
```csharp
public class Subject
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; }
}
```

### Schedule
```csharp
public class Schedule
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public DayOfWeek Day { get; set; } // 1=Lunes, 7=Domingo
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
```

### TaskItem
```csharp
public enum TaskStatus { Pending, InProgress, Completed, Expired }
public class TaskItem
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; }
    public int NotificationHours { get; set; } // anticipación
}
```

### Subtask
```csharp
public class Subtask
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}
```

---

Generado: 2026-04-23
Versión SDD: calendario-fundacional v1.0