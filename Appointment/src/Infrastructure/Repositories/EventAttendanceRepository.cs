using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventAttendanceRepository(AppDbContext dbContext) : IEventAttendanceRepository
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public async Task AddAsync(EventAttendance eventDetails, CancellationToken cancellationToken = default)
    {
        await _dbContext.AppointmentEventAttendance.AddAsync(eventDetails, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long eventAttendanceId, CancellationToken cancellationToken = default)
    {
        // Load the tracked entity instance first. This avoids issues with the InMemory provider
        // and guarantees EF Core will remove the correct tracked entity.
        EventAttendance? existing = await _dbContext.AppointmentEventAttendance
            .FindAsync([eventAttendanceId], cancellationToken);

        if (existing is null)
        {
            // If the row does not exist, treat as success (no-op).
            return;
        }

        _dbContext.AppointmentEventAttendance.Remove(existing);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the row does not exist at save time (concurrent delete), treat as success (no-op).
        }
    }

    public async Task<IEnumerable<EventAttendance?>> GetByIdAsync(long eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentEventAttendance
            .AsNoTracking()
            .Where(p => p.EventId == eventId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}