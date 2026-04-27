using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Servicio de carreras.
/// </summary>
public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course> GetByIdAsync(int id);
    Task<Course?> GetActiveAsync();
    Task<int> CreateAsync(Course course);
    Task<int> UpdateAsync(Course course);
    Task<int> DeleteAsync(Course course);
    Task SetActiveAsync(int courseId);
}