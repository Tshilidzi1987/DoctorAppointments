namespace Domain.Entities;

public class EventAttendance
{
    public long EventAttendanceId { get; set; }
    public long EventId { get; set; }
    public long AttendeeId { get; set; }    

    // Navigation properties (many-to-one)
    public Events? Event { get; set; }
    public Attendees? Attendee { get; set; }
}