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
        EventAttendance? eventDetails = await _dbContext.AppointmentEventAttendance.FindAsync([eventAttendanceId, cancellationToken], cancellationToken: cancellationToken);

        if (eventDetails is not null)
        {
            _dbContext.AppointmentEventAttendance.Remove(eventDetails);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<EventAttendance?>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentEventAttendance
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EventAttendance?> GetByIdAsync(long eventAttendanceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentEventAttendance
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.EventAttendanceId == eventAttendanceId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(EventAttendance eventDetails, CancellationToken cancellationToken = default)
    {
        _dbContext.AppointmentEventAttendance.Update(eventDetails);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
