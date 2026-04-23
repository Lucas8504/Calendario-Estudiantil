# Courses Specification

## Purpose

Gestión de múltiples carreras o cursos universitarios. Permite al usuario organizar sus estudios por carrera y mantener separadas las materias de cada una.

## Requirements

### Requirement: Create Course

El sistema DEBE permitir crear una nueva carrera con nombre y color de identificación.

#### Scenario: Crear carrera exitosamente

- GIVEN el usuario está en la pantalla de gestión de carreras
- WHEN ingresa nombre "Ingeniería en Sistemas" y selecciona color azul
- THEN la carrera se crea y aparece en la lista de carreras
- AND la carrera se persiste en SQLite

#### Scenario: Crear carrera sin nombre

- GIELD el usuario intenta crear una carrera
- WHEN deja el campo nombre vacío
- THEN el sistema muestra mensaje de error "El nombre es requerido"
- AND no se crea la carrera

### Requirement: Edit Course

El sistema DEBE permitir modificar el nombre y color de una carrera existente.

#### Scenario: Modificar carrera

- GIVEN existe una carrera "Medicina" con color rojo
- WHEN el usuario cambia el nombre a "Medicina Humana" y color verde
- THEN los cambios se guardan automáticamente
- AND todas las materias de esa carrera reflejan el cambio

### Requirement: Delete Course

El sistema DEBE permitir eliminar una carrera, con todas sus materias y tareas asociadas.

#### Scenario: Eliminar carrera con materias

- GIVEN existe una carrera con 3 materias y 5 tareas
- WHEN el usuario confirma la eliminación
- THEN se eliminan las 3 materias, sus horarios y 5 tareas
- AND se elimina la carrera
- AND la base de datos queda consistente

### Requirement: List Courses

El sistema DEBE mostrar una lista de todas las carreras registradas.

#### Scenario: Ver lista de carreras

- GIVEN existen 2 carreras: "Ingeniería" y "Arquitectura"
- WHEN el usuario abre la pantalla de carreras
- THEN ve ambas carreras con su nombre y color
- AND puede seleccionar una como activa

### Requirement: Select Active Course

El sistema DEBE permitir seleccionar una carrera como activa para filtrar la vista del calendario.

#### Scenario: Cambiar carrera activa

- GIVEN el usuario está en el calendario semanal
- WHEN selecciona "Arquitectura" como carrera activa
- THEN solo muestra materias y tareas de Arquitectura
- AND la selección persiste entre sesiones