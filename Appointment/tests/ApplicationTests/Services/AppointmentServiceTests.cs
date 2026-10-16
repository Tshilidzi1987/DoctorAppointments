
using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ApplicationTests.Services;

public class AppointmentServiceTests
{
    private class StubEventsRepository : IEventsRepository
    {
        public Func<Events, CancellationToken, Task<long>>? AddAsyncFunc;
        public Func<long, CancellationToken, Task<Events?>>? GetByIdWithAttendancesAsyncFunc;
        public Func<Events, CancellationToken, Task>? UpdateAsyncFunc;
        public Func<long, CancellationToken, Task>? DeleteAsyncFunc;

        public Task<long> AddAsync(Events eventDetails, CancellationToken cancellationToken = default)
        {
            if (AddAsyncFunc is null)
            {
                throw new InvalidOperationException("AddAsyncFunc not configured");
            }

            return AddAsyncFunc(eventDetails, cancellationToken);
        }

        public Task<Events?> GetByIdWithAttendancesAsync(long eventId, CancellationToken cancellationToken = default)
        {
            if (GetByIdWithAttendancesAsyncFunc is null)
            {
                throw new InvalidOperationException("GetByIdWithAttendancesAsyncFunc not configured");
            }

            return GetByIdWithAttendancesAsyncFunc(eventId, cancellationToken);
        }

        public Task UpdateAsync(Events eventDetails, CancellationToken cancellationToken = default)
        {
            if (UpdateAsyncFunc is null)
            {
                throw new InvalidOperationException("UpdateAsyncFunc not configured");
            }

            return UpdateAsyncFunc(eventDetails, cancellationToken);
        }

        public Task DeleteAsync(long eventId, CancellationToken cancellationToken = default)
        {
            if (DeleteAsyncFunc is null)
            {
                throw new InvalidOperationException("DeleteAsyncFunc not configured");
            }

            return DeleteAsyncFunc(eventId, cancellationToken);
        }
    }

    private class StubAttendeesRepository : IAttendeesRepository
    {
        public Func<Attendees, CancellationToken, Task<long>>? AddAsyncFunc;
        public Func<Attendees, CancellationToken, Task>? UpdateAsyncFunc;
        public Func<long, CancellationToken, Task>? DeleteAsyncFunc;

        public Task<long> AddAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default)
        {
            if (AddAsyncFunc is null)
            {
                throw new InvalidOperationException("AddAsyncFunc not configured");
            }

            return AddAsyncFunc(attendeeDetails, cancellationToken);
        }

        public Task UpdateAsync(Attendees attendeeDetails, CancellationToken cancellationToken = default)
        {
            if (UpdateAsyncFunc is null)
            {
                throw new InvalidOperationException("UpdateAsyncFunc not configured");
            }

            return UpdateAsyncFunc(attendeeDetails, cancellationToken);
        }

        public Task DeleteAsync(long attendeeId, CancellationToken cancellationToken = default)
        {
            if (DeleteAsyncFunc is null)
            {
                throw new InvalidOperationException("DeleteAsyncFunc not configured");
            }

            return DeleteAsyncFunc(attendeeId, cancellationToken);
        }
    }

    private class StubEventAttendanceRepository : IEventAttendanceRepository
    {
        public Func<long, CancellationToken, Task<IEnumerable<EventAttendance?>>>? GetByIdAsyncFunc;
        public Func<EventAttendance, CancellationToken, Task>? AddAsyncFunc;
        public Func<long, CancellationToken, Task>? DeleteAsyncFunc;

        public Task<IEnumerable<EventAttendance?>> GetByIdAsync(long eventId, CancellationToken cancellationToken = default)
        {
            if (GetByIdAsyncFunc is null)
            {
                throw new InvalidOperationException("GetByIdAsyncFunc not configured");
            }

            return GetByIdAsyncFunc(eventId, cancellationToken);
        }

        public Task AddAsync(EventAttendance eventAttendanceDetails, CancellationToken cancellationToken = default)
        {
            if (AddAsyncFunc is null)
            {
                throw new InvalidOperationException("AddAsyncFunc not configured");
            }

            return AddAsyncFunc(eventAttendanceDetails, cancellationToken);
        }

        public Task DeleteAsync(long eventAttendanceId, CancellationToken cancellationToken = default)
        {
            if (DeleteAsyncFunc is null)
            {
                throw new InvalidOperationException("DeleteAsyncFunc not configured");
            }

            return DeleteAsyncFunc(eventAttendanceId, cancellationToken);
        }
    }

    [Fact]
    public async Task AddAsync_Should_Create_Event_And_Attendees_And_Attendances()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        List<EventAttendance> recordedAttendances = [];
        List<Attendees> recordedAttendees = [];

        long nextAttendeeId = 100L;
        eventsRepo.AddAsyncFunc = (e, ct) =>
        {
            return Task.FromResult(500L);
        };

        attendeesRepo.AddAsyncFunc = (a, ct) =>
        {
            recordedAttendees.Add(a);
            long assigned = nextAttendeeId;
            nextAttendeeId++;
            return Task.FromResult(assigned);
        };

        attendanceRepo.AddAsyncFunc = (ea, ct) =>
        {
            EventAttendance copy = new()
            {
                EventAttendanceId = ea.EventAttendanceId,
                EventId = ea.EventId,
                AttendeeId = ea.AttendeeId,
                Attendee = ea.Attendee,
                Event = ea.Event
            };
            recordedAttendances.Add(copy);
            return Task.CompletedTask;
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        BookAppointmentDto appointment = new()
        {
            Title = "Checkup",
            Description = "Annual check",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            AppointmentAttendees =
            [
                new BookAppointmentAttendeeDto { Name = "Alice", Email = "a@example.com", Address = "Addr1" },
                new BookAppointmentAttendeeDto { Name = "Bob", Email = "b@example.com", Address = "Addr2" }
            ]
        };

        long resultId = await service.AddAsync(appointment, CancellationToken.None);

        Assert.Equal(500L, resultId);
        Assert.Equal(2, recordedAttendees.Count);
        Assert.Equal(2, recordedAttendances.Count);
        List<long> attendeeIds = [recordedAttendances[0].AttendeeId, recordedAttendances[1].AttendeeId];
        Assert.Contains(100L, attendeeIds);
        Assert.Contains(101L, attendeeIds);
        List<long> eventIds = [recordedAttendances[0].EventId, recordedAttendances[1].EventId];
        Assert.All(eventIds, id => Assert.Equal(500L, id));
    }

    [Fact]
    public async Task AddAsync_Should_Preserve_ValidationException()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        eventsRepo.AddAsyncFunc = (e, ct) =>
        {
            throw new ValidationException("bad");
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        BookAppointmentDto appointment = new()
        {
            Title = "T",
            Description = "D",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            AppointmentAttendees = []
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(appointment, CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_Should_Wrap_OtherExceptions_In_ApplicationException()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        eventsRepo.AddAsyncFunc = (e, ct) =>
        {
            throw new Exception("unexpected");
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        BookAppointmentDto appointment = new()
        {
            Title = "T",
            Description = "D",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            AppointmentAttendees = []
        };

        ApplicationException ex = await Assert.ThrowsAsync<ApplicationException>(() => service.AddAsync(appointment, CancellationToken.None));
        Assert.NotNull(ex.InnerException);
        Assert.Equal("An error occurred while booking appointment.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Event_And_Attendees_And_Return_EventId()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        Events? receivedEvent = null;
        List<Attendees> updatedAttendees = [];

        eventsRepo.UpdateAsyncFunc = (e, ct) =>
        {
            receivedEvent = e;
            return Task.CompletedTask;
        };

        attendeesRepo.UpdateAsyncFunc = (a, ct) =>
        {
            updatedAttendees.Add(a);
            return Task.CompletedTask;
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        UpdateAppointmentDto update = new()
        {
            EventId = 42L,
            Title = "Updated",
            Description = "Desc",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2.0),
            UpdateAppointmentAttendees =
            [
                new UpdateAppointmentAttendeeDto { AttendeeId = 1L, Name = "A", Email = "a@x", Address = "X", IsAttending = true },
                new UpdateAppointmentAttendeeDto { AttendeeId = 2L, Name = "B", Email = "b@x", Address = "Y", IsAttending = false }
            ]
        };

        long returned = await service.UpdateAsync(update, CancellationToken.None);

        Assert.Equal(42L, returned);
        Assert.NotNull(receivedEvent);
        Assert.Equal("Updated", receivedEvent!.Title);
        Assert.Equal(2, updatedAttendees.Count);
        Assert.Equal(1L, updatedAttendees[0].AttendeeId);
        Assert.Equal(2L, updatedAttendees[1].AttendeeId);
    }

    [Fact]
    public async Task UpdateAsync_Should_Preserve_ValidationException()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        // ensure event update stub does not throw so attendees exception is observed
        eventsRepo.UpdateAsyncFunc = (e, ct) => Task.CompletedTask;

        attendeesRepo.UpdateAsyncFunc = (a, ct) =>
        {
            throw new ValidationException("bad update");
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        UpdateAppointmentDto update = new()
        {
            EventId = 1L,
            Title = "T",
            Description = "D",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            UpdateAppointmentAttendees =
            [
                new UpdateAppointmentAttendeeDto { AttendeeId = 1L, Name = "A", Email = "a", Address = "addr", IsAttending = true }
            ]
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(update, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Event_And_Related_Attendees_And_Attendances_When_Present()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        bool eventDeleted = false;
        List<long> deletedAttendees = [];
        List<long> deletedAttendances = [];

        attendanceRepo.GetByIdAsyncFunc = (eventId, ct) =>
        {
            List<EventAttendance?> list =
            [
                new EventAttendance { EventAttendanceId = 11L, EventId = eventId, AttendeeId = 101L, Attendee = new Attendees { AttendeeId = 101L, Name = "N", Email = "e", Address = "a", IsAttending = true } },
                new EventAttendance { EventAttendanceId = 12L, EventId = eventId, AttendeeId = 102L, Attendee = new Attendees { AttendeeId = 102L, Name = "M", Email = "f", Address = "b", IsAttending = false } }
            ];

            return Task.FromResult<IEnumerable<EventAttendance?>>(list);
        };

        eventsRepo.DeleteAsyncFunc = (id, ct) =>
        {
            eventDeleted = true;
            return Task.CompletedTask;
        };

        attendeesRepo.DeleteAsyncFunc = (id, ct) =>
        {
            deletedAttendees.Add(id);
            return Task.CompletedTask;
        };

        attendanceRepo.DeleteAsyncFunc = (id, ct) =>
        {
            deletedAttendances.Add(id);
            return Task.CompletedTask;
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        await service.DeleteAsync(77L, CancellationToken.None);

        Assert.True(eventDeleted);
        Assert.Equal(2, deletedAttendees.Count);
        Assert.Contains(101L, deletedAttendees);
        Assert.Contains(102L, deletedAttendees);
        Assert.Equal(2, deletedAttendances.Count);
        Assert.Contains(11L, deletedAttendances);
        Assert.Contains(12L, deletedAttendances);
    }

    [Fact]
    public async Task DeleteAsync_Should_Do_Nothing_When_GetByIdAsync_Returns_Null()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        bool eventDeleted = false;

        attendanceRepo.GetByIdAsyncFunc = (eventId, ct) =>
        {
            return Task.FromResult<IEnumerable<EventAttendance?>>(null!);
        };

        eventsRepo.DeleteAsyncFunc = (id, ct) =>
        {
            eventDeleted = true;
            return Task.CompletedTask;
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        await service.DeleteAsync(88L, CancellationToken.None);

        Assert.False(eventDeleted);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Event_NotFound()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        eventsRepo.GetByIdWithAttendancesAsyncFunc = (id, ct) =>
        {
            return Task.FromResult<Events?>(null);
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        AppointmentDetailsDto? result = await service.GetByIdAsync(999L, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Map_Event_And_Attendees_To_Dto()
    {
        StubEventsRepository eventsRepo = new();
        StubAttendeesRepository attendeesRepo = new();
        StubEventAttendanceRepository attendanceRepo = new();

        Events bookingEvent = new()
        {
            EventId = 7L,
            Title = "T7",
            Description = "D7",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            EventAttendances =
            [
                new EventAttendance
                {
                    EventAttendanceId = 21L,
                    EventId = 7L,
                    AttendeeId = 301L,
                    Attendee = new Attendees { AttendeeId = 301L, Name = "Sam", Email = "sam@x", Address = "AddrS", IsAttending = true }
                },
                new EventAttendance
                {
                    EventAttendanceId = 22L,
                    EventId = 7L,
                    AttendeeId = 302L,
                    Attendee = new Attendees { AttendeeId = 302L, Name = "Lee", Email = "lee@x", Address = "AddrL", IsAttending = false }
                }
            ]
        };

        eventsRepo.GetByIdWithAttendancesAsyncFunc = (id, ct) =>
        {
            return Task.FromResult<Events?>(bookingEvent);
        };

        AppointmentService service = new(eventsRepo, attendanceRepo, attendeesRepo);

        AppointmentDetailsDto? dto = await service.GetByIdAsync(7L, CancellationToken.None);

        Assert.NotNull(dto);
        Assert.Equal(7L, dto!.EventId);
        Assert.Equal("T7", dto.Title);
        Assert.Equal(2, dto.AppointmentAttendees!.Count());
        List<AppointmentAttendeeDetailsDto> attendeesList = [.. dto.AppointmentAttendees!];
        Assert.Equal(301L, attendeesList[0].AttendeeId);
        Assert.Equal("Sam", attendeesList[0].Name);
        Assert.Equal("sam@x", attendeesList[0].Email);
        Assert.Equal("AddrS", attendeesList[0].Address);
        Assert.True(attendeesList[0].IsAttending);
    }
}