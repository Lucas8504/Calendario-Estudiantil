using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de carreras.
/// </summary>
public class CourseService : ICourseService
{
    private readonly IDatabaseService _databaseService;

    public CourseService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task<List<Course>> GetAllAsync()
        => _databaseService.GetAllCoursesAsync();

    public Task<Course> GetByIdAsync(int id)
        => _databaseService.GetCourseByIdAsync(id);

    public async Task<Course?> GetActiveAsync()
    {
        var activeId = await _databaseService.GetActiveCourseIdAsync();
        if (activeId == -1) return null;
        return await _databaseService.GetCourseByIdAsync(activeId);
    }

    public Task<int> CreateAsync(Course course)
        => _databaseService.InsertCourseAsync(course);

    public Task<int> UpdateAsync(Course course)
        => _databaseService.UpdateCourseAsync(course);

    public Task<int> DeleteAsync(Course course)
        => _databaseService.DeleteCourseAsync(course);

    public Task SetActiveAsync(int courseId)
        => _databaseService.SetActiveCourseIdAsync(courseId);
}