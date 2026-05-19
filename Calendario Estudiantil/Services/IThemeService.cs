namespace Calendario_Estudiantil.Services;

/// <summary>
/// Interfaz para el servicio de temas.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Obtiene o establece el tema seleccionado por el usuario.
    /// Valores: "Light", "Dark", "System"
    /// </summary>
    string SelectedTheme { get; set; }

    /// <summary>
    /// Aplica el tema seleccionado a la aplicación.
    /// </summary>
    void ApplyTheme();

    /// <summary>
    /// Obtiene el tema actual basado en la selección y el modo del sistema.
    /// </summary>
    AppTheme GetCurrentTheme();
}
