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
        Attendees? attendeeDetails = await _dbContext.AppointmentAttendee.FindAsync([attendeeId, cancellationToken], cancellationToken: cancellationToken);
        if (attendeeDetails is not null)
        {
            _dbContext.AppointmentAttendee.Remove(attendeeDetails);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    public async Task<IEnumerable<Attendees?>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentAttendee
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
    public async Task<IEnumerable<Attendees?>> GetByIdAsync(long attendeeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentAttendee
            .AsNoTracking()
            .Where(p => p.AttendeeId == attendeeId)
            .ToListAsync(cancellationToken)            
            .ConfigureAwait(false);
    }
    public async Task UpdateAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default)
    {
        _dbContext.AppointmentAttendee.Update(attendeeDetails);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}