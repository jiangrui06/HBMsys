namespace HospitalWebAPI.Models
{
    public enum PrescriptionStatus
    {
        Created,
        Charged,
        Dispensed
    }

    public class Prescription : BaseEntity
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public List<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Created;
    }
}
