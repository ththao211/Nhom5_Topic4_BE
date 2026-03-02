using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP_BE.Controllers
{
    /// <summary>
    /// PHÂN HỆ: MANAGER - Quản lý Kho nhãn hệ thống (Master Data)
    /// </summary>
    [Route("api/manager/labels")]
    [ApiController]
    [Authorize(Roles = "Manager")] // Chỉ Manager mới có quyền truy cập
    public class ManagerLabelsController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public ManagerLabelsController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        /// <summary> 
        /// [Role: Manager] Lấy danh sách nhãn mẫu từ kho chung.
        /// </summary>
        /// <param name="category">Lọc theo danh mục (Ví dụ: Giao thông, Y tế)</param>
        /// <response code="200">Thành công</response>
        /// <response code="401">Chưa đăng nhập</response>
        /// <response code="403">Không có quyền Manager</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LabelDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetLabels([FromQuery] string? category)
        {
            var labels = await _labelService.GetLabelsAsync(category);
            return Ok(labels);
        }

        /// <summary> 
        /// [Role: Manager] Thêm một nhãn mới vào thư viện nhãn của hệ thống.
        /// </summary>
        /// <remarks>
        /// Ghi chú: Nhãn này sẽ xuất hiện trong danh sách gợi ý khi Manager tạo dự án mới.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(LabelDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateLabel([FromBody] CreateLabelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newLabel = await _labelService.CreateLabelAsync(dto);
            return CreatedAtAction(nameof(GetLabels), new { id = newLabel.LabelID }, newLabel);
        }

        /// <summary> 
        /// [Role: Manager] Cập nhật thông tin (Tên, Màu sắc) của nhãn trong kho.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateLabel(int id, [FromBody] UpdateLabelDto dto)
        {
            var success = await _labelService.UpdateLabelAsync(id, dto);
            if (!success) return NotFound(new { message = "Không tìm thấy nhãn để cập nhật." });

            return Ok(new { message = "Cập nhật thành công." });
        }

        /// <summary> 
        /// [Role: Manager] Xóa nhãn khỏi kho mẫu.
        /// </summary>
        /// <remarks>
        /// Điều kiện: Chỉ xóa được nếu nhãn này chưa từng được sử dụng trong bất kỳ dự án nào.
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteLabel(int id)
        {
            var result = await _labelService.DeleteLabelAsync(id);
            if (!result.success) return BadRequest(new { message = result.message });

            return Ok(new { message = "Xóa nhãn thành công." });
        }
    }
}