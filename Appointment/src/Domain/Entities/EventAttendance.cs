namespace Domain.Entities;

public class EventAttendance
{
    public long EventAttendanceId { get; set; }
    public long EventId { get; set; }
    public long AttendeeId { get; set; }
    public bool IsAttending { get; set; }
}