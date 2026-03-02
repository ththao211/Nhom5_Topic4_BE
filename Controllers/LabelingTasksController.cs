using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;

namespace SWP_BE.Controllers
{
    [ApiController]
    //[Authorize] // Có thể thêm [Authorize(Roles = "Manager,Admin")] tùy luồng hệ thống
    public class LabelingTasksController : ControllerBase
    {
        private readonly ILabelingTaskService _taskService;

        public LabelingTasksController(ILabelingTaskService taskService)
        {
            _taskService = taskService;
        }

        // 1. Lấy dữ liệu chưa phân công
        [HttpGet("api/projects/{id:guid}/data-items/unassigned")]
        public async Task<IActionResult> GetUnassignedData(Guid id)
        {
            var data = await _taskService.GetUnassignedDataAsync(id);
            return Ok(data);
        }

        // 2. Gom lô & Tạo Task
        [HttpPost("api/projects/{id:guid}/tasks")]
        public async Task<IActionResult> CreateTask(Guid id, [FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _taskService.CreateTaskAsync(id, dto);
            if (!result.success) return BadRequest(new { message = result.message });

            return Ok(new { message = result.message, taskId = result.taskId });
        }

        // 3. Giao nhân sự
        [HttpPatch("api/tasks/{taskId:guid}/assign")]
        public async Task<IActionResult> AssignPersonnel(Guid taskId, [FromBody] AssignTaskDto dto)
        {
            var result = await _taskService.AssignPersonnelAsync(taskId, dto);
            if (!result.success) return NotFound(new { message = result.message });

            return Ok(new { message = result.message });
        }

        // 4. Theo dõi tiến độ dự án (Danh sách Tasks)
        [HttpGet("api/projects/{id:guid}/tasks")]
        public async Task<IActionResult> GetProjectTasks(Guid id)
        {
            var tasks = await _taskService.GetProjectTasksAsync(id);
            return Ok(tasks);
        }

        // 5. Điều chỉnh hạn chót
        [HttpPatch("api/tasks/{taskId:guid}/deadline")]
        public async Task<IActionResult> UpdateDeadline(Guid taskId, [FromBody] UpdateDeadlineDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _taskService.UpdateDeadlineAsync(taskId, dto);
            if (!result.success) return NotFound(new { message = result.message });

            return Ok(new { message = result.message });
        }
    }
}