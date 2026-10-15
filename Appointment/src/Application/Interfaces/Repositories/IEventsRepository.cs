using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEventsRepository
{
    Task<Events?> GetByIdAsync(long eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Events?>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Events eventDetails, CancellationToken cancellationToken = default);
    Task UpdateAsync(Events eventDetails, CancellationToken cancellationToken = default);
    Task DeleteAsync(long eventId, CancellationToken cancellationToken = default);
}