using HospitalWebAPI.Models;
using HospitalWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWebAPI.Controllers
{
    [ApiController]
    [Route("api/Record")]
    public class RecordController : ControllerBase
    {
        private readonly HospitalDataService _service;

        public RecordController(HospitalDataService service)
        {
            _service = service;
        }

        [HttpPost("createrecord/{presId}")]
        public IActionResult CreateRecord(int presId, [FromBody] DiagnosisRequest request)
        {
            var (success, message, result) = _service.CreateMedicalRecord(presId, request.Diagnosis);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpGet("getrecord/bypres/{presId}")]
        public IActionResult GetRecordByPres(int presId)
        {
            var record = _service.GetMedicalRecordByPrescriptionId(presId);
            if (record == null)
                return Ok(new { success = true, data = (object?)null, message = "未找到该处方的就诊记录" });
            return Ok(new { success = true, data = record, message = "查询成功" });
        }

        [HttpGet("getrecord/bytime")]
        public IActionResult GetRecordByTime([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            var list = _service.GetMedicalRecordsByTime(startTime, endTime);
            return Ok(new { success = true, data = list, message = list.Count > 0 ? "查询成功" : "当前时间段无记录" });
        }

        [HttpGet("getrecord/all")]
        public IActionResult GetRecordAll()
        {
            var list = _service.GetAllMedicalRecords();
            return Ok(new { success = true, data = list, message = "查询成功" });
        }

        [HttpPost("addreport")]
        public IActionResult AddReport([FromBody] Report report)
        {
            var (success, message, result) = _service.AddReport(report);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpGet("getreport/byname")]
        public IActionResult GetReportByName([FromQuery] string? patientName)
        {
            var list = _service.GetReportsByName(patientName);
            return Ok(new { success = true, data = list, message = list.Count > 0 ? "查询成功" : "暂无报告" });
        }

        [HttpGet("getreport/bytime")]
        public IActionResult GetReportByTime([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            var list = _service.GetReportsByTime(startTime, endTime);
            return Ok(new { success = true, data = list, message = list.Count > 0 ? "查询成功" : "当前时间段无报告" });
        }

        [HttpGet("getreport/all")]
        public IActionResult GetReportAll()
        {
            var list = _service.GetAllReports();
            return Ok(new { success = true, data = list, message = "查询成功" });
        }
    }

    public class DiagnosisRequest
    {
        public string Diagnosis { get; set; } = string.Empty;
    }
}
