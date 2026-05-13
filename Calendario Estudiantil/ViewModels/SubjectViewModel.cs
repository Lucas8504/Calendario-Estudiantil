using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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
            Subjects = new ObservableCollection<Subject>(list);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CreateSubjectAsync(string name, string colorHex = "#512BD4")
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var subject = new Subject
        {
            Name = name.Trim(),
            ColorHex = colorHex
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