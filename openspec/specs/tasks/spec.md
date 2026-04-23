# Tasks Specification

## Purpose

Sistema de tareas asociadas a materias. Cada tarea puede tener subtareas, fecha de vencimiento, y estados que se actualizan automáticamente.

## Requirements

### Requirement: Create Task

El sistema DEBE permitir crear una tarea asociada a una materia.

#### Scenario: Crear tarea con fecha específica

- GIVEN existe materia "Cálculo I"
- WHEN el usuario crea tarea "TP1: Límites" con vencimiento 20/04/2026
- THEN la tarea se crea con estado "Pendiente"
- AND se persiste en SQLite

#### Scenario: Crear tarea con asignación automática a próxima clase

- GIVEN materia "Física" tiene próxima clase el Jueves 18/04/2026 a las 08:00
- WHEN el usuario selecciona "Asignar a próxima clase"
- THEN la tarea se crea con fecha de vencimiento 18/04/2026 08:00
- AND se programa notificación según anticipación

### Requirement: Create Task with Subtasks

El sistema DEBE permitir crear tareas con subtareas.

#### Scenario: Crear tarea con subtareas

- GIVEN el usuario crea tarea "TP Final"
- WHEN agrega subtareas "Investigación", "Desarrollo", "Presentación"
- THEN la tarea tiene 3 subtareas en estado "Pendiente"
- AND cada subtarea es independiente

### Requirement: Complete Subtask

El sistema DEBE permitir marcar subtareas como completadas individualmente.

#### Scenario: Completar subtarea

- GIVEN tarea tiene 3 subtareas pendientes
- WHEN el usuario marca una subtarea como completada
- THEN esa subtarea cambia a estado "Completada"
- AND la tarea padre muestra progreso "1/3"

### Requirement: Task States

El sistema DEBE gestionar los estados de tarea: Pendiente, EnProgreso, Completada, Vencida.

#### Scenario: Cambiar a En Progreso

- GIVEN tarea está en estado "Pendiente"
- WHEN el usuario marca "Comenzar"
- THEN el estado cambia a "EnProgreso"

#### Scenario: Completar tarea

- GIVEN tarea tiene todas sus subtareas completadas (o no tiene subtareas)
- WHEN el usuario marca "Completar"
- THEN el estado cambia a "Completada"
- AND se cancela cualquier notificación pendiente

#### Scenario: Marcar automáticamente como Vencida

- GIVEN tarea tiene fecha de vencimiento 15/04/2026 23:59
- WHEN la fecha actual pasa del vencimiento
- THEN el sistema cambia el estado a "Vencida"
- AND muestra indicador visual de vencimiento

### Requirement: List Tasks

El sistema DEBE mostrar lista de tareas filtrable por materia, estado y carrera activa.

#### Scenario: Ver tareas pendientes

- GIVEN existen tareas en diferentes estados
- WHEN el usuario filtra por "Pendiente"
- THEN solo muestra tareas con estado Pendiente o EnProgreso

#### Scenario: Ver tareas por materia

- GIVEN la carrera activa tiene materias "Cálculo" y "Física"
- WHEN el usuario filtra por "Cálculo"
- THEN solo muestra tareas de esa materia

### Requirement: Edit Task

El sistema DEBE permitir modificar título, descripción, fecha de vencimiento y anticipación de notificación.

#### Scenario: Modificar vencimiento

- GIVEN tarea tiene vencimiento 20/04/2026
- WHEN el usuario cambia a 25/04/2026
- THEN se reprograman las notificaciones
- AND se actualiza la fecha en SQLite

### Requirement: Delete Task

El sistema DEBE permitir eliminar una tarea y sus subtareas.

#### Scenario: Eliminar tarea

- GIVEN existe tarea con 2 subtareas
- WHEN el usuario confirma eliminación
- THEN se eliminan las subtareas y la tarea
- AND se cancelan las notificaciones programadas