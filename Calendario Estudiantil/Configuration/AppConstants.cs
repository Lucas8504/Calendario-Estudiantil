namespace Calendario_Estudiantil.Configuration;

/// <summary>
/// Constantes de la aplicación.
/// Centraliza todos los valores hardcodeados para fácil configuración.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Rango de horas del grid de horarios.
    /// </summary>
    public static class ScheduleGrid
    {
        /// <summary>
        /// Hora mínima del día (7 = 7:00 AM).
        /// </summary>
        public const int MinHour = 7;

        /// <summary>
        /// Hora máxima del día (22 = 10:00 PM).
        /// </summary>
        public const int MaxHour = 22;

        /// <summary>
        /// Cantidad total de horas mostradas.
        /// </summary>
        public const int TotalHours = MaxHour - MinHour;

        /// <summary>
        /// Ancho de la columna de horas en pixels (15% menos que antes).
        /// </summary>
        public const int HourColumnWidth = 60;

        /// <summary>
        /// Ancho de cada columna de día en pixels.
        /// </summary>
        public const int DayColumnWidth = 90;

        /// <summary>
        /// Altura de cada fila en pixels.
        /// </summary>
        public const int RowHeight = 55;
    }

    /// <summary>
    /// Colores disponibles para materias.
    /// </summary>
    public static class SubjectColors
    {
        public const string Default = "#512BD4";
        public const string Red = "#FF5252";
        public const string Pink = "#FF4081";
        public const string Purple = "#E040FB";
        public const string Blue = "#448AFF";
        public const string Teal = "#64FFDA";
        public const string Green = "#69F0AE";
        public const string Orange = "#FFAB40";
    }
}