using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AppointmentController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetAppointmentById(long eventId, CancellationToken cancellationToken)
    {
        AppointmentDetailsDto? appointmentDetails = await appointmentService.GetByIdAsync(eventId, cancellationToken);
        if (appointmentDetails is null)
        {
            return NotFound();
        }
        return Ok(appointmentDetails);
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto bookAppointmentDto, CancellationToken cancellationToken)
    {
        long eventId = await appointmentService.AddAsync(bookAppointmentDto, cancellationToken);

        AppointmentDetailsDto? createdDetails = await appointmentService.GetByIdAsync(eventId, cancellationToken);

        if (createdDetails is null)
        {
            return NotFound();
        }
        return Ok(createdDetails);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentDto appointmentDto, CancellationToken cancellationToken)
    {
        long eventId = await appointmentService.UpdateAsync(appointmentDto, cancellationToken);

        AppointmentDetailsDto? updatedDetails = await appointmentService.GetByIdAsync(eventId, cancellationToken);

        if (updatedDetails is null)
        {
            return NotFound();
        }
        return Ok(updatedDetails);
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> CanccelAppointment(long eventId, CancellationToken cancellationToken)
    {
        await appointmentService.DeleteAsync(eventId, cancellationToken);
        return NoContent();
    }
}