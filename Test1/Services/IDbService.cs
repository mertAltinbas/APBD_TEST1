using Test1.Models;

namespace Test1.Services;

public interface IDbService
{
    Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId);
    Task CreateAppointmentAsync(CreateAppointmentDTO appointment);
}