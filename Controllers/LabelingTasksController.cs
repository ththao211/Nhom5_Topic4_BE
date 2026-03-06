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
    /// PHÂN HỆ: MANAGER - Quản lý Phân công và Theo dõi Task gán nhãn.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Manager")]
    [Tags("Task")]
    public class LabelingTasksController : ControllerBase
    {
        private readonly ILabelingTaskService _taskService;
        private readonly IEmailService _emailService;

        public LabelingTasksController(ILabelingTaskService taskService, IEmailService emailService)
        {
            _taskService = taskService;
            _emailService = emailService;
        }

        /// <summary> 
        /// [Role: Manager] Lấy danh sách Annotator kèm điểm (Score) và kinh nghiệm (Expertise) để chọn người giao task.
        /// </summary>
        /// <returns>Danh sách nhân sự có quyền Annotator.</returns>
        /// <response code="200">Trả về danh sách Annotator đang hoạt động.</response>
        [HttpGet("api/tasks/available-annotators")]
        [ProducesResponseType(typeof(IEnumerable<UserBasicDto>), 200)]
        public async Task<IActionResult> GetAvailableAnnotators()
        {
            var annotators = await _taskService.GetUsersByRoleAsync("Annotator");
            return Ok(annotators);
        }

        /// <summary> 
        /// [Role: Manager] Lấy danh sách Reviewer kèm điểm (Score) và kinh nghiệm (Expertise) để chọn người giao task.
        /// </summary>
        /// <returns>Danh sách nhân sự có quyền Reviewer.</returns>
        /// <response code="200">Trả về danh sách Reviewer đang hoạt động.</response>
        [HttpGet("api/tasks/available-reviewers")]
        [ProducesResponseType(typeof(IEnumerable<UserBasicDto>), 200)]
        public async Task<IActionResult> GetAvailableReviewers()
        {
            var reviewers = await _taskService.GetUsersByRoleAsync("Reviewer");
            return Ok(reviewers);
        }

        /// <summary> 
        /// [Role: Manager] Lấy danh sách các mục dữ liệu chưa được phân công trong dự án.
        /// </summary>
        /// <param name="id">ID của Dự án (Guid).</param>
        /// <returns>Danh sách dữ liệu thô chưa thuộc Task nào.</returns>
        /// <response code="200">Trả về danh sách dữ liệu Unassigned.</response>
        [HttpGet("api/projects/{id:guid}/data-items/unassigned")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetUnassignedData(Guid id)
        {
            var data = await _taskService.GetUnassignedDataAsync(id);
            return Ok(data);
        }

        /// <summary> 
        /// [Role: Manager] Gom lô dữ liệu (ví dụ 10-20 ảnh) và tạo thành một Task mới.
        /// </summary>
        /// <param name="id">ID của Dự án (Guid).</param>
        /// <param name="dto">Thông tin Task: Tên, danh sách DataIDs, và Deadline.</param>
        /// <returns>ID của Task vừa được tạo.</returns>
        /// <response code="200">Tạo Task thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc dữ liệu đã được gán cho task khác.</response>
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
        /// [Role: Manager] Giao nhân sự (Annotator/Reviewer) cho Task và TỰ ĐỘNG gửi mail thông báo.
        /// </summary>
        /// <param name="taskId">ID của Task (Guid).</param>
        /// <param name="dto">ID của Annotator và Reviewer được chọn.</param>
        /// <returns>Thông báo trạng thái giao việc.</returns>
        /// <response code="200">Giao nhân sự thành công, mail đang được gửi ngầm.</response>
        /// <response code="404">Không tìm thấy Task.</response>
        [HttpPatch("api/tasks/{taskId:guid}/assign")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignPersonnel(Guid taskId, [FromBody] AssignTaskDto dto)
        {
            var result = await _taskService.AssignPersonnelAsync(taskId, dto);
            if (!result.success || result.taskDetails == null) return NotFound(new { message = result.message });

            _ = Task.Run(async () =>
            {
                try
                {
                    var task = result.taskDetails;
                    var deadline = task.Deadline.ToString("dd/MM/yyyy HH:mm");
                    if (task.Annotator != null)
                        await _emailService.SendTaskAssignmentEmailAsync(task.Annotator.Email, task.Annotator.FullName, task.TaskName, task.Project?.ProjectName, deadline);
                    if (task.Reviewer != null)
                        await _emailService.SendTaskAssignmentEmailAsync(task.Reviewer.Email, task.Reviewer.FullName, task.TaskName, task.Project?.ProjectName, deadline);
                }
                catch (Exception ex) { Console.WriteLine($"[Email Error]: {ex.Message}"); }
            });

            return Ok(new { message = result.message + " và đang gửi thông báo tới Gmail!" });
        }

        /// <summary> 
        /// [Role: Manager] Theo dõi danh sách toàn bộ Task trong dự án kèm tiến độ và người phụ trách.
        /// </summary>
        /// <param name="id">ID của Dự án (Guid).</param>
        /// <returns>Danh sách Task với thông tin % hoàn thành, tên Annotator/Reviewer.</returns>
        /// <response code="200">Trả về danh sách Task thành công.</response>
        [HttpGet("api/projects/{id:guid}/tasks")]
        [ProducesResponseType(typeof(IEnumerable<TaskProgressDto>), 200)]
        public async Task<IActionResult> GetProjectTasks(Guid id)
        {
            var tasks = await _taskService.GetProjectTasksAsync(id);
            return Ok(tasks);
        }

        /// <summary> 
        /// [Role: Manager] Điều chỉnh hạn chót (Deadline) cho một Task cụ thể.
        /// </summary>
        /// <param name="taskId">ID của Task (Guid).</param>
        /// <param name="dto">Hạn chót mới.</param>
        /// <returns>Thông báo kết quả cập nhật.</returns>
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