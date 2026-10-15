using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEventAttendanceRepository
{
    Task<EventAttendance?> GetByIdAsync(long eventAttendanceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventAttendance?>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(EventAttendance eventAttendanceDetails, CancellationToken cancellationToken = default);
    Task UpdateAsync(EventAttendance eventAttendanceDetails, CancellationToken cancellationToken = default);
    Task DeleteAsync(long eventAttendanceId, CancellationToken cancellationToken = default);
}
