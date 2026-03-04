using Microsoft.AspNetCore.Mvc;
using SWP_BE.DTOs;
using SWP_BE.Services;

namespace SWP_BE.Controllers
{
    [ApiController]
    [Route("api")]
    public class AnnotatorController : ControllerBase
    {
        private readonly AnnotatorService _service;
        private readonly Guid _mockUserId = Guid.Parse("..."); // Sau này lấy từ JWT

        public AnnotatorController(AnnotatorService service) { _service = service; }

        // PART 1
        [HttpGet("annotator/tasks")] public async Task<IActionResult> GetTasks(string? status) => Ok(await _service.GetTasks(_mockUserId, status));
        [HttpGet("annotator/tasks/{taskId}")] public async Task<IActionResult> GetDetail(Guid taskId) => Ok(await _service.GetTaskDetail(taskId, _mockUserId));

        [HttpPatch("annotator/tasks/{taskId}/start")]
        public async Task<IActionResult> Start(Guid taskId)
        {
            // Logic tương tự Save: đổi status sang In-Progress
            return Ok();
        }

        // PART 2
        [HttpGet("task-items/{itemId}")] public async Task<IActionResult> GetItem(Guid itemId) => Ok(); // Get item info
        [HttpPost("task-items/{itemId}/annotation")] public async Task<IActionResult> Save(Guid itemId, SaveAnnotationDto dto) => Ok(await _service.SaveAnnotation(itemId, dto));
        [HttpPatch("task-items/{itemId}/flag")] public async Task<IActionResult> Flag(Guid itemId) => Ok(); // Set IsFlagged = true

        // PART 3
        [HttpPost("tasks/{taskId}/submit")] public async Task<IActionResult> Submit(Guid taskId) => Ok(await _service.SubmitTask(taskId, _mockUserId, false));
        [HttpPost("tasks/{taskId}/resubmit")] public async Task<IActionResult> Resubmit(Guid taskId) => Ok(await _service.SubmitTask(taskId, _mockUserId, true));

        // PART 4
        [HttpPost("tasks/{taskId}/dispute")] public async Task<IActionResult> Dispute(Guid taskId, DisputeRequestDto dto) => Ok();
        [HttpGet("annotator/reputation")] public async Task<IActionResult> GetRep() => Ok();
    }
}