namespace HospitalWebAPI.Models
{
    public class Report : BaseEntity
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string ExamItem { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime ReportTime { get; set; } = DateTime.Now;
    }
}
