namespace Test1.Models;

public class CreateAppointmentDTO
{
    public int AppointmentId { get; set; }
    public int PattientId { get; set; }
    public int DoctorPaswword { get; set; }
    public List<ServicesDTO> Services { get; set; } = [];
}

public class ServicesDTO
{
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
}