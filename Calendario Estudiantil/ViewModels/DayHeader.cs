namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// Header de un día en el grid de horarios.
/// </summary>
public class DayHeader
{
    /// <summary>
    /// Índice del día (0=Lunes, 4=Viernes, etc.)
    /// </summary>
    public int DayIndex { get; set; }

    /// <summary>
    /// Nombre abreviado del día (Lun, Mar, etc.)
    /// </summary>
    public string DayName { get; set; } = string.Empty;

    /// <summary>
    /// Número del día en el mes.
    /// </summary>
    public int DayNumber { get; set; }

    /// <summary>
    /// Indica si este día es el día actual.
    /// </summary>
    public bool IsToday { get; set; }
}