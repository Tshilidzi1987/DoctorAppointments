namespace Application.Dtos;

public class UpdateAppointmentDto
{
    public long EventId { get; set; }
    public long EventAttendanceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAttending { get; set; }
    public IEnumerable<UpdateAppointmentAttendeeDto>? UpdateAppointmentAttendees { get; set; }
}