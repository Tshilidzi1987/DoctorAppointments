using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureTests.Repositories;

public class EventAttendanceRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private class TestAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        public bool ThrowOnSave { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (ThrowOnSave)
            {
                throw new DbUpdateConcurrencyException();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }

    private static TestAppDbContext CreateTestContext(bool throwOnSave = false)
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        TestAppDbContext ctx = new(options) { ThrowOnSave = throwOnSave };
        return ctx;
    }

    [Fact]
    public async Task AddAsync_Should_Add_Attendance_And_AssignId()
    {
        await using AppDbContext ctx = CreateContext();
        EventAttendanceRepository repo = new(ctx);

        EventAttendance attendance = new()
        {
            EventId = 10,
            AttendeeId = 20
        };

        await repo.AddAsync(attendance, CancellationToken.None);

        Assert.True(attendance.EventAttendanceId != 0);
        EventAttendance? persisted = await ctx.AppointmentEventAttendance.FindAsync(attendance.EventAttendanceId);
        Assert.NotNull(persisted);
        Assert.Equal(10, persisted!.EventId);
        Assert.Equal(20, persisted.AttendeeId);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Attendances_For_Event()
    {
        await using AppDbContext ctx = CreateContext();

        EventAttendance a1 = new() { EventId = 1, AttendeeId = 101 };
        EventAttendance a2 = new() { EventId = 1, AttendeeId = 102 };
        EventAttendance a3 = new() { EventId = 2, AttendeeId = 201 };

        await ctx.AppointmentEventAttendance.AddRangeAsync([a1, a2, a3]);
        await ctx.SaveChangesAsync();

        EventAttendanceRepository repo = new(ctx);

        IEnumerable<EventAttendance?> results = await repo.GetByIdAsync(1, CancellationToken.None);

        List<EventAttendance?> list = [.. results];
        Assert.Equal(2, list.Count);
        List<long> attendeeIds = [.. list.Select(x => x!.AttendeeId).OrderBy(x => x)];
        Assert.Equal([101L, 102L], attendeeIds);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Empty_When_None()
    {
        await using AppDbContext ctx = CreateContext();

        EventAttendanceRepository repo = new(ctx);

        IEnumerable<EventAttendance?> results = await repo.GetByIdAsync(9999, CancellationToken.None);

        List<EventAttendance?> list = [.. results];
        Assert.Empty(list);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Attendance_When_Exists()
    {
        await using AppDbContext ctx = CreateContext();

        EventAttendance attendance = new() { EventId = 5, AttendeeId = 55 };
        await ctx.AppointmentEventAttendance.AddAsync(attendance);
        await ctx.SaveChangesAsync();

        EventAttendanceRepository repo = new(ctx);

        await repo.DeleteAsync(attendance.EventAttendanceId, CancellationToken.None);

        EventAttendance? found = await ctx.AppointmentEventAttendance.FindAsync(attendance.EventAttendanceId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotThrow_When_Not_Exists()
    {
        await using AppDbContext ctx = CreateContext();
        EventAttendanceRepository repo = new(ctx);

        Exception? ex = await Record.ExceptionAsync(() => repo.DeleteAsync(12345, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteAsync_Should_Handle_DbUpdateConcurrencyException_Gracefully()
    {
        await using TestAppDbContext ctx = CreateTestContext(throwOnSave: true);

        // No need to seed the entity; repository will mark a stub entity as Deleted and SaveChangesAsync will throw.
        EventAttendanceRepository repo = new(ctx);

        // The repository catches DbUpdateConcurrencyException and should not surface it.
        Exception? ex = await Record.ExceptionAsync(() => repo.DeleteAsync(9999, CancellationToken.None));
        Assert.Null(ex);
    }
}