using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para gestión de carreras.
/// </summary>
public partial class CoursesViewModel : ObservableObject
{
    private readonly Services.ICourseService _courseService;

    [ObservableProperty]
    private ObservableCollection<Course> _courses = new();

    [ObservableProperty]
    private Course? _selectedCourse;

    [ObservableProperty]
    private bool _isLoading;

    public CoursesViewModel(Services.ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task LoadCoursesAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _courseService.GetAllAsync();
            Courses = new ObservableCollection<Course>(list);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CreateCourseAsync(string name, string colorHex = "#512BD4")
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var course = new Course { Name = name.Trim(), ColorHex = colorHex };
        await _courseService.CreateAsync(course);
        await LoadCoursesAsync();
    }

    public async Task DeleteCourseAsync(Course? course)
    {
        if (course == null) return;
        await _courseService.DeleteAsync(course);
        await LoadCoursesAsync();
    }

    public async Task SelectActiveAsync(Course? course)
    {
        if (course == null) return;
        await _courseService.SetActiveAsync(course.Id);
        SelectedCourse = course;
    }
}