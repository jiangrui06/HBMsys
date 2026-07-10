namespace HospitalWebAPI.Models
{
    public class PatientReg : BaseEntity
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}
