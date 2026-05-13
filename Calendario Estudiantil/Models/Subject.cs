using SQLite;

namespace Calendario_Estudiantil.Models;

/// <summary>
/// Materia (asignatura) asociada a una carrera.
/// </summary>
public class Subject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color en formato hexadecimal (#RRGGBB).
    /// </summary>
    public string ColorHex { get; set; } = "#512BD4";
}