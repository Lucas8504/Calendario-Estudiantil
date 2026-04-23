# Notifications Specification

## Purpose

Sistema de notificaciones locales inteligentes que avisa al usuario antes del vencimiento de tareas, con warnings visuales y flujo de confirmación al interactuar.

## Requirements

### Requirement: Schedule Notification

El sistema DEBE programar una notificación local según la anticipación configurada por el usuario.

#### Scenario: Programar notificación con anticipación de 24 horas

- GIVEN tarea vence el 20/04/2026 a las 08:00
- AND usuario configura anticipación de 24 horas
- WHEN se crea la tarea
- THEN se programa notificación para 19/04/2026 a las 08:00

#### Scenario: Programar notificación con anticipación de 2 días

- GIVEN tarea vence 20/04/2026 08:00
- AND anticipación configurada es "2 días"
- THEN se programa para 18/04/2026 08:00

### Requirement: Display Warning for Short Anticipation

El sistema DEBE mostrar warning visual de color rojo cuando la anticipación es menor a 12 horas.

#### Scenario: Anticipación menor a 12 horas muestra rojo

- GIVEN usuario configura anticipación de 6 horas
- WHEN visualiza los detalles de la tarea
- THEN el número de horas se muestra en color rojo
- AND tooltip indica "Poco tiempo restante"

#### Scenario: Anticipación mayor a 12 horas sin warning

- GIVEN anticipación es 24 horas
- WHEN se visualiza la tarea
- THEN el texto de anticipación aparece en color normal

### Requirement: Notification Content

El sistema DEBE mostrar información relevante en la notificación.

#### Scenario: Contenido de notificación

- GIVEN tarea "TP1 Cálculo" vence en 24 horas
- WHEN se dispara la notificación
- THEN muestra: "TP1 Cálculo - Vence mañana"
- AND código de la tarea para identificarla

### Requirement: Notification Action - Complete

El sistema DEBE permitir completar la tarea desde la notificación.

#### Scenario: Completar desde notificación

- GIVEN llega notificación de tarea
- WHEN el usuario toca opción "Completar"
- THEN la tarea cambia a estado "Completada"
- AND se cancelan notificaciones futuras
- AND se cierra la notificación

### Requirement: Notification Action - Snooze

El sistema DEBE permitir posponer la notificación.

#### Scenario: Posponer notificación

- GIVEN llega notificación
- WHEN usuario toca "Posponer 1 hora"
- THEN se programa nueva notificación para 1 hora después
- AND la original se marca como respondida

### Requirement: Cancel Notification - Completion Check

El sistema DEBE preguntar al usuario si completó la tarea cuando intenta cancelar una notificación de recordatorio.

#### Scenario: Preguntar al cancelar notificación

- GIVEN llega notificación de tarea pendiente
- WHEN el usuario intenta cerrar/cancelar la notificación
- THEN se muestra diálogo "¿Completaste la tarea?"
- AND opciones: "Sí, completada" / "No, mantener pendiente"

#### Scenario: Usuario confirma completitud

- GIVEN diálogo "¿Completaste la tarea?"
- WHEN usuario selecciona "Sí, completada"
- THEN tarea se marca como "Completada"
- AND se cancelan todas las notificaciones de esa tarea

#### Scenario: Usuario indica no completada

- GIVEN diálogo "¿Completaste la tarea?"
- WHEN usuario selecciona "No, mantener pendiente"
- THEN la tarea permanece en estado actual
- AND se reprograma notificación para más tarde (configurable)

### Requirement: Auto-Expire Tasks

El sistema DEBE marcar automáticamente tareas como vencidas cuando pasa la fecha límite.

#### Scenario: Tarea vence automáticamente

- GIVEN tarea tiene vencimiento 20/04/2026 23:59
- WHEN la fecha actual es 21/04/2026 00:01
- THEN el sistema cambia estado a "Vencida"
- AND se muestra indicador visual en la UI

### Requirement: Cancel All Notifications on Complete

El sistema DEBE cancelar todas las notificaciones pendientes cuando una tarea se marca como completada.

#### Scenario: Completar tarea cancela notificaciones

- GIVEN tarea tiene 2 notificaciones programadas (anticipación + recordatorio extra)
- WHEN usuario completa la tarea
- THEN se cancelan las 2 notificaciones
- AND no vuelve a reminders