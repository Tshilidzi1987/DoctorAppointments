
using Api.Controllers.V1;
using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTests.Controllers.V1;

public class AppointmentControllerTests
{
    private class StubAppointmentService : IAppointmentService
    {
        public Func<long, CancellationToken, Task<AppointmentDetailsDto?>>? GetByIdAsyncFunc;
        public Func<BookAppointmentDto, CancellationToken, Task<long>>? AddAsyncFunc;
        public Func<UpdateAppointmentDto, CancellationToken, Task<long>>? UpdateAsyncFunc;
        public Func<long, CancellationToken, Task>? DeleteAsyncFunc;

        public Task<AppointmentDetailsDto?> GetByIdAsync(long eventId, CancellationToken cancellationToken)
        {
            if (GetByIdAsyncFunc is null)
            {
                throw new InvalidOperationException("GetByIdAsyncFunc not configured");
            }

            return GetByIdAsyncFunc(eventId, cancellationToken);
        }

        public Task<long> AddAsync(BookAppointmentDto appointmentDto, CancellationToken cancellationToken)
        {
            if (AddAsyncFunc is null)
            {
                throw new InvalidOperationException("AddAsyncFunc not configured");
            }

            return AddAsyncFunc(appointmentDto, cancellationToken);
        }

        public Task<long> UpdateAsync(UpdateAppointmentDto appointmentDto, CancellationToken cancellationToken)
        {
            if (UpdateAsyncFunc is null)
            {
                throw new InvalidOperationException("UpdateAsyncFunc not configured");
            }

            return UpdateAsyncFunc(appointmentDto, cancellationToken);
        }

        public Task DeleteAsync(long eventId, CancellationToken cancellationToken)
        {
            if (DeleteAsyncFunc is null)
            {
                throw new InvalidOperationException("DeleteAsyncFunc not configured");
            }

            return DeleteAsyncFunc(eventId, cancellationToken);
        }
    }

    [Fact]
    public async Task GetAppointmentById_Returns_NotFound_When_Service_Returns_Null()
    {
        StubAppointmentService stub = new()
        {
            GetByIdAsyncFunc = (id, ct) =>
            {
                return Task.FromResult<AppointmentDetailsDto?>(null);
            }
        };

        AppointmentController controller = new(stub);

        IActionResult result = await controller.GetAppointmentById(999L, CancellationToken.None);

        NotFoundResult? notFound = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentById_Returns_Ok_With_Details_When_Found()
    {
        StubAppointmentService stub = new();
        AppointmentDetailsDto details = new()
        {
            EventId = 1L,
            Title = "T1",
            Description = "D1",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            AppointmentAttendees =
            [
                new AppointmentAttendeeDetailsDto { AttendeeId = 10L, Name = "X", Email = "x@x", Address = "Addr", IsAttending = true }
            ]
        };

        stub.GetByIdAsyncFunc = (id, ct) =>
        {
            return Task.FromResult<AppointmentDetailsDto?>(details);
        };

        AppointmentController controller = new(stub);

        IActionResult actionResult = await controller.GetAppointmentById(1L, CancellationToken.None);

        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(actionResult);
        AppointmentDetailsDto? returned = Assert.IsType<AppointmentDetailsDto>(okResult.Value);
        Assert.Equal(details.EventId, returned.EventId);
        Assert.Equal(details.Title, returned.Title);
    }

    [Fact]
    public async Task BookAppointment_Returns_NotFound_When_GetById_Returns_Null()
    {
        StubAppointmentService stub = new()
        {
            AddAsyncFunc = (dto, ct) =>
            {
                return Task.FromResult(200L);
            },
            GetByIdAsyncFunc = (id, ct) =>
            {
                return Task.FromResult<AppointmentDetailsDto?>(null);
            }
        };

        AppointmentController controller = new(stub);

        BookAppointmentDto request = new()
        {
            Title = "New",
            Description = "D",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            AppointmentAttendees = []
        };

        IActionResult result = await controller.BookAppointment(request, CancellationToken.None);

        NotFoundResult? notFound = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task BookAppointment_Returns_Ok_With_Details_When_Succeeds()
    {
        StubAppointmentService stub = new();
        AppointmentDetailsDto created = new()
        {
            EventId = 300L,
            Title = "Created",
            Description = "Desc",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            AppointmentAttendees = []
        };

        stub.AddAsyncFunc = (dto, ct) =>
        {
            return Task.FromResult(300L);
        };
        stub.GetByIdAsyncFunc = (id, ct) =>
        {
            return Task.FromResult<AppointmentDetailsDto?>(created);
        };

        AppointmentController controller = new(stub);

        BookAppointmentDto request = new()
        {
            Title = "New",
            Description = "D",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            AppointmentAttendees = []
        };

        IActionResult result = await controller.BookAppointment(request, CancellationToken.None);

        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        AppointmentDetailsDto? returned = Assert.IsType<AppointmentDetailsDto>(okResult.Value);
        Assert.Equal(300L, returned.EventId);
        Assert.Equal("Created", returned.Title);
    }

    [Fact]
    public async Task UpdateAppointment_Returns_NotFound_When_GetById_Returns_Null()
    {
        StubAppointmentService stub = new()
        {
            UpdateAsyncFunc = (dto, ct) =>
            {
                return Task.FromResult(400L);
            },
            GetByIdAsyncFunc = (id, ct) =>
            {
                return Task.FromResult<AppointmentDetailsDto?>(null);
            }
        };

        AppointmentController controller = new(stub);

        UpdateAppointmentDto update = new()
        {
            EventId = 400L,
            Title = "U",
            Description = "D",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1.0),
            UpdateAppointmentAttendees = []
        };

        IActionResult result = await controller.UpdateAppointment(update, CancellationToken.None);

        NotFoundResult? notFound = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task UpdateAppointment_Returns_Ok_With_Details_When_Succeeds()
    {
        StubAppointmentService stub = new();
        AppointmentDetailsDto updated = new()
        {
            EventId = 500L,
            Title = "Udone",
            Description = "DescU",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2.0),
            AppointmentAttendees = []
        };

        stub.UpdateAsyncFunc = (dto, ct) =>
        {
            return Task.FromResult(500L);
        };
        stub.GetByIdAsyncFunc = (id, ct) =>
        {
            return Task.FromResult<AppointmentDetailsDto?>(updated);
        };

        AppointmentController controller = new(stub);

        UpdateAppointmentDto update = new()
        {
            EventId = 500L,
            Title = "Udone",
            Description = "DescU",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2.0),
            UpdateAppointmentAttendees = []
        };

        IActionResult result = await controller.UpdateAppointment(update, CancellationToken.None);

        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        AppointmentDetailsDto? returned = Assert.IsType<AppointmentDetailsDto>(okResult.Value);
        Assert.Equal(500L, returned.EventId);
        Assert.Equal("Udone", returned.Title);
    }

    [Fact]
    public async Task CanccelAppointment_Returns_NoContent_And_Calls_DeleteAsync()
    {
        StubAppointmentService stub = new();
        bool deletedCalled = false;
        long deletedId = 0L;

        stub.DeleteAsyncFunc = (id, ct) =>
        {
            deletedCalled = true;
            deletedId = id;
            return Task.CompletedTask;
        };

        AppointmentController controller = new(stub);

        IActionResult result = await controller.CanccelAppointment(777L, CancellationToken.None);

        NoContentResult? noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContent.StatusCode);
        Assert.True(deletedCalled);
        Assert.Equal(777L, deletedId);
    }
}