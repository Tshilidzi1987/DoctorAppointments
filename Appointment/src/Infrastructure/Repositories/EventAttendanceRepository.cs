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
        EventAttendance attendee = new() { EventAttendanceId = eventAttendanceId };
        _dbContext.Entry(attendee).State = EntityState.Deleted;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the row does not exist (concurrent delete), treat as success (no-op).
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