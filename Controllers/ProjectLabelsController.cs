using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;

namespace SWP_BE.Controllers
{
    [ApiController]
    [Authorize]
    public class ProjectLabelsController : ControllerBase
    {
        private readonly IProjectLabelService _projectLabelService;

        public ProjectLabelsController(IProjectLabelService projectLabelService)
        {
            _projectLabelService = projectLabelService;
        }

        // 1. Lấy danh sách nhãn của 1 Project
        [HttpGet("api/projects/{projectId:guid}/labels")]
        public async Task<IActionResult> GetProjectLabels(Guid projectId)
        {
            var labels = await _projectLabelService.GetLabelsByProjectIdAsync(projectId);
            return Ok(labels);
        }

        // 2. Import nhãn từ kho (nhận List các LabelID)
        [HttpPost("api/projects/{projectId:guid}/labels/import")]
        public async Task<IActionResult> ImportLabels(Guid projectId, [FromBody] ImportProjectLabelsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _projectLabelService.ImportLabelsAsync(projectId, dto);
            if (!result.success) return BadRequest(new { message = result.message });

            return Ok(new { message = result.message });
        }

        // 3. Tạo nhãn tùy chỉnh trực tiếp trong dự án
        [HttpPost("api/projects/{projectId:guid}/labels/custom")]
        public async Task<IActionResult> CreateCustomLabel(Guid projectId, [FromBody] CreateCustomProjectLabelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _projectLabelService.CreateCustomLabelAsync(projectId, dto);
            return CreatedAtAction(nameof(GetProjectLabels), new { projectId }, result);
        }

        // 4. Sửa CustomName của nhãn trong dự án
        [HttpPut("api/project-labels/{projectLabelId:int}")]
        public async Task<IActionResult> UpdateProjectLabel(int projectLabelId, [FromBody] UpdateProjectLabelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _projectLabelService.UpdateProjectLabelAsync(projectLabelId, dto);
            if (!success) return NotFound(new { message = "Nhãn dự án không tồn tại." });

            return Ok(new { message = "Cập nhật nhãn dự án thành công." });
        }

        // 5. Gỡ nhãn khỏi dự án
        [HttpDelete("api/project-labels/{projectLabelId:int}")]
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