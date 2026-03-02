using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP_BE.Controllers
{
    /// <summary>
    /// PHÂN HỆ: MANAGER - Quản lý Dự án gán nhãn
    /// </summary>
    [ApiController]
    [Route("api/manager/projects")]
    [Authorize(Roles = "Manager")] // Yêu cầu quyền Manager để truy cập
    public class ManagerController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ManagerController(IProjectService projectService) { _projectService = projectService; }

        private Guid GetManagerId()
        {
            var claim = User.FindFirst("id");
            if (claim == null) throw new UnauthorizedAccessException("Phiên đăng nhập hết hạn hoặc thiếu ID.");
            return Guid.Parse(claim.Value);
        }

        /// <summary> 
        /// [Role: Manager] Lấy danh sách toàn bộ dự án mà Manager này đang quản lý. 
        /// </summary>
        /// <response code="200">Trả về danh sách các dự án.</response>
        /// <response code="401">Chưa đăng nhập.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetProjects()
        {
            var managerId = GetManagerId();
            return Ok(await _projectService.GetProjectsAsync(managerId));
        }

        /// <summary> 
        /// [Role: Manager] Lấy thông tin chi tiết của một dự án cụ thể. 
        /// </summary>
        /// <param name="id">ID của dự án (Guid)</param>
        /// <response code="200">Trả về chi tiết dự án.</response>
        /// <response code="404">Dự án không tồn tại hoặc không thuộc quyền quản lý của bạn.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var managerId = GetManagerId();
            var result = await _projectService.GetProjectByIdAsync(id, managerId);
            return result == null ? NotFound("Project không tồn tại.") : Ok(result);
        }

        /// <summary> 
        /// [Role: Manager] Tạo mới một dự án gán nhãn. 
        /// </summary>
        /// <remarks>
        /// Chức năng này bao gồm thiết lập tên, mô tả và loại dữ liệu cho dự án.
        /// </remarks>
        /// <param name="dto">Thông tin dự án mới</param>
        /// <response code="200">Tạo dự án thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            var managerId = GetManagerId();
            var projectId = await _projectService.CreateProjectAsync(dto, managerId);
            return Ok(new { message = "Project created successfully", projectId = projectId });
        }

        /// <summary> 
        /// [Role: Manager] Cập nhật thông tin cơ bản của dự án. 
        /// </summary>
        /// <param name="id">ID của dự án (Guid)</param>
        /// <param name="dto">Thông tin cập nhật</param>
        /// <response code="200">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy dự án.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectDto dto)
        {
            var managerId = GetManagerId();
            var success = await _projectService.UpdateProjectAsync(id, dto, managerId);
            return success ? Ok(new { message = "Updated" }) : NotFound();
        }

        /// <summary> 
        /// [Role: Manager] Thay đổi trạng thái hoạt động của dự án. 
        /// </summary>
        /// <param name="id">ID dự án (Guid)</param>
        /// <param name="status">Trạng thái mới (Ví dụ: Active, Deactive, Closed)</param>
        /// <response code="200">Cập nhật trạng thái thành công.</response>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] string status)
        {
            var managerId = GetManagerId();
            var success = await _projectService.ChangeStatusAsync(id, status, managerId);
            return success ? Ok(new { message = "Status updated" }) : NotFound();
        }

        /// <summary> 
        /// [Role: Manager] Cập nhật đường dẫn file hướng dẫn (Guideline) cho dự án. 
        /// </summary>
        /// <param name="id">ID dự án (Guid)</param>
        /// <param name="url">URL của file guideline mới (Firebase/Cloud URL)</param>
        [HttpPost("{id}/guideline")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateGuideline(Guid id, [FromQuery] string url)
        {
            var managerId = GetManagerId();
            var success = await _projectService.UpdateGuidelineAsync(id, url, managerId);
            return success ? Ok(new { message = "Guideline updated" }) : NotFound();
        }

        /// <summary> 
        /// [Role: Manager] Tải lên danh sách liên kết dữ liệu thô vào dự án. 
        /// </summary>
        /// <remarks>
        /// Dữ liệu này sẽ ở trạng thái "Unassigned" cho đến khi được phân công vào Task.
        /// </remarks>
        /// <param name="id">ID dự án (Guid)</param>
        /// <param name="dto">Danh sách URLs dữ liệu</param>
        /// <response code="200">Thêm dữ liệu thành công.</response>
        /// <response code="400">Danh sách links trống.</response>
        [HttpPost("{id}/data")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadData(Guid id, [FromBody] UploadDataDto dto)
        {
            if (dto.FileUrls == null || !dto.FileUrls.Any()) return BadRequest("Links không được để trống.");
            var managerId = GetManagerId();
            var success = await _projectService.UploadDataAsync(id, dto, managerId);
            return success ? Ok(new { message = "Data added" }) : NotFound();
        }

        /// <summary> 
        /// [Role: Manager] Chia nhỏ dữ liệu thành các Task (Phân lô dữ liệu). 
        /// </summary>
        /// <param name="id">ID dự án (Guid)</param>
        /// <param name="dto">Cấu hình chia task</param>
        /// <response code="200">Chia task thành công.</response>
        /// <response code="400">Lỗi nghiệp vụ khi chia task.</response>
        [HttpPost("{id}/split-tasks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SplitTasks(Guid id, [FromBody] SplitTaskDto dto)
        {
            var managerId = GetManagerId();
            try
            {
                var result = await _projectService.SplitTasksAsync(id, dto, managerId);
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}