# Tasks: Calendario Estudiantil

## Phase 1: Infrastructure (Foundation)

- [ ] 1.1 Agregar NuGet packages al csproj: CommunityToolkit.Mvvm, sqlite-net-pcl, Plugin.LocalNotification
- [ ] 1.2 Crear carpeta Models/ y agregar Models/DayOfWeek.cs (enum: Lunes=1 .. Domingo=7)
- [ ] 1.3 Crear Models/Course.cs (Id, Name, ColorHex)
- [ ] 1.4 Crear Models/Subject.cs (Id, CourseId, Name, ColorHex)
- [ ] 1.5 Crear Models/Schedule.cs (Id, SubjectId, Day, StartTime, EndTime)
- [ ] 1.6 Crear Models/TaskItem.cs + TaskStatus enum (Pending, InProgress, Completed, Expired)
- [ ] 1.7 Crear Models/Subtask.cs (Id, TaskItemId, Title, IsCompleted)
- [ ] 1.8 Crear Data/DatabaseService.cs con sqlite-net-pcl (Init, CRUD genérico)

## Phase 2: Services (Business Logic)

- [ ] 2.1 Crear Services/INotificationService.cs (ScheduleAsync, CancelAsync, ShowCompletionDialogAsync)
- [ ] 2.2 Crear Services/NotificationService.cs implementando INotificationService
- [ ] 2.3 Crear Services/ICourseService.cs (GetAll, Create, Update, Delete, GetActive, SetActive)
- [ ] 2.4 Crear Services/CourseService.cs implementando ICourseService
- [ ] 2.5 Crear Services/ISubjectService.cs (GetByCourse, Create, Update, Delete)
- [ ] 2.6 Crear Services/SubjectService.cs implementando ISubjectService
- [ ] 2.7 Crear Services/ITaskService.cs (Create, GetBySubject, GetForToday, UpdateStatus, ScheduleNotification, CancelNotification)
- [ ] 2.8 Crear Services/TaskService.cs implementando ITaskService + lógica anticipación <12h warning

## Phase 3: ViewModels (Presentation)

- [ ] 3.1 Crear ViewModels/MainViewModel.cs (navegación Shell, ActiveCourse)
- [ ] 3.2 Crear ViewModels/CoursesViewModel.cs (ObservableCollection<Course>, CreateCommand, DeleteCommand, SelectActiveCommand)
- [ ] 3.3 Crear ViewModels/ScheduleViewModel.cs (horarios para semana, indicador hora actual)
- [ ] 3.4 Crear ViewModels/TasksViewModel.cs (ObservableCollection<TaskItem>, CreateCommand, CompleteCommand, SubtaskLogic)
- [ ] 3.5 Crear ViewModels/SubjectViewModel.cs (edición materia + horarios)

## Phase 4: Views (UI)

- [ ] 4.1 Crear Helpers/Converters.cs (TimeSpanToString, ColorHexToColor, ShortAnticipationWarning)
- [ ] 4.2 Modificar App.xaml.cs: inicializar DatabaseService, cargar ActiveCourse
- [ ] 4.3 Modificar MauiProgram.cs: registrar services en DI
- [ ] 4.4 Modificar AppShell.xaml: Flyout menu con 4 tabs (Calendario, Tareas, Materias, Carreras)
- [ ] 4.5 Crear Views/CoursesPage.xaml/.cs: CRUD carreras con ColorPicker
- [ ] 4.6 Crear Views/subjectsPage.xaml/.cs: lista materias por carrera activa
- [ ] 4.7 Crear Views/SchedulePage.xaml/.cs: grid semanal custom con CollectionView
- [ ] 4.8 Crear Views/TasksPage.xaml/.cs: lista filtrable + crear tarea + subtareas
- [ ] 4.9 Crear Views/TaskDetailPage.xaml/.cs: detalle tarea con subtareas

## Phase 5: Notifications (UX)

- [ ] 5.1 Configurar Plugin.LocalNotification en MauiProgram.cs
- [ ] 5.2 Implementar NotificationService.ScheduleAsync con anticipación configurable
- [ ] 5.3 Implementar warning rojo UI si NotificationHours < 12
- [ ] 5.4 Implementar ShowCompletionDialogAsync: "¿Completaste la tarea?" con Sí/No
- [ ] 5.5 Crear lógica auto-expire: marcar Vencida si DueDate < now

## Phase 6: System Integration (Polish)

- [ ] 6.1 Soporte Dark Mode: usar AppThemeBinding en XAML
- [ ] 6.2 Indicador hora actual en grid (línea horizontal roja)
- [ ] 6.3 Botón "Hoy" para navegar a semana actual
- [ ] 6.4 Persistir ActiveCourseId en Preferences (app close/open)
- [ ] 6.5 Eliminar MainPage.xaml y MainPage.xaml.cs (reemplazados por Views/)

---

## Implementation Order

```
csproj (NuGet)
    ↓
Models/ (todas las entidades primero)
    ↓
Data/ (DatabaseService depende de Models)
    ↓
Services/ (Services dependen de Data)
    ↓
ViewModels/ (ViewModels dependen de Services)
    ↓
Views/ (Views dependen de ViewModels)
    ↓
Helpers/ (Helpers son cross-cutting)
    ↓
Notifications (NotificationService depende de todo)
```

## Open Questions

- [ ] ¿Grid semanal con CollectionView o SfScheduler? Usar custom CollectionView (gratis)
- [ ] ¿Widgets Android? Postergar para v2 (requiere implementación nativa)
- [ ] ¿Integración calendario nativo? Postergar para v2 (permisos Android 13+)
- [ ] ¿Syncfusion.Maui.Scheduler? No - usar custom grid con CollectionView

---

**Total: ~35 tareas organizadas en 6 fases**
**Prioridad: Phase 1 → 2 → 3 → 4 → 5 → 6**