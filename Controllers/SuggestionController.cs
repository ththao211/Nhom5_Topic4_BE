using Microsoft.AspNetCore.Mvc;
using SWP_BE.Services;

namespace SWP_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionController : ControllerBase
    {
        private readonly SuggestionService _service;
        public SuggestionController(SuggestionService service) => _service = service;

        [HttpGet("annotators/{projectId}")]
        public async Task<IActionResult> GetAnnotators(Guid projectId)
        {
            var result = await _service.GetAnnotatorSuggestions(projectId);
            return Ok(result);
        }

        [HttpGet("reviewers/{projectId}")]
        public async Task<IActionResult> GetReviewers(Guid projectId)
        {
            var result = await _service.GetReviewerSuggestions(projectId);
            return Ok(result);
        }
    }
}