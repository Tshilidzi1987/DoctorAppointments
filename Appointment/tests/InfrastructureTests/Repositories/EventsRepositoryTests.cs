using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureTests.Repositories;

public class EventsRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Events CreateValidEvent(string title = "Family Planning", int durationHours = 1)
    {
        return new()
        {
            Title = title,
            Description = "Family Planning",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(durationHours)
        };
    }

    private static Attendees CreateValidAttendee(string name = "chris", string email = "chris@gmail.com")
    {
        return new()
        {
            Name = name,
            Email = email,
            Address = "123 Test St",
            IsAttending = true
        };
    }

    private static EventAttendance CreateValidAttendance(long eventId, long attendeeId)
    {
        return new()
        {
            EventId = eventId,
            AttendeeId = attendeeId
        };
    }

    [Fact]
    public async Task AddAsync_Should_AddEvent_AndReturnId()
    {
        await using AppDbContext ctx = CreateContext();
        EventsRepository repo = new(ctx);

        Events evt = CreateValidEvent("Family Planning", 1);

        long id = await repo.AddAsync(evt, CancellationToken.None);

        Assert.True(id != 0, "Expected returned id to be set by EF provider");
        Events? persisted = await ctx.AppointmentEvent.FindAsync(id);
        Assert.NotNull(persisted);
        Assert.Equal("Family Planning", persisted!.Title);
    }

    [Fact]
    public async Task GetByIdWithAttendancesAsync_Should_LoadAttendancesAndAttendee()
    {
        await using AppDbContext ctx = CreateContext();
        // seed event, attendee and attendance linking them using qualifying objects
        Attendees attendee = CreateValidAttendee("Chris", "chris@gmail.com");
        Events evt = CreateValidEvent("Roadmap Review", 2);

        await ctx.AppointmentAttendee.AddAsync(attendee);
        await ctx.AppointmentEvent.AddAsync(evt);
        await ctx.SaveChangesAsync();

        EventAttendance attendance = CreateValidAttendance(evt.EventId, attendee.AttendeeId);
        await ctx.AppointmentEventAttendance.AddAsync(attendance);
        await ctx.SaveChangesAsync();

        EventsRepository repo = new(ctx);

        Events? loaded = await repo.GetByIdWithAttendancesAsync(evt.EventId, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(evt.EventId, loaded!.EventId);
        Assert.NotNull(loaded.EventAttendances);
        Assert.Single(loaded.EventAttendances);
        EventAttendance loadedAttendance = loaded.EventAttendances!.First();
        Assert.NotNull(loadedAttendance.Attendee);
        Assert.Equal("Chris", loadedAttendance.Attendee!.Name);
    }

    [Fact]
    public async Task GetByIdWithAttendancesAsync_Should_ReturnNull_WhenNotFound()
    {
        await using AppDbContext ctx = CreateContext();
        EventsRepository repo = new(ctx);

        Events? result = await repo.GetByIdWithAttendancesAsync(9999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistChanges()
    {
        await using AppDbContext ctx = CreateContext();
        Events evt = CreateValidEvent("Initial Title", 1);

        await ctx.AppointmentEvent.AddAsync(evt);
        await ctx.SaveChangesAsync();

        EventsRepository repo = new(ctx);

        evt.Title = "Updated Title";
        await repo.UpdateAsync(evt, CancellationToken.None);

        Events? persisted = await ctx.AppointmentEvent.FindAsync(evt.EventId);
        Assert.NotNull(persisted);
        Assert.Equal("Updated Title", persisted!.Title);
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveEvent_WhenExists()
    {
        await using AppDbContext ctx = CreateContext();
        Events evt = CreateValidEvent("To Be Deleted", 1);

        await ctx.AppointmentEvent.AddAsync(evt);
        await ctx.SaveChangesAsync();

        EventsRepository repo = new(ctx);

        await repo.DeleteAsync(evt.EventId, CancellationToken.None);

        Events? found = await ctx.AppointmentEvent.FindAsync(evt.EventId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotThrow_WhenNotExists()
    {
        await using AppDbContext ctx = CreateContext();
        EventsRepository repo = new(ctx);

        // calling delete for an id that doesn't exist must not throw
        Exception? ex = await Record.ExceptionAsync(() => repo.DeleteAsync(12345, CancellationToken.None));
        Assert.Null(ex);
    }
}