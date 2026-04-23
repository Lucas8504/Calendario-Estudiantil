# Propuesta: Calendario Estudiantil

## Intent

Crear una aplicación móvil de calendario estudiantil que permita organizar materias y horarios semanales mediante un grid de franjas horarias, gestionar tareas con notificaciones inteligentes, soportar múltiples carreras, y aprovechar features nativos del dispositivo (dark mode, widgets, integración calendario).

## Scope

### In Scope
- Vista semanal con grid de franjas horarias
- Gestión de materias (crear, editar, eliminar) con horario semanal automático o manual
- Soporte para múltiples carreras/cursos
- Sistema de tareas con subtareas
- Notificaciones locales con anticipación configurable (días/horas)
- Warning visual rojo si anticipación < 12 horas
- Pregunta al cancelar notificación: ¿tarea completada?
- Vencimiento automático de tareas no completadas
- Persistencia local con SQLite
- Dark mode automático del sistema
- Widgets en home del celular
- Integración con calendario nativo del sistema

### Out of Scope
- Sincronización en la nube
- Exportación/importación de datos
- Autenticación de usuarios
- Modo offline/online

## Capabilities

### New Capabilities
- `subjects`: Gestión de materias con horarios (automático/manual)
- `courses`: Gestión de múltiples carreras/cursos
- `schedule-grid`: Vista semanal con grid de franjas horarias
- `tasks`: Sistema de tareas con subtareas y estados
- `notifications`: Notificaciones inteligentes con warning y vencimiento
- `system-integration`: Widgets, calendario nativo, dark mode

## Approach

Arquitectura Clean Architecture adaptada a .NET MAUI:
- **Domain**: Entidades (Materia, Tarea, Carrera, Horario, Subtarea)
- **Application**: Use Cases para cada operación de negocio
- **Infrastructure**: SQLite + Servicios de notificaciones
- **Presentation**: MVVM con CommunityToolkit.Mvvm

Stack técnico:
- .NET 8 + MAUI
- CommunityToolkit.Mvvm para MVVM
- sqlite-net-pcl para base de datos local
- Plugin.LocalNotification para notificaciones
- Plugin.Calendario para integración con calendario del sistema

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| Domain/Entities | New | Materia, Carrera, Tarea, Subtarea, Horario |
| Application/UseCases | New | Casos de uso de negocio |
| Infrastructure/Data | New | SQLite con repositorios |
| Infrastructure/Services | New | Notificaciones, calendario |
| Presentation/ViewModels | New | MVVM ViewModels |
| Presentation/Views | New | XAML Pages |
| Platforms | Modified | Android widget, integración calendario |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Widgets iOS limitados | High | Priorizar Android, crear widget nativo si es crítico |
| Integración calendario variable por OS | Med | Usar plugin cross-platform con fallback |
| Notificaciones programadas pueden perderse | Low | Re-agendar al abrir app |

## Rollback Plan

Al ser proyecto nuevo, rollback implica eliminar todo el código fuente y mantener solo la estructura base de la plantilla MAUI original.

## Dependencies

- CommunityToolkit.Mvvm
- sqlite-net-pcl
- Plugin.LocalNotification
- Plugin.Calendario (por investigar alternativa mejor)

## Success Criteria
- [ ] Vista semanal grid funcional
- [ ] CRUD de materias con horarios
- [ ] CRUD de carreras
- [ ] CRUD de tareas con subtareas
- [ ] Notificaciones con warning <12h
- [ ] Pregunta al cancelar notificación
- [ ] Vencimiento automático de tareas
- [ ] Persistencia SQLite funcionando
- [ ] Dark mode funcionando
- [ ] Widget Android funcionando
- [ ] Integración calendario nativo funcionando

---

## Detalles Técnicos

### Entidades Principales
```
Carrera (Id, Nombre, Color)
Materia (Id, CarreraId, Nombre, Color, Horarios[])
Horario (Id, MateriaId, DiaSemana, HoraInicio, HoraFin)
Tarea (Id, MateriaId, Titulo, Descripcion, FechaVencimiento, Estado, AnticipacionNotificacion)
Subtarea (Id, TareaId, Titulo, Completada)
```

### Estados de Tarea
- Pendiente
- EnProgreso
- Completada
- Vencida

### Lógica de Notificaciones
1. Crear tarea → Programar notificación según anticipación
2. Si anticipación < 12h → Color rojo en UI
3. Al abrir notificación → Mostrar opciones: "Completar" o "Posponer"
4. Si usuario Cancela → Preguntar "¿Completaste la tarea?"
   - Si Sí → Marcar completada
   - Si No → Mantener pendiente
5. Si fecha vencimiento pasa →自动 marcar como Vencida