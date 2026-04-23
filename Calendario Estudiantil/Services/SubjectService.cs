using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de materias.
/// </summary>
public class SubjectService : ISubjectService
{
    public Task<List<Subject>> GetAllAsync()
        => DatabaseService.GetAllSubjectsAsync();

    public Task<List<Subject>> GetByCourseAsync(int courseId)
        => DatabaseService.GetSubjectsByCourseAsync(courseId);

    public Task<Subject?> GetByIdAsync(int id)
        => DatabaseService.GetSubjectByIdAsync(id);

    public Task<int> CreateAsync(Subject subject)
        => DatabaseService.InsertSubjectAsync(subject);

    public Task<int> UpdateAsync(Subject subject)
        => DatabaseService.UpdateSubjectAsync(subject);

    public async Task<int> DeleteAsync(Subject subject)
    {
        await DatabaseService.DeleteSchedulesBySubjectAsync(subject.Id);
        await DatabaseService.DeleteTasksBySubjectAsync(subject.Id);
        return await DatabaseService.DeleteSubjectAsync(subject);
    }

    public Task<List<Schedule>> GetSchedulesAsync(int subjectId)
        => DatabaseService.GetSchedulesBySubjectAsync(subjectId);

    public Task<int> AddScheduleAsync(Schedule schedule)
        => DatabaseService.InsertScheduleAsync(schedule);

    public Task<int> DeleteScheduleAsync(Schedule schedule)
        => DatabaseService.DeleteScheduleAsync(schedule);
}