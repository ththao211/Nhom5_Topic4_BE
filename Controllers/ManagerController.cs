using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Services;
using System.Security.Claims;

namespace SWP_BE.Controllers
{
    [ApiController]
    [Route("api/manager/projects")]
    [Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;
        private readonly ReputationService _reputationService;
        private readonly ExportService _exportService;

        public ManagerController(
            IProjectService projectService,
            ReputationService reputationService,
            ExportService exportService,
            AppDbContext context)
        {
            _projectService = projectService;
            _reputationService = reputationService;
            _exportService = exportService;
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

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetProjects()
        {
            var managerId = GetManagerId();
            return Ok(await _projectService.GetProjectsAsync(managerId));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var managerId = GetManagerId();
            var result = await _projectService.GetProjectByIdAsync(id, managerId);
            return result == null ? NotFound("Project không tồn tại.") : Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            var managerId = GetManagerId();
            var projectId = await _projectService.CreateProjectAsync(dto, managerId);
            return Ok(new { message = "Project created successfully", projectId = projectId });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectDto dto)
        {
            var managerId = GetManagerId();
            var success = await _projectService.UpdateProjectAsync(id, dto, managerId);
            return success ? Ok(new { message = "Updated" }) : NotFound();
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] string status)
        {
            var managerId = GetManagerId();
            var success = await _projectService.ChangeStatusAsync(id, status, managerId);
            return success ? Ok(new { message = "Status updated" }) : NotFound();
        }

        [HttpPost("{id}/guideline")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateGuideline(Guid id, [FromQuery] string url)
        {
            var managerId = GetManagerId();
            var success = await _projectService.UpdateGuidelineAsync(id, url, managerId);
            return success ? Ok(new { message = "Guideline updated" }) : NotFound();
        }

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

            var totalTasks = project.Tasks?.Count ?? 0;
            var totalDataItems = project.DataItems?.Count ?? 0;

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

        [HttpGet("{projectId}/disputes")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetDisputes(Guid projectId)
        {
            var managerId = GetManagerId();

            var disputes = await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Where(d =>
                    d.Task.ProjectID == projectId &&
                    d.Task.Project.ManagerID == managerId)
                .Select(d => new
                {
                    d.DisputeID,
                    TaskName = d.Task.TaskName,
                    ProjectName = d.Task.Project.ProjectName,
                })
                .OrderByDescending(d => d.DisputeID)
                .ToListAsync();

            return Ok(disputes);
        }

        [HttpGet("disputes")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllDisputesForDashboard()
        {
            var managerId = GetManagerId();

            var disputes = await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Where(d => d.Task.Project.ManagerID == managerId)
                .Select(d => new
                {
                    d.DisputeID,
                    TaskName = d.Task.TaskName,
                    ProjectName = d.Task.Project.ProjectName,
                    Status = d.Status,
                    d.CreatedAt
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(disputes);
        }

        [HttpGet("disputes/{disputeId}")]
        public async Task<IActionResult> GetDisputeDetail(Guid disputeId)
        {
            var managerId = GetManagerId();

            var dispute = await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d =>
                    d.DisputeID == disputeId &&
                    d.Task.Project.ManagerID == managerId);

            if (dispute == null)
                return NotFound();

            var evidenceImages = await _context.ReviewComments
                .Where(rc => rc.ReviewHistory.TaskID == dispute.TaskID
                          && rc.ReviewHistory.FinalResult == "DisputeEvidence")
                .OrderByDescending(rc => rc.CreatedAt)
                .Select(rc => rc.EvidenceImages)
                .FirstOrDefaultAsync();

            var images = string.IsNullOrEmpty(evidenceImages)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(evidenceImages);

            return Ok(new
            {
                dispute.DisputeID,
                TaskName = dispute.Task.TaskName,
                ProjectName = dispute.Task.Project.ProjectName,
                Annotator = dispute.User.FullName,
                dispute.Reason,
                dispute.Status,
                EvidenceImages = images,
                dispute.CreatedAt
            });
        }

        [HttpPatch("disputes/{disputeId}")]
        public async Task<IActionResult> ResolveDispute(Guid disputeId, [FromQuery] string action, [FromBody] string managerComment)
        {
            var managerId = GetManagerId();

            var dispute = await _context.Disputes
                .Include(d => d.Task)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DisputeID == disputeId && d.Task.Project.ManagerID == managerId);

            if (dispute == null) return NotFound("Dispute không tồn tại.");
            if (dispute.Status != "Pending") return BadRequest("Dispute đã được xử lý.");

            dispute.ManagerComment = managerComment;

            if (action == "accept")
            {
                dispute.Status = "Accepted";

                if (dispute.Task.ReviewerID.HasValue)
                {
                    await _reputationService.HandleReviewerDisputeLossAsync(dispute.Task.ReviewerID.Value, dispute.Task.TaskID);
                }
                dispute.Task.Status = SWP_BE.Models.Task.TaskStatus.Approved;

                if (dispute.Task.AnnotatorID.HasValue)
                {
                    await _reputationService.HandleTaskCompletionAsync(dispute.Task.AnnotatorID.Value, dispute.Task);
                }
            }
            else if (action == "reject")
            {
                dispute.Status = "Rejected";
                await _reputationService.HandleAnnotatorDisputeLossAsync(dispute.UserID, dispute.TaskID);
                dispute.Task.Status = SWP_BE.Models.Task.TaskStatus.Rejected;
            }
            else
            {
                return BadRequest("Action phải là accept hoặc reject.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã phân xử khiếu nại thành công." });
        }

        [HttpGet("yolo/{projectId}")]
        public async Task<IActionResult> ExportYolo(Guid projectId)
        {
            try
            {
                var (fileBytes, fileName) = await _exportService.ExportYoloZipAsync(projectId);
                return File(fileBytes, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Export YOLO thất bại", error = ex.Message });
            }
        }

        [HttpGet("coco/{projectId}")]
        public async Task<IActionResult> ExportCoco(Guid projectId)
        {
            try
            {
                var (fileBytes, fileName) = await _exportService.ExportCocoFileAsync(projectId);
                return File(fileBytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Export COCO thất bại", error = ex.Message });
            }
        }
        [Authorize(Roles = "Manager")]
        [HttpGet("projects/{projectId}/missing-label-reports")]
        public async Task<IActionResult> GetMissingLabelReports(Guid projectId)
        {
            var managerId = GetManagerId();

            var reports = await _context.Disputes
                .Include(d => d.User)
                .Include(d => d.Task)
                .Where(d =>
                    d.Task.ProjectID == projectId &&
                    d.Task.Project.ManagerID == managerId)
                .Select(d => new
                {
                    d.DisputeID,
                    TaskName = d.Task.TaskName,
                    ProjectName = d.Task.Project.ProjectName,
                    Status = d.Status,
                    d.CreatedAt
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(reports);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("missing-label-evidence/{taskId}")]
        public async Task<IActionResult> GetMissingLabelEvidence(Guid disputeLabelId)
        { 
            var managerId = GetManagerId();

            var dispute = await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d =>
                    d.DisputeID == disputeLabelId &&
                    d.Task.Project.ManagerID == managerId);

            if (dispute == null)
                return NotFound();

            var evidenceImages = await _context.ReviewComments
                .Where(rc => rc.ReviewHistory.TaskID == dispute.TaskID
                          && rc.ReviewHistory.FinalResult == "DisputeEvidence")
                .OrderByDescending(rc => rc.CreatedAt)
                .Select(rc => rc.EvidenceImages)
                .FirstOrDefaultAsync();

            var images = string.IsNullOrEmpty(evidenceImages)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(evidenceImages);

            return Ok(new
            {
                dispute.DisputeID,
                TaskName = dispute.Task.TaskName,
                ProjectName = dispute.Task.Project.ProjectName,
                Annotator = dispute.User.FullName,
                dispute.Reason,
                dispute.Status,
                EvidenceImages = images,
                dispute.CreatedAt
            });
        }
    }
}