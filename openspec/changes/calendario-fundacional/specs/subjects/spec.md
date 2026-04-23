# Subjects Specification

## Purpose

Gestión de materias (asignaturas) asociadas a una carrera. Cada materia tiene horarios semanales que definen cuándo se dicta la clase.

## Requirements

### Requirement: Create Subject

El sistema DEBE permitir crear una nueva materia asociada a una carrera.

#### Scenario: Crear materia exitosamente

- GIVEN existe la carrera "Ingeniería"
- WHEN el usuario crea materia "Cálculo I" con color amarillo
- THEN la materia se crea y aparece en la lista de la carrera
- AND se muestran opciones para configurar horario

#### Scenario: Crear materia sin carrera activa

- GIVEN no hay carrera seleccionada
- WHEN el usuario intenta crear materia
- THEN el sistema muestra mensaje "Selecciona una carrera primero"
- AND no se crea la materia

### Requirement: Configure Schedule Automatically

El sistema DEBE permitir configurar el horario de una materia de forma automática para toda la semana.

#### Scenario: Horario automático semanal

- GIVEN el usuario crea materia "Física I"
- WHEN selecciona "Horario automático" con días Lun-Vie, hora 08:00-10:00
- THEN se generan 5 registros de horario (uno por cada día)
- AND todas las franjas aparecen en el grid del calendario

#### Scenario: Horario automático con días específicos

- GIVEN el usuario configura horario automático
- WHEN selecciona solo Lun-Mié-Vie
- THEN se generan 3 registros de horario para esos días

### Requirement: Configure Schedule Manually

El sistema DEBE permitir configurar el horario día por día de forma manual.

#### Scenario: Horario manual día por día

- GIVEN el usuario está en edición de materia
- WHEN agrega horario: Martes 14:00-16:00 y Jueves 14:00-16:00
- THEN se crean 2 registros de horario específicos
- AND aparecen en el grid solo esos días y horas

### Requirement: Edit Subject

El sistema DEBE permitir modificar nombre, color y horarios de una materia existente.

#### Scenario: Modificar materia

- GIVEN existe materia "Álgebra" con horario Lun 10:00-12:00
- WHEN el usuario cambia nombre a "Álgebra Lineal" y agrega horario Mie 10:00-12:00
- THEN la materia se actualiza con ambos horarios
- AND el grid del calendario refleja los cambios

### Requirement: Delete Subject

El sistema DEBE permitir eliminar una materia y todas sus tareas asociadas.

#### Scenario: Eliminar materia

- GIVEN existe materia "Química" con 3 tareas
- WHEN el usuario confirma eliminación
- THEN se eliminan las 3 tareas y la materia
- AND las tareas no aparecen más en el sistema

### Requirement: List Subjects by Course

El sistema DEBE mostrar las materias filtradas por la carrera activa.

#### Scenario: Ver materias de carrera activa

- GIVEN la carrera activa es "Ingeniería" con materias "Cálculo" y "Física"
- WHEN el usuario abre la lista de materias
- THEN ve solo esas 2 materias