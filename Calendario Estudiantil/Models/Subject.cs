using SQLite;

namespace Calendario_Estudiantil.Models;

/// <summary>
/// Materia (asignatura) asociada a una carrera.
/// </summary>
public class Subject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color en formato hexadecimal (#RRGGBB).
    /// </summary>
    [MaxLength(7)]
    public string ColorHex { get; set; } = "#512BD4";
}