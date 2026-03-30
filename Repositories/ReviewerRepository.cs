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

        public async Task<IEnumerable<object>> GetReviewerDisputes(Guid reviewerId)
        {
            return await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Where(d => d.Task.ReviewerID == reviewerId)
                .Select(d => new
                {
                    d.DisputeID,
                    TaskName = d.Task.TaskName,
                    ProjectName = d.Task.Project.ProjectName,
                    ManagerComment = d.ManagerComment,

                    EvidenceImages = _context.ReviewComments
                        .Where(rc => rc.ReviewHistory.TaskID == d.TaskID
                                  && rc.ReviewHistory.FinalResult == "DisputeEvidence")
                        .OrderByDescending(rc => rc.CreatedAt)
                        .Select(rc => rc.EvidenceImages)
                        .FirstOrDefault()
                })
                .ToListAsync<object>();
        }

        Task<IEnumerable<ReviewerDisputeDto>> IReviewerRepository.GetReviewerDisputes(Guid reviewerId)
        {
            throw new NotImplementedException();
        }
    }

}
