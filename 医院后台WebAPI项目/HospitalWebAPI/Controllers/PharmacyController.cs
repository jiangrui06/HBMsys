using HospitalWebAPI.Models;
using HospitalWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWebAPI.Controllers
{
    [ApiController]
    [Route("api/Pharmacy")]
    public class PharmacyController : ControllerBase
    {
        private readonly HospitalDataService _service;

        public PharmacyController(HospitalDataService service)
        {
            _service = service;
        }

        [HttpGet("getallmedicine")]
        public IActionResult GetAllMedicine()
        {
            var list = _service.GetAllMedicines();
            return Ok(new { success = true, data = list, message = "查询成功" });
        }

        [HttpPost("addmedicine")]
        public IActionResult AddMedicine([FromBody] Medicine medicine)
        {
            var (success, message, result) = _service.AddMedicine(medicine);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpDelete("deletemedicine")]
        public IActionResult DeleteMedicine(int id, bool confirmed = false)
        {
            var (success, message) = _service.DeleteMedicine(id, confirmed);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }

        [HttpPost("addprescription")]
        public IActionResult AddPrescription([FromBody] Prescription prescription)
        {
            var (success, message, result) = _service.AddPrescription(prescription);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpGet("getprescription")]
        public IActionResult GetPrescription(int? id = null)
        {
            if (id.HasValue)
            {
                var prescription = _service.GetPrescriptionById(id.Value);
                if (prescription == null)
                    return Ok(new { success = true, data = new List<object>(), message = "未找到该处方" });
                return Ok(new { success = true, data = new List<Prescription> { prescription }, message = "查询成功" });
            }
            var list = _service.GetAllPrescriptions();
            return Ok(new { success = true, data = list, message = "查询成功" });
        }

        [HttpPut("dispense/{id}")]
        public IActionResult Dispense(int id)
        {
            var (success, message, result) = _service.DispensePrescription(id);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }
    }
}
