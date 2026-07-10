using HospitalWebAPI.Models;
using HospitalWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWebAPI.Controllers
{
    [ApiController]
    [Route("api/Patient")]
    public class PatientController : ControllerBase
    {
        private readonly HospitalDataService _service;

        public PatientController(HospitalDataService service)
        {
            _service = service;
        }

        [HttpGet("homeindex")]
        public IActionResult HomeIndex()
        {
            return Ok(new { success = true, data = _service.GetHomeIndex(), message = "获取成功" });
        }

        [HttpPost("addreg")]
        public IActionResult AddReg([FromBody] PatientReg reg)
        {
            var (success, message, result) = _service.AddRegistration(reg);
            if (!success)
                return BadRequest(new { success = false, message });
            return Ok(new { success = true, data = result, message });
        }

        [HttpGet("myreg")]
        public IActionResult MyReg([FromQuery] string? name)
        {
            var list = _service.GetMyRegistrations(name);
            return Ok(new { success = true, data = list, message = list.Count > 0 ? "查询成功" : "暂无挂号记录" });
        }

        [HttpGet("allreg")]
        public IActionResult AllReg()
        {
            var list = _service.GetAllRegistrations();
            return Ok(new { success = true, data = list, message = "查询成功" });
        }
    }
}
