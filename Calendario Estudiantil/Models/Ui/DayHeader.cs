namespace Calendario_Estudiantil.Models.Ui;

/// <summary>
/// Representa una columna (día) de la semana en el grid.
/// </summary>
public class DayHeader
{
    public int DayIndex { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
}
