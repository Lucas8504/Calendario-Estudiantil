using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Servicio de materias.
/// </summary>
public interface ISubjectService
{
    Task<List<Subject>> GetAllAsync();
    Task<Subject> GetByIdAsync(int id);
    Task<int> CreateAsync(Subject subject);
    Task<int> UpdateAsync(Subject subject);
    Task<int> DeleteAsync(Subject subject);
    Task<List<Schedule>> GetSchedulesAsync(int subjectId);
    Task<int> AddScheduleAsync(Schedule schedule);
    Task<int> UpdateScheduleAsync(Schedule schedule);
    Task<int> DeleteScheduleAsync(Schedule schedule);
}