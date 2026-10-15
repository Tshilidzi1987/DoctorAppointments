using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Services;

public class AppointmentService(
    IEventsRepository eventRepository,
    IEventAttendanceRepository eventAttendanceRepository,
    IAttendeesRepository attendeesRepository) : IAppointmentService
{
    public async Task<long> AddAsync(BookAppointmentDto appointmentDto, CancellationToken cancellationToken)
    {
        try
        {
            long? eventId = await eventRepository.AddAsync(new Events 
            {
                Title = appointmentDto.Title,
                Description = appointmentDto.Description,
                StartTime = appointmentDto.StartTime,
                EndTime = appointmentDto.EndTime
            },cancellationToken);

            foreach (BookAppointmentAttendeeDto attendee in appointmentDto.AppointmentAttendees!)
            {
                long attendeeId = await attendeesRepository.AddAsync(new Attendees
                {
                    Name = attendee.Name,
                    Email = attendee.Email,
                    Address = attendee.Address,
                    IsAttending = true

                }, cancellationToken);

                await eventAttendanceRepository.AddAsync(new EventAttendance
                {
                    EventId = eventId.Value,
                    AttendeeId = attendeeId
                }, cancellationToken);
            }

            return eventId.Value;
        }
        catch (ValidationException)
        {
            // Preserve validation exception type so callers can handle it explicitly
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while booking appointment.", ex);
        }
    }

    public async Task<long> UpdateAsync(UpdateAppointmentDto appointmentDto, CancellationToken cancellationToken)
    {
        try
        {
            await eventRepository.UpdateAsync(new Events
            {
                EventId = appointmentDto.EventId,
                Title = appointmentDto.Title,
                Description = appointmentDto.Description,
                StartTime = appointmentDto.StartTime,
                EndTime = appointmentDto.EndTime
            }, cancellationToken);

            foreach (UpdateAppointmentAttendeeDto attendee in appointmentDto.UpdateAppointmentAttendees!)
            {
                await attendeesRepository.UpdateAsync(new Attendees
                {
                    AttendeeId = attendee.AttendeeId,
                    Name = attendee.Name,
                    Email = attendee.Email,
                    Address = attendee.Address,
                    IsAttending = attendee.IsAttending
                }, cancellationToken);
            }

            return appointmentDto.EventId;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while updating doctors appointment.", ex);
        }
    }

    public async Task DeleteAsync(long eventId, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<EventAttendance?> eventAttendance = await eventAttendanceRepository.GetByIdAsync(eventId, cancellationToken);

            if (eventAttendance is not null)
            {
                await eventRepository.DeleteAsync(eventId, cancellationToken);

                foreach (EventAttendance? attendance in eventAttendance)
                {
                    await attendeesRepository.DeleteAsync(attendance!.AttendeeId, cancellationToken);

                    await eventAttendanceRepository.DeleteAsync(attendance!.EventAttendanceId, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting appointment with event id {eventId}.", ex);
        }
    }

    public async Task<AppointmentDetailsDto?> GetByIdAsync(long eventId, CancellationToken cancellationToken)
    {
        try
        {
            // load event with attendances and attendee details
            Events? bookingEvent = await eventRepository.GetByIdWithAttendancesAsync(eventId, cancellationToken);
            if (bookingEvent is null)
            {
                return null;
            }         

            AppointmentDetailsDto appointmentDetailsDto = new()
            {
                EventId = bookingEvent.EventId,
                Title = bookingEvent.Title,
                Description = bookingEvent.Description,
                StartTime = bookingEvent.StartTime,
                EndTime = bookingEvent.EndTime,
                AppointmentAttendees = [.. (bookingEvent.EventAttendances ?? Enumerable.Empty<EventAttendance>())
                    .Where(ea => ea?.Attendee is not null)
                    .Select(ea => new AppointmentAttendeeDetailsDto
                    {
                        AttendeeId = ea.AttendeeId,
                        Name = ea!.Attendee!.Name,
                        Email = ea.Attendee.Email,
                        Address = ea.Attendee.Address,
                        IsAttending = ea.Attendee.IsAttending
                    })]
            };

            return appointmentDetailsDto;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while retrieving booking appointment with event id {eventId}.", ex);
        }
    }
}