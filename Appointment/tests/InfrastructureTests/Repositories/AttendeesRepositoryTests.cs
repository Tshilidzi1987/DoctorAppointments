using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureTests.Repositories;

public class AttendeesRepositoryTests
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
    public async Task AddAsync_Should_Add_Attendee_And_AssignId()
    {
        await using AppDbContext ctx = CreateContext();
        AttendeesRepository repo = new(ctx);

        Attendees attendee = new()
        {
            Name = "chris",
            Email = "chris@example.com",
            Address = "123 Main St",
            IsAttending = true
        };

        long assignedId = await repo.AddAsync(attendee, CancellationToken.None);

        Assert.True(assignedId != 0);
        Attendees? persisted = await ctx.AppointmentAttendee.FindAsync(assignedId);
        Assert.NotNull(persisted);
        Assert.Equal("chris", persisted!.Name);
        Assert.Equal("chris@example.com", persisted.Email);
        Assert.Equal("123 Main St", persisted.Address);
        Assert.True(persisted.IsAttending);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Attendee()
    {
        await using AppDbContext ctx = CreateContext();

        Attendees seed = new()
        {
            Name = "Bob",
            Email = "bob@old.com",
            Address = "Old Address",
            IsAttending = false
        };

        await ctx.AppointmentAttendee.AddAsync(seed);
        await ctx.SaveChangesAsync();

        AttendeesRepository repo = new(ctx);

        // Modify the seeded entity
        Attendees toUpdate = new()
        {
            AttendeeId = seed.AttendeeId,
            Name = "Bob Updated",
            Email = "bob@new.com",
            Address = "New Address",
            IsAttending = true
        };

        await repo.UpdateAsync(toUpdate, CancellationToken.None);

        Attendees? refreshed = await ctx.AppointmentAttendee.FindAsync(seed.AttendeeId);
        Assert.NotNull(refreshed);
        Assert.Equal("Bob Updated", refreshed!.Name);
        Assert.Equal("bob@new.com", refreshed.Email);
        Assert.Equal("New Address", refreshed.Address);
        Assert.True(refreshed.IsAttending);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Attendee_When_Exists()
    {
        await using AppDbContext ctx = CreateContext();

        Attendees attendee = new() { Name = "Carol", Email = "carol@example.com", Address = "Addr", IsAttending = true };
        await ctx.AppointmentAttendee.AddAsync(attendee);
        await ctx.SaveChangesAsync();

        AttendeesRepository repo = new(ctx);

        await repo.DeleteAsync(attendee.AttendeeId, CancellationToken.None);

        Attendees? found = await ctx.AppointmentAttendee.FindAsync(attendee.AttendeeId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotThrow_When_Not_Exists()
    {
        await using AppDbContext ctx = CreateContext();
        AttendeesRepository repo = new(ctx);

        Exception? ex = await Record.ExceptionAsync(() => repo.DeleteAsync(99999, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteAsync_Should_Handle_DbUpdateConcurrencyException_Gracefully()
    {
        await using TestAppDbContext ctx = CreateTestContext(throwOnSave: true);

        // Do not seed any entity; repository will mark a stub entity as Deleted and SaveChangesAsync will throw.
        AttendeesRepository repo = new(ctx);

        Exception? ex = await Record.ExceptionAsync(() => repo.DeleteAsync(5555, CancellationToken.None));
        Assert.Null(ex);
    }
}