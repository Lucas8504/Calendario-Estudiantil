using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Helpers;

/// <summary>
/// Helper compartido para animar cards con efecto de deslizamiento desde abajo.
/// Elimina ~30 líneas de código duplicado entre SchedulePage y TasksPage.
/// </summary>
public static class CardAnimationHelper
{
    /// <summary>
    /// Anima una lista de cards con un efecto de cascada (fade + slide).
    /// </summary>
    /// <param name="cards">Lista de Frames a animar</param>
    /// <param name="staggerDelay">Delay en ms entre cada card (default: 80)</param>
    /// <param name="duration">Duración de cada animación en ms (default: 300)</param>
    /// <param name="slideDistance">Píxeles a deslizar (default: 50)</param>
    public static async Task AnimateCardsAsync(
        List<Frame> cards,
        int staggerDelay = 80,
        int duration = 300,
        double slideDistance = 50)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];

            // Estado inicial: invisible y desplazado hacia abajo
            card.Opacity = 0;
            card.TranslationY = slideDistance;

            // Pequeño delay para efecto cascada
            await Task.Delay(staggerDelay);

            // Animar: aparecer y deslizar hacia arriba
            _ = card.TranslateTo(0, 0, (uint)duration, Easing.CubicOut);
            _ = card.FadeTo(1, (uint)duration, Easing.CubicOut);
        }
    }
}
