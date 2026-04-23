# System Integration Specification

## Purpose

Integración con features nativos del sistema operativo: dark mode automático, widgets en home screen, y sincronización con el calendario nativo del dispositivo.

## Requirements

### Requirement: Dark Mode Support

El sistema DEBE soportar tema oscuro y seguir la configuración del sistema.

#### Scenario: Dark mode automático

- GIVEN el dispositivo tiene modo oscuro activado
- WHEN el usuario abre la aplicación
- THEN la interfaz se muestra en tema oscuro
- AND los colores de materias se ajustan para contraste

#### Scenario: Cambiar a modo claro

- GIVEN la app está en dark mode
- WHEN el usuario desactiva dark mode en el sistema
- THEN la app cambia a tema claro automáticamente
- AND la preferencia se sincroniza

### Requirement: Widget - Today's Tasks

El sistema DEBE proporcionar un widget que muestre las tareas del día.

#### Scenario: Widget muestra tareas de hoy

- GIVEN existen tareas pendientes para hoy
- WHEN el usuario agrega el widget al home
- THEN el widget muestra lista de tareas con materia y hora
- AND actualiza automáticamente

#### Scenario: Widget vacío

- GIVEN no hay tareas para hoy
- WHEN se visualiza el widget
- THEN muestra "Sin tareas para hoy"

#### Scenario: Widget muestra próxima clase

- GIVEN hay materia a las 10:00
- WHEN el widget se configura para mostrar "Próxima clase"
- THEN indica "Cálculo I a las 10:00"

### Requirement: Widget Configuration

El sistema DEBE permitir configurar qué muestra el widget.

#### Scenario: Configurar widget

- GIVEN el usuario/edita la configuración del widget
- WHEN selecciona "Próxima clase" como contenido
- THEN el widget muestra la siguiente clase del día

### Requirement: System Calendar Integration

El sistema DEBE sincronizar las materias y tareas con el calendario nativo del dispositivo.

#### Scenario: Sincronizar materia al calendario

- GIVEN materia tiene horario Lun 08:00-10:00
- WHEN el usuario activa "Sincronizar con calendario"
- THEN se crea evento recurrente en el calendario del sistema
- AND se actualiza automáticamente si cambia el horario

#### Scenario: Agregar tarea al calendario

- GIVEN tarea tiene vencimiento 20/04/2026
- WHEN el usuario selecciona "Agregar al calendario"
- THEN se crea evento de recordatorio en el calendario nativo

### Requirement: Calendar Permissions

El sistema DEBE solicitar permisos apropiados para acceder al calendario del sistema.

#### Scenario: Solicitar permiso de calendario

- GIVEN el usuario intenta sincronizar por primera vez
- WHEN toca "Sincronizar con calendario"
- THEN el sistema muestra diálogo de permiso
- AND si acepta, permite la sincronización

#### Scenario: Permiso denegado

- GIVEN el usuario negó el permiso de calendario
- WHEN intenta sincronizar
- THEN muestra mensaje "Permiso de calendario requerido"
- AND ofrece abrir configuración

### Requirement: Widget Update on Task Change

El sistema DEBE actualizar el widget cuando cambian las tareas.

#### Scenario: Nueva tarea actualiza widget

- GIVEN el widget está en home
- WHEN se crea nueva tarea para hoy
- THEN el widget se actualiza para mostrarla
- AND el usuario ve el cambio sin abrir la app

### Requirement: Background Refresh

El sistema DEBE actualizar datos en background para mantener widgets actualizados.

#### Scenario: Actualización en background

- GIVEN la app está cerrada
- WHEN hay cambio en tareas
- THEN el sistema actualiza el widget periódicamente
- AND muestra información actualizada