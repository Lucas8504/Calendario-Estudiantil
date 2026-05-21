using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Calendario_Estudiantil.Helpers;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para crear/editar materias.
/// </summary>
public partial class SubjectViewModel : ObservableObject
{
    private readonly Services.ISubjectService _subjectService;

    [ObservableProperty]
    private ObservableCollection<Subject> _subjects = new();

    [ObservableProperty]
    private Subject? _selectedSubject;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Todas las materias sin filtrar (para búsqueda)
    private List<Subject> _allSubjects = new();

    public SubjectViewModel(Services.ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    public async Task LoadSubjectsAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _subjectService.GetAllAsync();
            _allSubjects = list;
            ApplySearchFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Aplica el filtro de búsqueda sobre las materias.
    /// </summary>
    private void ApplySearchFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allSubjects
            : _allSubjects.Where(s => 
                s.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
              .ToList();

        Subjects.ReplaceWith(filtered);
    }

    /// <summary>
    /// Busca materias por nombre.
    /// </summary>
    public void Search(string query)
    {
        SearchQuery = query;
        ApplySearchFilter();
    }

    /// <summary>
    /// Limpia el filtro de búsqueda.
    /// </summary>
    public void ClearSearch()
    {
        SearchQuery = string.Empty;
        ApplySearchFilter();
    }

    public async Task CreateSubjectAsync(string name, string colorHex = "#512BD4")
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var subject = new Subject
        {
            Name = name.Trim(),
            ColorHex = ColorValidation.Validate(colorHex)
        };

        await _subjectService.CreateAsync(subject);
        await LoadSubjectsAsync();
    }

    public async Task DeleteSubjectAsync(Subject? subject)
    {
        if (subject == null) return;
        await _subjectService.DeleteAsync(subject);
        await LoadSubjectsAsync();
    }
}