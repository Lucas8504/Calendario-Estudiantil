using System.Collections.ObjectModel;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// Interfaz para el ViewModel del grid de horarios.
/// Define el contrato para poder hacer testing y DI.
/// </summary>
public interface IScheduleGridViewModel
{
    DateTime CurrentWeekStart { get; }
    ObservableCollection<DayHeader> DayHeaders { get; }
    ObservableCollection<ScheduleCell> Cells { get; }
    bool IsLoading { get; }
    string WeekLabel { get; }
    int MinHour { get; }
    int MaxHour { get; }
    int TotalHours { get; }

    Task LoadGridAsync();
    Task PreviousWeekAsync();
    Task NextWeekAsync();
    Task GoToTodayAsync();
}