using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP_BE.Controllers
{
    /// <summary>
    /// PHÂN HỆ: MANAGER - Quản lý Nhãn (Labels) cụ thể cho từng dự án
    /// </summary>
    [ApiController]
    [Route("api/project-labels")]
    [Authorize(Roles = "Manager")] // Chỉ Manager mới có quyền thay đổi bộ nhãn của dự án
    public class ProjectLabelsController : ControllerBase
    {
        private readonly IProjectLabelService _projectLabelService;

        public ProjectLabelsController(IProjectLabelService projectLabelService)
        {
            _projectLabelService = projectLabelService;
        }

        /// <summary> 
        /// [Role: Manager/Annotator/Reviewer] Lấy danh sách toàn bộ nhãn của một Project cụ thể.
        /// </summary>
        /// <param name="projectId">ID của dự án (Guid)</param>
        /// <response code="200">Trả về danh sách nhãn của dự án.</response>
        [HttpGet("api/projects/{projectId:guid}/labels")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetProjectLabels(Guid projectId)
        {
            var labels = await _projectLabelService.GetLabelsByProjectIdAsync(projectId);
            return Ok(labels);
        }

        /// <summary> 
        /// [Role: Manager] Import danh sách nhãn từ Kho nhãn mẫu (Library) vào dự án.
        /// </summary>
        /// <remarks>
        /// Nhận vào danh sách LabelID từ Kho nhãn để áp dụng cho dự án hiện tại.
        /// </remarks>
        /// <param name="projectId">ID dự án (Guid)</param>
        /// <param name="dto">Danh sách ID các nhãn cần import</param>
        /// <response code="200">Import thành công.</response>
        /// <response code="400">Lỗi dữ liệu hoặc nhãn đã tồn tại trong dự án.</response>
        [HttpPost("api/projects/{projectId:guid}/labels/import")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ImportLabels(Guid projectId, [FromBody] ImportProjectLabelsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _projectLabelService.ImportLabelsAsync(projectId, dto);
            if (!result.success) return BadRequest(new { message = result.message });

            return Ok(new { message = result.message });
        }

        /// <summary> 
        /// [Role: Manager] Tạo nhãn tùy chỉnh (Custom Label) chỉ dành riêng cho dự án này.
        /// </summary>
        /// <remarks>
        /// Dùng khi dự án cần những nhãn đặc thù không có sẵn trong Kho nhãn mẫu.
        /// </remarks>
        /// <param name="projectId">ID dự án (Guid)</param>
        /// <param name="dto">Thông tin nhãn tùy chỉnh</param>
        /// <response code="201">Tạo thành công nhãn tùy chỉnh.</response>
        [HttpPost("api/projects/{projectId:guid}/labels/custom")]
        [ProducesResponseType(201)]
        public async Task<IActionResult> CreateCustomLabel(Guid projectId, [FromBody] CreateCustomProjectLabelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _projectLabelService.CreateCustomLabelAsync(projectId, dto);
            return CreatedAtAction(nameof(GetProjectLabels), new { projectId }, result);
        }

        /// <summary> 
        /// [Role: Manager] Cập nhật tên hiển thị (CustomName) của nhãn trong dự án.
        /// </summary>
        /// <param name="projectLabelId">ID liên kết giữa Project và Label (int)</param>
        /// <param name="dto">Nội dung cập nhật mới</param>
        /// <response code="200">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy nhãn trong dự án.</response>
        [HttpPut("{projectLabelId:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProjectLabel(int projectLabelId, [FromBody] UpdateProjectLabelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _projectLabelService.UpdateProjectLabelAsync(projectLabelId, dto);
            if (!success) return NotFound(new { message = "Nhãn dự án không tồn tại." });

            return Ok(new { message = "Cập nhật nhãn dự án thành công." });
        }

        /// <summary> 
        /// [Role: Manager] Gỡ bỏ một nhãn khỏi dự án.
        /// </summary>
        /// <param name="projectLabelId">ID liên kết cần xóa (int)</param>
        /// <response code="200">Gỡ nhãn thành công.</response>
        /// <response code="404">Nhãn không tồn tại trong dự án.</response>
        [HttpDelete("{projectLabelId:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProjectLabel(int projectLabelId)
        {
            var result = await _projectLabelService.DeleteProjectLabelAsync(projectLabelId);

            if (!result.success)
            {
                return NotFound(new { message = result.message });
            }

            return Ok(new { message = result.message });
        }
    }
}