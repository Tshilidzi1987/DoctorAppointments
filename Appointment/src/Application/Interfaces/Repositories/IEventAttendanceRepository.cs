using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEventAttendanceRepository
{
    Task<IEnumerable<EventAttendance?>> GetByIdAsync(long eventId, CancellationToken cancellationToken = default);
    Task AddAsync(EventAttendance eventAttendanceDetails, CancellationToken cancellationToken = default);
    Task DeleteAsync(long eventAttendanceId, CancellationToken cancellationToken = default);
}