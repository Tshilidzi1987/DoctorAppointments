namespace Application.Dtos;

public class AppointmentDetailsDto
{
    public long EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public IEnumerable<AppointmentAttendeeDetailsDto>? AppointmentAttendees { get; set; }
}