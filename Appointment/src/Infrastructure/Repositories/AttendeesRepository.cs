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
        // Load the tracked entity instance first. This avoids issues with the InMemory provider
        // and guarantees EF Core will remove the correct tracked entity.
        Attendees? existing = await _dbContext.AppointmentAttendee
            .FindAsync([attendeeId], cancellationToken);

        if (existing is null)
        {
            // If the row does not exist, treat as success (no-op).
            return;
        }

        _dbContext.AppointmentAttendee.Remove(existing);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the row does not exist at save time (concurrent delete), treat as success (no-op).
        }
    }

    public async Task UpdateAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendeeDetails);

        // Load the existing/tracked entity from the same DbContext to avoid EF Core tracking conflicts.
        Attendees? existing = await _dbContext.AppointmentAttendee
            .FindAsync([attendeeDetails.AttendeeId], cancellationToken);

        if (existing is null)
        {
            // If the entity does not exist in the store, treat as no-op (tests expect no exception here).
            return;
        }

        // Copy changed values onto the tracked entity instance.
        existing.Name = attendeeDetails.Name;
        existing.Email = attendeeDetails.Email;
        existing.Address = attendeeDetails.Address;
        existing.IsAttending = attendeeDetails.IsAttending;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Gracefully handle concurrency like the Delete implementation does (no-op).
        }
    }
}