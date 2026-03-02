using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;

namespace SWP_BE.Controllers
{
    [Route("api/labels")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập. Bạn có thể thêm Roles = "Admin,Manager" nếu chỉ cấp quản lý được sửa kho nhãn.
    public class LabelsController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelsController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        // 1. Lấy danh sách nhãn mẫu (có hỗ trợ query param ?category=xxx)
        [HttpGet]
        public async Task<IActionResult> GetLabels([FromQuery] string? category)
        {
            var labels = await _labelService.GetLabelsAsync(category);
            return Ok(labels);
        }

        // 2. Tạo nhãn mới
        [HttpPost]
        public async Task<IActionResult> CreateLabel([FromBody] CreateLabelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newLabel = await _labelService.CreateLabelAsync(dto);
            return CreatedAtAction(nameof(GetLabels), new { id = newLabel.LabelID }, newLabel);
        }

        // 3. Cập nhật nhãn
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLabel(int id, [FromBody] UpdateLabelDto dto)
        {
            var success = await _labelService.UpdateLabelAsync(id, dto);
            if (!success) return NotFound(new { message = "Nhãn không tồn tại." });

            return Ok(new { message = "Cập nhật nhãn thành công." });
        }

        // 4. Xóa nhãn mẫu
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLabel(int id)
        {
            var result = await _labelService.DeleteLabelAsync(id);

            if (!result.success)
            {
                if (result.message.Contains("không tồn tại"))
                    return NotFound(new { message = result.message });

                return BadRequest(new { message = result.message }); // Lỗi do đang bị dính ở Project
            }

            return Ok(new { message = result.message });
        }
    }
}