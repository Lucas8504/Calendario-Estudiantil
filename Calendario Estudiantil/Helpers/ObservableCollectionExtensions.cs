using System.Collections.ObjectModel;

namespace Calendario_Estudiantil.Helpers;

/// <summary>
/// Extensiones para ObservableCollection que permiten mutación en lugar de reemplazo.
/// Esto evita re-render completo del UI cuando se filtran listas.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Reemplaza el contenido de la colección sin crear una nueva instancia.
    /// Usa Clear + AddRange para trigger eficiente del UI.
    /// </summary>
    public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
