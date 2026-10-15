using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AttendeesRepository(AppDbContext dbContext) : IAttendeesRepository
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public async Task<long> AddAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default)
    {
        await _dbContext.AppointmentAttendee.AddAsync(attendeeDetails, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return attendeeDetails.AttendeeId;
    }
  
    public async Task DeleteAsync(long attendeeId, CancellationToken cancellationToken = default)
    {
        Attendees attendee = new() { AttendeeId = attendeeId };
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

    public async Task UpdateAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default)
    {
        _dbContext.AppointmentAttendee.Update(attendeeDetails);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}