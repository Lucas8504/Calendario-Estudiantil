using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de carreras.
/// </summary>
public class CourseService : ICourseService
{
    public Task<List<Course>> GetAllAsync()
        => DatabaseService.GetAllCoursesAsync();

    public Task<Course> GetByIdAsync(int id)
        => DatabaseService.GetCourseByIdAsync(id);

    public async Task<Course?> GetActiveAsync()
    {
        var activeId = await DatabaseService.GetActiveCourseIdAsync();
        if (activeId == -1) return null;
        return await DatabaseService.GetCourseByIdAsync(activeId);
    }

    public Task<int> CreateAsync(Course course)
        => DatabaseService.InsertCourseAsync(course);

    public Task<int> UpdateAsync(Course course)
        => DatabaseService.UpdateCourseAsync(course);

    public Task<int> DeleteAsync(Course course)
        => DatabaseService.DeleteCourseAsync(course);

    public Task SetActiveAsync(int courseId)
        => DatabaseService.SetActiveCourseIdAsync(courseId);
}