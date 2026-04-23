# Schedule Grid Specification

## Purpose

Vista principal del calendario semanal con grid de franjas horarias que muestra las materias según su horario asignado.

## Requirements

### Requirement: Display Weekly Grid

El sistema DEBE mostrar un grid semanal con días como columnas y franjas horarias como filas.

#### Scenario: Ver grid semanal

- GIVEN existen materias con horarios registrados
- WHEN el usuario abre la vista del calendario
- THEN ve columns para Lun, Mar, Mie, Jue, Vie, Sáb, Dom
- AND filas desde 06:00 hasta 22:00 (o configurable)
- AND las materias aparecen en las celdas correspondientes a su horario

#### Scenario: Grid vacío sin horarios

- GIVEN no existen horarios de materias
- WHEN el usuario abre el calendario
- THEN ve el grid vacío con mensaje "Agrega materias y horarios"

### Requirement: Display Subject Blocks

El sistema DEBE mostrar bloques visuales de materia en el grid con color y nombre.

#### Scenario: Bloque de materia en grid

- GIVEN existe materia "Cálculo I" con color azul, horario Lun 08:00-10:00
- WHEN se visualiza el grid del lunes
- THEN muestra un bloque azul con texto "Cálculo I" de 08:00 a 10:00
- AND al tocar el bloque se abre detalle de la materia

#### Scenario: Materia que dura más de una hora

- GIVEN materia tiene horario 08:00-11:00
- WHEN se renderiza el grid
- THEN el bloque ocupa 3 filas (una por hora)
- AND mantiene el color y nombre uniforme

### Requirement: Navigate Between Weeks

El sistema DEBE permitir navegar entre semanas del calendario.

#### Scenario: Cambiar semana

- GIVEN está en semana del 15-21 de abril
- WHEN usuario toca "siguiente semana"
- THEN el grid muestra 22-28 de abril
- AND las materias aparecen según sus horarios recurrentes

### Requirement: Week Navigation Controls

El sistema DEBE mostrar controles para navegar semanas y volver a la actual.

#### Scenario: Botón "hoy"

- GIVEN el usuario navegó a otra semana
- WHEN toca botón "Hoy"
- THEN vuelve a la semana actual
- AND el grid muestra la fecha actual resaltada

### Requirement: Current Time Indicator

El sistema DEBE mostrar una línea horizontal indicando la hora actual.

#### Scenario: Indicador de hora actual

- GIVEN son las 09:30
- WHEN el usuario ve el grid
- THEN hay una línea horizontal en la fila de 09:00
- AND está posicionada al 50% de la celda (30 min)
- AND es visible solo en el día actual

### Requirement: Tap Subject Block

El sistema DEBE permitir tocar un bloque de materia para ver opciones.

#### Scenario: Tocar bloque de materia

- GIVEN hay materia "Física" en el grid
- WHEN el usuario toca el bloque
- THEN se muestra popup/modal con opciones: "Ver tareas", "Agregar tarea", "Editar materia"

### Requirement: Filter by Active Course

El sistema DEBE filtrar las materias mostradas según la carrera activa.

#### Scenario: Filtrar por carrera

- GIVEN carrera activa es "Ingeniería" con 3 materias
- AND existe materia "Historia" de carrera "Arquitectura"
- WHEN se muestra el grid
- THEN solo aparecen las 3 materias de Ingeniería