namespace Test1.Models;

public class AppointmentDTO
{
    public DateTime Date { get; set; }
    public PatientDTO Patient { get; set; }
    public DoctorDTO Doctor { get; set; }
    public List<AppointmentServiceDTO> Appointments { get; set; } = [];
}

public class PatientDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class DoctorDTO
{
    public int Id { get; set; }
    public string Password { get; set; }
}

public class AppointmentServiceDTO
{
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
}