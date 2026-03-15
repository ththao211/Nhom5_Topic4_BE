using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Services;
using System.Security.Claims;


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
        private readonly AppDbContext _context;

        public ManagerController(
            IProjectService projectService,
            AppDbContext context)
        {
            _projectService = projectService;
            _context = context;
        }

        private Guid GetManagerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ hoặc thiếu ID người dùng.");
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
        /// [Role: Manager] Lấy thông tin tổng quan (overview) của dự án để hiển thị Dashboard. 
        /// </summary>
        /// <remarks>
        /// API này trả về dữ liệu tổng hợp bao gồm: thông tin dự án, danh sách nhãn (kèm màu sắc), danh sách các task, và thống kê tiến độ hoàn thành.
        /// </remarks>
        /// <param name="projectId">ID của dự án cần xem tổng quan (Guid)</param>
        /// <response code="200">Trả về dữ liệu tổng quan của dự án.</response>
        /// <response code="404">Dự án không tồn tại hoặc không thuộc quyền quản lý của bạn.</response>
        [HttpGet("{projectId}/overview")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProjectOverview(Guid projectId)
        {
            var managerId = GetManagerId();

            var project = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.DataItems)
                .Include(p => p.ProjectLabels)
                    .ThenInclude(pl => pl.Label)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Annotator)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Reviewer)
                .FirstOrDefaultAsync(p =>
                    p.ProjectID == projectId &&
                    p.ManagerID == managerId);

            if (project == null)
                return NotFound("Project không tồn tại hoặc không thuộc quyền của bạn.");

            // Statistics
            var totalTasks = project.Tasks?.Count ?? 0;
            var totalDataItems = project.DataItems?.Count ?? 0;

            // FIX: Chỉ sử dụng các trạng thái chắc chắn có trong Enum TaskStatus của bạn
            var completedTasks = project.Tasks?
                .Count(t => t.Status == SWP_BE.Models.Task.TaskStatus.Approved) ?? 0;

            var inProgressTasks = project.Tasks?
                .Count(t =>
                    t.Status == SWP_BE.Models.Task.TaskStatus.InProgress ||
                    t.Status == SWP_BE.Models.Task.TaskStatus.PendingReview) ?? 0;

            var result = new
            {
                project = new
                {
                    project.ProjectID,
                    project.ProjectName,
                    project.Description,
                    project.Topic,
                    Status = project.Status.ToString(),
                    project.ProjectType,
                    project.Deadline,
                    project.GuidelineUrl,
                    project.CreatedAt
                },

                manager = project.Manager == null ? null : new
                {
                    project.Manager.UserID,
                    project.Manager.FullName,
                    project.Manager.Email
                },

                totalDataItems,

                // Lấy đúng màu sắc để Frontend vẽ Overview (như Dashboard Manager)
                labels = project.ProjectLabels?.Select(pl => new
                {
                    pl.ProjectLabelID,
                    pl.LabelID,
                    LabelName = pl.Label?.LabelName,
                    pl.CustomName,
                    Color = pl.Label?.DefaultColor ?? "#ffffff"
                }),

                tasks = project.Tasks?.Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    Status = t.Status.ToString(),
                    t.Deadline,
                    t.RateComplete,

                    annotator = t.Annotator == null ? null : new
                    {
                        t.Annotator.UserID,
                        t.Annotator.FullName,
                        t.Annotator.Score
                    },

                    reviewer = t.Reviewer == null ? null : new
                    {
                        t.Reviewer.UserID,
                        t.Reviewer.FullName,
                        t.Reviewer.Score
                    }
                }),

                statistics = new
                {
                    totalTasks,
                    completedTasks,
                    inProgressTasks,
                    progressPercentage = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0
                }
            };

            return Ok(result);
        }
    }
}