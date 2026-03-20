using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;

namespace SWP_BE.Repositories
{
    public interface IReviewerRepository
    {
        Task<IEnumerable<ReviewerDisputeDto>> GetReviewerDisputes(Guid reviewerId);
    }
    public class ReviewerRepository : IReviewerRepository
    {
        private readonly AppDbContext _context;

        public ReviewerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewerDisputeDto>> GetReviewerDisputes(Guid reviewerId)
        {
            return await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Where(d => d.Task.ReviewerID == reviewerId)
                .Select(d => new ReviewerDisputeDto
                {
                    DisputeID = d.DisputeID,
                    TaskName = d.Task.TaskName,
                    ProjectName = d.Task.Project.ProjectName,
                    EvidenceImages = new List<string>() 
                })
                .ToListAsync();
        }
    }

}
