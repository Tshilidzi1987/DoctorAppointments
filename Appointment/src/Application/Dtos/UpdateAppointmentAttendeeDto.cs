namespace Application.Dtos;

public class UpdateAppointmentAttendeeDto
{
    public long AttendeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsAttending { get; set; }
}
