using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Helpers;

/// <summary>
/// Utilidades compartidas para días de la semana.
/// Centraliza todos los mapeos y nombres de día que estaban duplicados.
/// </summary>
public static class DayHelper
{
    /// <summary>
    /// Nombres completos: "Lunes", "Martes", ...
    /// </summary>
    public static readonly string[] FullNames =
        ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

    /// <summary>
    /// Nombres abreviados: "Lun", "Mar", "Mié", ...
    /// </summary>
    public static readonly string[] ShortNames =
        ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom"];

    /// <summary>
    /// Mapeo: nombre completo → DiaSemana.
    /// </summary>
    public static readonly Dictionary<string, DiaSemana> DiaMap = new()
    {
        ["Lunes"] = DiaSemana.Lunes,
        ["Martes"] = DiaSemana.Martes,
        ["Miércoles"] = DiaSemana.Miércoles,
        ["Jueves"] = DiaSemana.Jueves,
        ["Viernes"] = DiaSemana.Viernes,
        ["Sábado"] = DiaSemana.Sábado,
        ["Domingo"] = DiaSemana.Domingo
    };

    /// <summary>
    /// Mapeo: abreviación → DiaSemana.
    /// </summary>
    public static readonly Dictionary<string, DiaSemana> ShortDiaMap = new()
    {
        ["Lun"] = DiaSemana.Lunes,
        ["Mar"] = DiaSemana.Martes,
        ["Mié"] = DiaSemana.Miércoles,
        ["Jue"] = DiaSemana.Jueves,
        ["Vie"] = DiaSemana.Viernes,
        ["Sáb"] = DiaSemana.Sábado,
        ["Dom"] = DiaSemana.Domingo
    };

    /// <summary>
    /// Mapeo: DiaSemana → nombre completo.
    /// </summary>
    public static readonly Dictionary<DiaSemana, string> ReverseDiaMap = new()
    {
        [DiaSemana.Lunes] = "Lunes",
        [DiaSemana.Martes] = "Martes",
        [DiaSemana.Miércoles] = "Miércoles",
        [DiaSemana.Jueves] = "Jueves",
        [DiaSemana.Viernes] = "Viernes",
        [DiaSemana.Sábado] = "Sábado",
        [DiaSemana.Domingo] = "Domingo"
    };

    /// <summary>
    /// Mapeo: nombre completo → abreviación (para botones de día).
    /// </summary>
    public static readonly Dictionary<string, string> FullToShortMap = new()
    {
        ["Lunes"] = "Lun",
        ["Martes"] = "Mar",
        ["Miércoles"] = "Mié",
        ["Jueves"] = "Jue",
        ["Viernes"] = "Vie",
        ["Sábado"] = "Sáb",
        ["Domingo"] = "Dom"
    };

    /// <summary>
    /// Obtiene el nombre completo del día (ej: "Lunes").
    /// </summary>
    public static string GetDayName(DiaSemana day) => day switch
    {
        DiaSemana.Lunes => "Lunes",
        DiaSemana.Martes => "Martes",
        DiaSemana.Miércoles => "Miércoles",
        DiaSemana.Jueves => "Jueves",
        DiaSemana.Viernes => "Viernes",
        DiaSemana.Sábado => "Sábado",
        DiaSemana.Domingo => "Domingo",
        _ => ""
    };

    /// <summary>
    /// Obtiene la abreviación del día (ej: "Lun").
    /// </summary>
    public static string GetDayShortName(DiaSemana day) => day switch
    {
        DiaSemana.Lunes => "Lun",
        DiaSemana.Martes => "Mar",
        DiaSemana.Miércoles => "Mié",
        DiaSemana.Jueves => "Jue",
        DiaSemana.Viernes => "Vie",
        DiaSemana.Sábado => "Sáb",
        DiaSemana.Domingo => "Dom",
        _ => ""
    };

    /// <summary>
    /// Convierte System.DayOfWeek a DiaSemana.
    /// </summary>
    public static DiaSemana FromDayOfWeek(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Monday => DiaSemana.Lunes,
        DayOfWeek.Tuesday => DiaSemana.Martes,
        DayOfWeek.Wednesday => DiaSemana.Miércoles,
        DayOfWeek.Thursday => DiaSemana.Jueves,
        DayOfWeek.Friday => DiaSemana.Viernes,
        DayOfWeek.Saturday => DiaSemana.Sábado,
        DayOfWeek.Sunday => DiaSemana.Domingo,
        _ => DiaSemana.Lunes
    };
}
