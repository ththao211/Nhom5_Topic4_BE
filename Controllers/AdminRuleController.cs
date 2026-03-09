using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs; // Đảm bảo bạn đã có các DTO này
using SWP_BE.Services;

namespace SWP_BE.Controllers
{
    [ApiController]
    [Route("api/admin/rules")]
    public class AdminRuleController : ControllerBase
    {
        private readonly ReputationService _service;

        public AdminRuleController(ReputationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRules()
        {
            var rules = await _service.GetAllRulesForAdminAsync();
            return Ok(rules);
        }

        [HttpPut("{ruleId}")]
        public async Task<IActionResult> UpdateRule(int ruleId, [FromBody] UpdateRuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu gửi lên không hợp lệ." });

            var result = await _service.UpdateRuleAsync(ruleId, dto);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}