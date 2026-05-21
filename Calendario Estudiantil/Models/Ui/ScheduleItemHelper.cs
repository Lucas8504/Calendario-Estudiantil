using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Models.Ui;

/// <summary>
/// Helper para combinar Subject + Schedule (usado internamente).
/// </summary>
public class ScheduleItemHelper
{
    public Subject Subject { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
}
