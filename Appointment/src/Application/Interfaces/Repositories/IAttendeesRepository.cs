
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IAttendeesRepository
{
    Task<long> AddAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default);
    Task UpdateAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default);
    Task DeleteAsync(long attendeeId, CancellationToken cancellationToken = default);
}
