using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de materias con caché.
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly IDatabaseService _databaseService;
    private List<Subject>? _subjectsCache;

    public SubjectService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        if (_subjectsCache != null)
            return _subjectsCache;

        _subjectsCache = await _databaseService.GetAllSubjectsAsync();
        return _subjectsCache;
    }

    public async Task<Subject> GetByIdAsync(int id)
    {
        // Check cache first
        if (_subjectsCache != null)
        {
            var cached = _subjectsCache.FirstOrDefault(s => s.Id == id);
            if (cached != null)
                return cached;
        }

        return await _databaseService.GetSubjectByIdAsync(id);
    }

    public async Task<int> CreateAsync(Subject subject)
    {
        var result = await _databaseService.InsertSubjectAsync(subject);
        InvalidateCache();
        return result;
    }

    public async Task<int> UpdateAsync(Subject subject)
    {
        var result = await _databaseService.UpdateSubjectAsync(subject);
        InvalidateCache();
        return result;
    }

    public async Task<int> DeleteAsync(Subject subject)
    {
        var result = await _databaseService.DeleteSubjectAsync(subject);
        InvalidateCache();
        return result;
    }

    public Task<List<Schedule>> GetSchedulesAsync(int subjectId)
        => _databaseService.GetSchedulesBySubjectAsync(subjectId);

    public Task<int> AddScheduleAsync(Schedule schedule)
        => _databaseService.InsertScheduleAsync(schedule);

    public Task<int> UpdateScheduleAsync(Schedule schedule)
        => _databaseService.UpdateScheduleAsync(schedule);

    public Task<int> DeleteScheduleAsync(Schedule schedule)
        => _databaseService.DeleteScheduleAsync(schedule);

    /// <summary>
    /// Loads all schedules for multiple subjects in a single query (prevents N+1).
    /// </summary>
    public Task<Dictionary<int, List<Schedule>>> GetSchedulesForSubjectsAsync(IEnumerable<int> subjectIds)
        => _databaseService.GetSchedulesForSubjectsAsync(subjectIds);

    private void InvalidateCache()
    {
        _subjectsCache = null;
    }
}
