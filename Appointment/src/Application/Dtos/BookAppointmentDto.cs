namespace Application.Dtos;

public class BookAppointmentDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }    
    public IEnumerable<BookAppointmentAttendeeDto>? AppointmentAttendees { get; set; }
}