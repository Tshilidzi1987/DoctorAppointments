
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Events> AppointmentEvent { get; set; } = null!;
    public DbSet<Attendees> AppointmentAttendee { get; set; } = null!;
    public DbSet<EventAttendance> AppointmentEventAttendance { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Events>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
        });

        modelBuilder.Entity<Attendees>(entity =>
        {
            entity.ToTable("Attendees");
            entity.HasKey(e => e.AttendeeId);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.IsAttending).IsRequired();
        });

        modelBuilder.Entity<EventAttendance>(entity =>
        {
            entity.ToTable("EventAttendance");
            entity.HasKey(e => e.EventAttendanceId);

            // configure foreign keys and navigation properties
            entity.HasOne(ea => ea.Event)
                  .WithMany(e => e.EventAttendances)
                  .HasForeignKey(ea => ea.EventId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            entity.HasOne(ea => ea.Attendee)
                  .WithMany(a => a.EventAttendances)
                  .HasForeignKey(ea => ea.AttendeeId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            // EF Core will not include EventId and AttendeeId on UPDATE statements.
            entity.Property(e => e.EventId)
                  .IsRequired()
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            entity.Property(e => e.AttendeeId)
                  .IsRequired()
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            
        });
    }
}