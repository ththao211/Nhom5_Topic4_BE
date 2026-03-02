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
    /// PHÂN HỆ: MANAGER - Quản lý Phân công và Theo dõi Task gán nhãn
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Manager")]
    [Tags("Task")]
    public class LabelingTasksController : ControllerBase
    {
        private readonly ILabelingTaskService _taskService;

        public LabelingTasksController(ILabelingTaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary> 
        /// [Role: Manager] Lấy danh sách các mục dữ liệu chưa được phân công trong dự án.
        /// </summary>
        /// <param name="id">ID của Dự án (Guid)</param>
        /// <response code="200">Trả về danh sách dữ liệu Unassigned.</response>
        /// <response code="401">Chưa đăng nhập.</response>
        [HttpGet("api/projects/{id:guid}/data-items/unassigned")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetUnassignedData(Guid id)
        {
            var data = await _taskService.GetUnassignedDataAsync(id);
            return Ok(data);
        }

        /// <summary> 
        /// [Role: Manager] Gom lô dữ liệu và tạo Task mới.
        /// </summary>
        /// <param name="id">ID của Dự án (Guid)</param>
        /// <param name="dto">Thông tin lô dữ liệu cần gom</param>
        /// <response code="200">Tạo Task thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc logic gom lô lỗi.</response>
        [HttpPost("api/projects/{id:guid}/tasks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateTask(Guid id, [FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _taskService.CreateTaskAsync(id, dto);
            if (!result.success) return BadRequest(new { message = result.message });

            return Ok(new { message = result.message, taskId = result.taskId });
        }

        /// <summary> 
        /// [Role: Manager] Giao nhân sự (Annotator và Reviewer) cho một Task cụ thể.
        /// </summary>
        /// <param name="taskId">ID của Task (Guid)</param>
        /// <param name="dto">Thông tin nhân sự được gán</param>
        /// <response code="200">Giao việc thành công.</response>
        /// <response code="404">Không tìm thấy Task.</response>
        [HttpPatch("api/tasks/{taskId:guid}/assign")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignPersonnel(Guid taskId, [FromBody] AssignTaskDto dto)
        {
            var result = await _taskService.AssignPersonnelAsync(taskId, dto);
            if (!result.success) return NotFound(new { message = result.message });

            return Ok(new { message = result.message });
        }

        /// <summary> 
        /// [Role: Manager] Theo dõi danh sách toàn bộ Task trong một dự án (Tiến độ).
        /// </summary>
        /// <param name="id">ID của Dự án (Guid)</param>
        /// <response code="200">Trả về danh sách Task kèm trạng thái (New, In-Progress, Approved...).</response>
        [HttpGet("api/projects/{id:guid}/tasks")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetProjectTasks(Guid id)
        {
            var tasks = await _taskService.GetProjectTasksAsync(id);
            return Ok(tasks);
        }

        /// <summary> 
        /// [Role: Manager] Điều chỉnh hạn chót (Deadline) cho Task đã được phân công.
        /// </summary>
        /// <param name="taskId">ID của Task (Guid)</param>
        /// <param name="dto">Thông tin Deadline mới</param>
        /// <response code="200">Cập nhật Deadline thành công.</response>
        /// <response code="404">Không tìm thấy Task để cập nhật.</response>
        [HttpPatch("api/tasks/{taskId:guid}/deadline")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateDeadline(Guid taskId, [FromBody] UpdateDeadlineDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _taskService.UpdateDeadlineAsync(taskId, dto);
            if (!result.success) return NotFound(new { message = result.message });

            return Ok(new { message = result.message });
        }
    }
}