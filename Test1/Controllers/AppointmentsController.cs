using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Test1.Exceptions;
using Test1.Models;
using Test1.Services;

namespace Test1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly IDbService _dbService;

    public AppointmentsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{appointmentId}")]
    public async Task<IActionResult> GetAppointmentById(int appointmentId)
    {
        try
        {
            var result = await _dbService.GetAppointmentByIdAsync(appointmentId);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound("Appointment not found");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    [Route("api/appointments")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO appointment)
    {
        if (appointment == null)
        {
            return BadRequest("Appointment data is required.");
        }

        if (appointment.Services == null || !appointment.Services.Any())
        {
            return BadRequest("At least one service is required.");
        }

        try
        {
            await _dbService.CreateAppointmentAsync(appointment);
            return StatusCode(201, "Appointment created successfully.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }
}