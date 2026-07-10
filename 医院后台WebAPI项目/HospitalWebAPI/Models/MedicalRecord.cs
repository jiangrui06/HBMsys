namespace HospitalWebAPI.Models
{
    public class MedicalRecord : BaseEntity
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; } = DateTime.Now;
    }
}
