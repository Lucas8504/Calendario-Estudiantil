using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de materias.
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly IDatabaseService _databaseService;

    public SubjectService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task<List<Subject>> GetAllAsync()
        => _databaseService.GetAllSubjectsAsync();

    public Task<List<Subject>> GetByCourseAsync(int courseId)
        => _databaseService.GetSubjectsByCourseAsync(courseId);

    public Task<Subject> GetByIdAsync(int id)
        => _databaseService.GetSubjectByIdAsync(id);

    public Task<int> CreateAsync(Subject subject)
        => _databaseService.InsertSubjectAsync(subject);

    public Task<int> UpdateAsync(Subject subject)
        => _databaseService.UpdateSubjectAsync(subject);

    public Task<int> DeleteAsync(Subject subject)
        => _databaseService.DeleteSubjectAsync(subject);

    public Task<List<Schedule>> GetSchedulesAsync(int subjectId)
        => _databaseService.GetSchedulesBySubjectAsync(subjectId);

    public Task<int> AddScheduleAsync(Schedule schedule)
        => _databaseService.InsertScheduleAsync(schedule);

    public Task<int> DeleteScheduleAsync(Schedule schedule)
        => _databaseService.DeleteScheduleAsync(schedule);

    /// <summary>
    /// Loads all schedules for multiple subjects in a single query (prevents N+1).
    /// </summary>
    public Task<Dictionary<int, List<Schedule>>> GetSchedulesForSubjectsAsync(IEnumerable<int> subjectIds)
        => _databaseService.GetSchedulesForSubjectsAsync(subjectIds);
}