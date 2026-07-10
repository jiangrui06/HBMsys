using HospitalWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWebAPI.Controllers
{
    [ApiController]
    [Route("api/Charge")]
    public class ChargeController : ControllerBase
    {
        private readonly HospitalDataService _service;

        public ChargeController(HospitalDataService service)
        {
            _service = service;
        }

        [HttpPost("create/{prescriptionId}")]
        public IActionResult Create(int prescriptionId)
        {
            var (success, message, result) = _service.CreateChargeBill(prescriptionId);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var list = _service.GetAllChargeBills();
            return Ok(new { success = true, data = list, message = "查询成功" });
        }

        [HttpGet("getbyid/{id}")]
        public IActionResult GetById(int id)
        {
            var bill = _service.GetChargeBillById(id);
            if (bill == null)
                return Ok(new { success = true, data = (object?)null, message = "收费单不存在" });
            return Ok(new { success = true, data = bill, message = "查询成功" });
        }

        [HttpPut("pay/{id}")]
        public IActionResult Pay(int id)
        {
            var (success, message, result) = _service.PayChargeBill(id);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpPut("cancel/{id}")]
        public IActionResult Cancel(int id, bool confirmed = false)
        {
            var (success, message, result) = _service.CancelChargeBill(id, confirmed);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }
    }
}
