using HospitalWebAPI.Models;
using System.Text.RegularExpressions;

namespace HospitalWebAPI.Services
{
    public class HospitalDataService
    {
        private readonly List<Medicine> _medicines = new();
        private readonly List<Prescription> _prescriptions = new();
        private readonly List<ChargeBill> _chargeBills = new();
        private readonly List<PatientReg> _patientRegs = new();
        private readonly List<MedicalRecord> _medicalRecords = new();
        private readonly List<Report> _reports = new();

        private readonly object _lock = new();

        private static readonly HashSet<string> ValidDepartments = new(StringComparer.OrdinalIgnoreCase)
        {
            "骨科", "口腔科", "内科", "外科", "儿科", "妇科", "眼科", "皮肤科"
        };

        public HospitalDataService()
        {
            _medicines.AddRange(new[]
            {
                new Medicine { Id = 1, Name = "阿莫西林胶囊", Specification = "0.25g*24粒", Price = 18.50m, Stock = 100, CreatedBy = "System" },
                new Medicine { Id = 2, Name = "布洛芬缓释胶囊", Specification = "0.3g*20粒", Price = 22.00m, Stock = 80, CreatedBy = "System" },
                new Medicine { Id = 3, Name = "感冒灵颗粒", Specification = "10g*9袋", Price = 15.00m, Stock = 120, CreatedBy = "System" },
                new Medicine { Id = 4, Name = "维生素C片", Specification = "0.1g*100片", Price = 9.80m, Stock = 200, CreatedBy = "System" }
            });
        }

        #region Medicine
        public List<Medicine> GetAllMedicines() => _medicines.ToList();

        public Medicine? GetMedicineById(int id) => _medicines.FirstOrDefault(m => m.Id == id);

        public (bool success, string message, Medicine? medicine) AddMedicine(Medicine medicine)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(medicine.Name))
                    return (false, "药品名称不能为空", null);
                if (medicine.Price < 0)
                    return (false, "药品价格不能为负数", null);
                if (medicine.Stock < 0)
                    return (false, "药品库存不能为负数", null);
                if (_medicines.Any(m => m.Name.Equals(medicine.Name, StringComparison.OrdinalIgnoreCase)))
                    return (false, "药品已存在，请勿重复添加", null);

                medicine.Id = _medicines.Count > 0 ? _medicines.Max(m => m.Id) + 1 : 1;
                medicine.CreatedAt = DateTime.Now;
                _medicines.Add(medicine);
                return (true, "药品添加成功", medicine);
            }
        }

        public (bool success, string message) DeleteMedicine(int id, bool confirmed)
        {
            lock (_lock)
            {
                if (!confirmed)
                    return (false, "请确认是否删除该药品");

                var medicine = _medicines.FirstOrDefault(m => m.Id == id);
                if (medicine == null)
                    return (false, "药品不存在");

                _medicines.Remove(medicine);
                return (true, "药品删除成功");
            }
        }
        #endregion

        #region Prescription
        public (bool success, string message, Prescription? prescription) AddPrescription(Prescription prescription)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(prescription.PatientName))
                    return (false, "患者姓名不能为空", null);
                if (string.IsNullOrWhiteSpace(prescription.DoctorName))
                    return (false, "医生姓名不能为空", null);
                if (prescription.Items == null || prescription.Items.Count == 0)
                    return (false, "处方药品不能为空", null);

                foreach (var item in prescription.Items)
                {
                    var medicine = _medicines.FirstOrDefault(m => m.Id == item.MedicineId);
                    if (medicine == null)
                        return (false, $"药品 ID {item.MedicineId} 不存在", null);
                    if (item.Quantity <= 0)
                        return (false, $"药品 {medicine.Name} 数量必须大于 0", null);
                    if (item.Quantity > medicine.Stock)
                        return (false, $"药品 {medicine.Name} 库存不足，当前库存 {medicine.Stock}", null);

                    item.MedicineName = medicine.Name;
                    item.UnitPrice = medicine.Price;
                }

                prescription.Id = _prescriptions.Count > 0 ? _prescriptions.Max(p => p.Id) + 1 : 1;
                prescription.Status = PrescriptionStatus.Created;
                prescription.CreatedAt = DateTime.Now;
                _prescriptions.Add(prescription);
                return (true, "处方开具成功", prescription);
            }
        }

        public List<Prescription> GetAllPrescriptions() => _prescriptions.ToList();

        public Prescription? GetPrescriptionById(int id) => _prescriptions.FirstOrDefault(p => p.Id == id);

        public (bool success, string message, Prescription? prescription) DispensePrescription(int id)
        {
            lock (_lock)
            {
                var prescription = _prescriptions.FirstOrDefault(p => p.Id == id);
                if (prescription == null)
                    return (false, "处方不存在", null);
                if (prescription.Status == PrescriptionStatus.Dispensed)
                    return (false, "该处方已发药，请勿重复发药", null);
                if (prescription.Status != PrescriptionStatus.Charged)
                    return (false, "该处方尚未缴费结算，无法发药", null);

                foreach (var item in prescription.Items)
                {
                    var medicine = _medicines.FirstOrDefault(m => m.Id == item.MedicineId);
                    if (medicine == null)
                        return (false, $"药品 {item.MedicineName} 不存在", null);
                    if (item.Quantity > medicine.Stock)
                        return (false, $"药品 {medicine.Name} 库存不足，当前库存 {medicine.Stock}", null);

                    medicine.Stock -= item.Quantity;
                }

                prescription.Status = PrescriptionStatus.Dispensed;
                return (true, "发药成功，库存已扣减", prescription);
            }
        }
        #endregion

        #region Charge
        public (bool success, string message, ChargeBill? bill) CreateChargeBill(int prescriptionId)
        {
            lock (_lock)
            {
                var prescription = _prescriptions.FirstOrDefault(p => p.Id == prescriptionId);
                if (prescription == null)
                    return (false, "处方不存在", null);
                if (_chargeBills.Any(b => b.PrescriptionId == prescriptionId && b.Status != ChargeStatus.Cancelled))
                    return (false, "该处方已生成收费单", null);

                var bill = new ChargeBill
                {
                    Id = _chargeBills.Count > 0 ? _chargeBills.Max(b => b.Id) + 1 : 1,
                    PrescriptionId = prescription.Id,
                    PatientName = prescription.PatientName,
                    Amount = prescription.TotalAmount,
                    Status = ChargeStatus.Unpaid,
                    CreatedAt = DateTime.Now
                };
                _chargeBills.Add(bill);
                return (true, "收费单生成成功", bill);
            }
        }

        public List<ChargeBill> GetAllChargeBills() => _chargeBills.ToList();

        public ChargeBill? GetChargeBillById(int id) => _chargeBills.FirstOrDefault(b => b.Id == id);

        public (bool success, string message, ChargeBill? bill) PayChargeBill(int id)
        {
            lock (_lock)
            {
                var bill = _chargeBills.FirstOrDefault(b => b.Id == id);
                if (bill == null)
                    return (false, "收费单不存在", null);
                if (bill.Status == ChargeStatus.Paid)
                    return (false, "该收费单已结算，不能重复缴费", null);
                if (bill.Status == ChargeStatus.Cancelled)
                    return (false, "该收费单已取消，无法缴费", null);

                bill.Status = ChargeStatus.Paid;
                bill.ReceiptNo = $"REC{bill.Id:D6}{DateTime.Now:yyyyMMddHHmmss}";

                var prescription = _prescriptions.FirstOrDefault(p => p.Id == bill.PrescriptionId);
                if (prescription != null)
                    prescription.Status = PrescriptionStatus.Charged;

                return (true, "缴费成功，票据已生成", bill);
            }
        }

        public (bool success, string message, ChargeBill? bill) CancelChargeBill(int id, bool confirmed)
        {
            lock (_lock)
            {
                if (!confirmed)
                    return (false, "请确认是否取消该收费单", null);

                var bill = _chargeBills.FirstOrDefault(b => b.Id == id);
                if (bill == null)
                    return (false, "收费单不存在", null);
                if (bill.Status == ChargeStatus.Paid)
                    return (false, "该收费单已缴费，无法取消", null);
                if (bill.Status == ChargeStatus.Cancelled)
                    return (false, "该收费单已取消", null);

                bill.Status = ChargeStatus.Cancelled;
                return (true, "收费单已取消", bill);
            }
        }
        #endregion

        #region Patient Registration
        public string GetHomeIndex() => "欢迎使用医院患者挂号系统";

        public (bool success, string message, PatientReg? reg) AddRegistration(PatientReg reg)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(reg.PatientName))
                    return (false, "患者姓名不能为空", null);
                if (reg.PatientName.Length > 20 || !Regex.IsMatch(reg.PatientName, @"^[\u4e00-\u9fa5a-zA-Z]+$"))
                    return (false, "患者姓名包含非法字符或长度超限", null);
                if (string.IsNullOrWhiteSpace(reg.Phone))
                    return (false, "手机号不能为空", null);
                if (!Regex.IsMatch(reg.Phone, @"^1[3-9]\d{9}$"))
                    return (false, "手机号格式不正确", null);
                if (string.IsNullOrWhiteSpace(reg.Department))
                    return (false, "科室不能为空", null);
                if (!ValidDepartments.Contains(reg.Department))
                    return (false, $"科室 {reg.Department} 不存在，请选择有效科室", null);
                if (_patientRegs.Any(r => r.PatientName.Equals(reg.PatientName, StringComparison.OrdinalIgnoreCase)
                    && r.Department.Equals(reg.Department, StringComparison.OrdinalIgnoreCase)
                    && r.Phone == reg.Phone))
                    return (false, "该患者已在该科室挂号，请勿重复挂号", null);

                reg.Id = _patientRegs.Count > 0 ? _patientRegs.Max(r => r.Id) + 1 : 1;
                reg.CreatedAt = DateTime.Now;
                _patientRegs.Add(reg);
                return (true, "挂号成功", reg);
            }
        }

        public List<PatientReg> GetMyRegistrations(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<PatientReg>();
            return _patientRegs.Where(r => r.PatientName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<PatientReg> GetAllRegistrations() => _patientRegs.ToList();
        #endregion

        #region Medical Record
        public (bool success, string message, MedicalRecord? record) CreateMedicalRecord(int prescriptionId, string diagnosis)
        {
            lock (_lock)
            {
                var prescription = _prescriptions.FirstOrDefault(p => p.Id == prescriptionId);
                if (prescription == null)
                    return (false, "处方不存在", null);
                if (_medicalRecords.Any(r => r.PrescriptionId == prescriptionId))
                    return (false, "该处方已生成就诊记录，请勿重复生成", null);
                if (string.IsNullOrWhiteSpace(diagnosis))
                    return (false, "诊断内容不能为空", null);

                var record = new MedicalRecord
                {
                    Id = _medicalRecords.Count > 0 ? _medicalRecords.Max(r => r.Id) + 1 : 1,
                    PrescriptionId = prescriptionId,
                    PatientName = prescription.PatientName,
                    Diagnosis = diagnosis,
                    VisitTime = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                _medicalRecords.Add(record);
                return (true, "就诊记录生成成功", record);
            }
        }

        public MedicalRecord? GetMedicalRecordByPrescriptionId(int prescriptionId)
            => _medicalRecords.FirstOrDefault(r => r.PrescriptionId == prescriptionId);

        public List<MedicalRecord> GetMedicalRecordsByTime(DateTime? startTime, DateTime? endTime)
        {
            var query = _medicalRecords.AsQueryable();
            if (startTime.HasValue)
                query = query.Where(r => r.VisitTime >= startTime.Value);
            if (endTime.HasValue)
                query = query.Where(r => r.VisitTime <= endTime.Value);
            return query.ToList();
        }

        public List<MedicalRecord> GetAllMedicalRecords() => _medicalRecords.ToList();
        #endregion

        #region Report
        public (bool success, string message, Report? report) AddReport(Report report)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(report.PatientName))
                    return (false, "患者姓名不能为空", null);
                if (string.IsNullOrWhiteSpace(report.ExamItem))
                    return (false, "检查项目不能为空", null);
                if (string.IsNullOrWhiteSpace(report.Content))
                    return (false, "报告内容不能为空", null);

                report.Id = _reports.Count > 0 ? _reports.Max(r => r.Id) + 1 : 1;
                report.ReportTime = DateTime.Now;
                report.CreatedAt = DateTime.Now;
                _reports.Add(report);
                return (true, "检查报告录入成功", report);
            }
        }

        public List<Report> GetReportsByName(string? patientName)
        {
            if (string.IsNullOrWhiteSpace(patientName))
                return new List<Report>();
            return _reports.Where(r => r.PatientName.Equals(patientName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Report> GetReportsByTime(DateTime? startTime, DateTime? endTime)
        {
            var query = _reports.AsQueryable();
            if (startTime.HasValue)
                query = query.Where(r => r.ReportTime >= startTime.Value);
            if (endTime.HasValue)
                query = query.Where(r => r.ReportTime <= endTime.Value);
            return query.ToList();
        }

        public List<Report> GetAllReports() => _reports.ToList();
        #endregion
    }
}
