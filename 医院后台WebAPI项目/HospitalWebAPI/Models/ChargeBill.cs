namespace HospitalWebAPI.Models
{
    public enum ChargeStatus
    {
        Unpaid,
        Paid,
        Cancelled
    }

    public class ChargeBill : BaseEntity
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ChargeStatus Status { get; set; } = ChargeStatus.Unpaid;
        public string? ReceiptNo { get; set; }
    }
}
