using System.Collections.ObjectModel;
using System.Diagnostics;
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



    public SubjectViewModel(Services.ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    public async Task LoadSubjectsAsync()
    {
        IsLoading = true;
        var sw = Stopwatch.StartNew();
        Debug.WriteLine($"[TIMING] SubjectVM.LoadSubjectsAsync START");

        try
        {
            var list = await _subjectService.GetAllAsync();
            Debug.WriteLine($"[TIMING]   GetAllAsync: {sw.ElapsedMilliseconds}ms ({list.Count} subjects)");
            Subjects.ReplaceWith(list);
            Debug.WriteLine($"[TIMING]   ReplaceWith: {sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine($"[TIMING] SubjectVM.LoadSubjectsAsync DONE: {sw.ElapsedMilliseconds}ms");
        }
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