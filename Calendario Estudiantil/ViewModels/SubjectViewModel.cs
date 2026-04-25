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
    private readonly Services.ICourseService _courseService;

    [ObservableProperty]
    private ObservableCollection<Subject> _subjects = new();

    [ObservableProperty]
    private Subject? _selectedSubject;

    [ObservableProperty]
    private bool _isLoading;

    public SubjectViewModel(Services.ISubjectService subjectService, Services.ICourseService courseService)
    {
        _subjectService = subjectService;
        _courseService = courseService;
    }

    public async Task LoadSubjectsAsync()
    {
        IsLoading = true;
        try
        {
            var active = await _courseService.GetActiveAsync();
            if (active == null)
            {
                Subjects.Clear();
                return;
            }

            var list = await _subjectService.GetByCourseAsync(active.Id);
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

        var active = await _courseService.GetActiveAsync();
        if (active == null) return;

        var subject = new Subject
        {
            CourseId = active.Id,
            Name = name.Trim(),
            ColorHex = colorHex
        };

        await _subjectService.CreateAsync(subject);
        await LoadSubjectsAsync();
    }

    /// <summary>
    /// Crea materia sin carrera (modo simple).
    /// </summary>
    public async Task CreateSubjectNoCourseAsync(string name, string colorHex = "#512BD4")
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var subject = new Subject
        {
            CourseId = 0, // Sin carrera
            Name = name.Trim(),
            ColorHex = colorHex
        };

        await _subjectService.CreateAsync(subject);
        await LoadSubjectsNoCourseAsync();
    }

    public async Task LoadSubjectsNoCourseAsync()
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

    public async Task DeleteSubjectAsync(Subject? subject)
    {
        if (subject == null) return;
        await _subjectService.DeleteAsync(subject);
        await LoadSubjectsAsync();
    }
}