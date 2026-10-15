using Application.Dtos;

namespace Application.Interfaces.Services;
public interface IAppointmentService
{
    Task<AppointmentDetailsDto?> GetByIdAsync(long eventAttendanceId, CancellationToken cancellationToken);
    Task AddAsync(BookAppointmentDto appointmentDto, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateAppointmentDto appointmentDto, CancellationToken cancellationToken);
    Task DeleteAsync(long eventAttendanceId, CancellationToken cancellationToken);
}