using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEventsRepository
{
    Task<Events?> GetByIdWithAttendancesAsync(long eventId, CancellationToken cancellationToken = default);
    Task<long> AddAsync(Events eventDetails, CancellationToken cancellationToken = default);
    Task UpdateAsync(Events eventDetails, CancellationToken cancellationToken = default);
    Task DeleteAsync(long eventId, CancellationToken cancellationToken = default);
}