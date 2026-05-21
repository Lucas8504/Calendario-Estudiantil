using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Helpers;

/// <summary>
/// Helper compartido para manejar la barra de búsqueda en las páginas.
/// Elimina ~60 líneas de código duplicado entre SchedulePage, TasksPage y SubjectsPage.
/// </summary>
public static class SearchHelper
{
    /// <summary>
    /// Muestra u oculta la barra de búsqueda y enfoca el entry si se muestra.
    /// </summary>
    public static void ToggleSearch(Frame searchBarFrame, Entry searchEntry)
    {
        searchBarFrame.IsVisible = !searchBarFrame.IsVisible;
        if (searchBarFrame.IsVisible)
        {
            searchEntry.Focus();
        }
    }

    /// <summary>
    /// Limpia la búsqueda: resetea el query, el texto del entry y oculta la barra.
    /// </summary>
    public static void ClearSearch<TViewModel>(
        TViewModel viewModel,
        Action<TViewModel, string> clearAction,
        Entry searchEntry,
        Frame searchBarFrame)
        where TViewModel : class
    {
        clearAction(viewModel, string.Empty);
        searchEntry.Text = string.Empty;
        searchBarFrame.IsVisible = false;
    }

    /// <summary>
    /// Actualiza el query de búsqueda en el ViewModel.
    /// </summary>
    public static void UpdateSearch<TViewModel>(
        TViewModel viewModel,
        Action<TViewModel, string> searchAction,
        string query)
        where TViewModel : class
    {
        searchAction(viewModel, query);
    }
}
