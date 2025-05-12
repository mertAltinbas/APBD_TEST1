using System.Data.Common;
using Microsoft.Data.SqlClient;
using Test1.Exceptions;
using Test1.Models;

namespace Test1.Services;

public class DbService : IDbService
{
    private readonly string? _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId)
    {
        var query =
            @"select  a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.pwz, s.name, ap.service_fee
            from appointment a
             join doctor d on d.doctor_id = a.doctor_id
             join patient p on p.patient_id = a.patient_id
             join Appointment_service ap on ap.appointment_id = a.appointment_id
             join ""service"" s on s.service_id = ap.service_id
            where a.appointment_id = @appointmentId;";

        AppointmentDTO? appointments = null;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        await connection.OpenAsync();

        command.Parameters.AddWithValue("@appointmentId", appointmentId);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (appointments == null)
            {
                appointments = new AppointmentDTO()
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientDTO
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorDTO
                    {
                        Id = reader.GetInt32(4),
                        Password = reader.GetString(5)
                    },
                    Appointments = new List<AppointmentServiceDTO>()
                };
            }

            appointments.Appointments.Add(new AppointmentServiceDTO()
            {
                Name = reader.GetString(6),
                ServiceFee = reader.GetDecimal(7)
            });
        }

        if (appointments is null)
        {
            throw new NotFoundException("No appointments found for the specified ID.");
        }

        return appointments;
    }

    public async Task CreateAppointmentAsync(CreateAppointmentDTO appointment)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            // Check if patient exists
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(1) FROM Patient WHERE patient_id = @patientId";
            command.Parameters.AddWithValue("@patientId", appointment.PattientId);
            var patientExists = (int)await command.ExecuteScalarAsync() > 0;
            if (!patientExists)
                throw new NotFoundException($"Patient with ID {appointment.PattientId} does not exist.");

            // Check if doctor exists
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(1) FROM Doctor WHERE pwz = @pwz";
            command.Parameters.AddWithValue("@pwz", appointment.DoctorPaswword);
            var doctorExists = (int)await command.ExecuteScalarAsync() > 0;
            if (!doctorExists)
                throw new NotFoundException($"Doctor with PWZ {appointment.DoctorPaswword} does not exist.");

            // Insert appointment
            command.Parameters.Clear();
            command.CommandText =
                "INSERT INTO Appointment (appointment_id, patient_id, doctor_id) VALUES (@appointmentId, @patientId, @pwz)";
            command.Parameters.AddWithValue("@appointmentId", appointment.AppointmentId);
            command.Parameters.AddWithValue("@patientId", appointment.PattientId);
            command.Parameters.AddWithValue("@pwz", appointment.DoctorPaswword);
            await command.ExecuteNonQueryAsync();

            foreach (var service in appointment.Services)
            {
                // Check if service exists
                command.Parameters.Clear();
                command.CommandText = "SELECT COUNT(1) FROM Service WHERE name = @serviceName";
                command.Parameters.AddWithValue("@serviceName", service.Name);
                var serviceExists = (int)await command.ExecuteScalarAsync() > 0;
                if (!serviceExists)
                    throw new NotFoundException($"Service with name {service.Name} does not exist.");

                // Insert service into appointment_service
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Appointment_Service (appointment_id, service_id, service_fee) " +
                                      "SELECT @appointmentId, service_id, @serviceFee FROM Service WHERE name = @serviceName";
                command.Parameters.AddWithValue("@appointmentId", appointment.AppointmentId);
                command.Parameters.AddWithValue("@serviceFee", service.ServiceFee);
                command.Parameters.AddWithValue("@serviceName", service.Name);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}