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
    public async Task AddAsync(BookAppointmentDto appointmentDto, CancellationToken cancellationToken)
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
                    Address = attendee.Address
                }, cancellationToken);

                await eventAttendanceRepository.AddAsync(new EventAttendance
                {
                    EventId = eventId.Value,
                    AttendeeId = attendeeId,
                    IsAttending = true
                }, cancellationToken);
            }
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

    public async Task UpdateAsync(UpdateAppointmentDto appointmentDto, CancellationToken cancellationToken)
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
                    Address = attendee.Address
                }, cancellationToken);

                await eventAttendanceRepository.UpdateAsync(new EventAttendance
                {
                    EventAttendanceId = appointmentDto.EventAttendanceId,
                    IsAttending = appointmentDto.IsAttending
                }, cancellationToken);
            }
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

    public async Task DeleteAsync(long eventAttendanceId, CancellationToken cancellationToken)
    {
        try
        {
            EventAttendance? eventAttendance = await eventAttendanceRepository.GetByIdAsync(eventAttendanceId, cancellationToken);

            if (eventAttendance is not null)
            {
                await eventRepository.DeleteAsync(eventAttendance.EventId, cancellationToken);
                await attendeesRepository.DeleteAsync(eventAttendance.AttendeeId, cancellationToken);           
                await eventAttendanceRepository.DeleteAsync(eventAttendanceId, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting appointment with event attendance id {eventAttendanceId}.", ex);
        }
    }

    public async Task<AppointmentDetailsDto?> GetByIdAsync(long eventAttendanceId, CancellationToken cancellationToken)
    {
        try
        {
            EventAttendance? eventAttendance = await eventAttendanceRepository.GetByIdAsync(eventAttendanceId, cancellationToken);
            Events? bookingEvent = new();

            if (eventAttendance is not null) 
            {
                bookingEvent = await eventRepository.GetByIdAsync(eventAttendance.EventId, cancellationToken);
                IEnumerable<Attendees?> attendees = await attendeesRepository.GetByIdAsync(eventAttendance.AttendeeId, cancellationToken);
            }

            AppointmentDetailsDto? appointmentDetailsDto = new()
            {
                Title = bookingEvent!.Title,
                Description = bookingEvent.Description,
                StartTime = bookingEvent.StartTime,
                EndTime = bookingEvent.EndTime,
            };

            return appointmentDetailsDto;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while retrieving booking appointment with event attendance id {eventAttendanceId}.", ex);
        }
    }
}