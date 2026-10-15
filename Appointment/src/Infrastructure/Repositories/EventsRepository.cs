
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventsRepository(AppDbContext dbContext) : IEventsRepository
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public async Task<long> AddAsync(Events eventDetails, CancellationToken cancellationToken = default)
    {
        await _dbContext.AppointmentEvent.AddAsync(eventDetails, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return eventDetails.EventId;
    }

    public async Task DeleteAsync(long eventId, CancellationToken cancellationToken = default)
    {
        Events? eventDetails = await _dbContext.AppointmentEvent.FindAsync([eventId, cancellationToken], cancellationToken: cancellationToken);

        if (eventDetails is not null)
        {
            _dbContext.AppointmentEvent.Remove(eventDetails);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    //load event together with attendances and attendee details
    public async Task<Events?> GetByIdWithAttendancesAsync(long eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentEvent
            .AsNoTracking()
            .Include(e => e.EventAttendances)
            .ThenInclude(ea => ea.Attendee)
            .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(Events eventDetails, CancellationToken cancellationToken = default)
    {
        _dbContext.AppointmentEvent.Update(eventDetails);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}