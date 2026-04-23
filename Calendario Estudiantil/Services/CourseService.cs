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

    public Task<Course?> GetByIdAsync(int id)
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

    public async Task<int> DeleteAsync(Course course)
    {
        var subjects = await DatabaseService.GetSubjectsByCourseAsync(course.Id);
        foreach (var subject in subjects)
        {
            await DatabaseService.DeleteSchedulesBySubjectAsync(subject.Id);
            await DatabaseService.DeleteTasksBySubjectAsync(subject.Id);
        }

        await DatabaseService.DeleteSubjectsByCourseAsync(course.Id);

        var activeId = await DatabaseService.GetActiveCourseIdAsync();
        if (activeId == course.Id)
            await DatabaseService.SetActiveCourseIdAsync(-1);

        return await DatabaseService.DeleteCourseAsync(course);
    }

    public Task SetActiveAsync(int courseId)
        => DatabaseService.SetActiveCourseIdAsync(courseId);
}