using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// Helper para pairing de materia con su horario.
/// Usado internamente para construir el grid de horarios.
/// </summary>
public class ScheduleItemHelper
{
    public Subject Subject { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
}