using Application.Dtos;

namespace Application.Interfaces.Services;
public interface IAppointmentService
{
    Task<AppointmentDetailsDto?> GetByIdAsync(long eventAttendanceId, CancellationToken cancellationToken);
    Task<long> AddAsync(BookAppointmentDto appointmentDto, CancellationToken cancellationToken);
    Task<long> UpdateAsync(UpdateAppointmentDto appointmentDto, CancellationToken cancellationToken);
    Task DeleteAsync(long eventId, CancellationToken cancellationToken);
}