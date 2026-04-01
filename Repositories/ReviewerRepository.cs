using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.DTOs;
using SWP_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            // Bước 1: Kéo dữ liệu từ DB (Bao gồm cả Reason, Status, CreatedAt, TaskID)
            var rawData = await _context.Disputes
                .Include(d => d.Task)
                    .ThenInclude(t => t.Project)
                .Where(d => d.Task.ReviewerID == reviewerId &&
                 d.Status == "Pending")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    DisputeID = d.DisputeID,
                    TaskID = d.TaskID,
                    TaskName = d.Task.TaskName,
                    ProjectName = d.Task.Project.ProjectName,
                    Reason = d.Reason,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,

                    EvidenceString = _context.ReviewComments
                        .Where(rc => rc.ReviewHistory.TaskID == d.TaskID
                                  && rc.ReviewHistory.FinalResult == "DisputeEvidence")
                        .OrderByDescending(rc => rc.CreatedAt)
                        .Select(rc => rc.EvidenceImages)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Bước 2: Map dữ liệu thô sang ReviewerDisputeDto chuẩn
            return rawData.Select(d => new ReviewerDisputeDto
            {
                DisputeID = d.DisputeID,
                TaskID = d.TaskID,
                TaskName = d.TaskName,
                ProjectName = d.ProjectName,
                Reason = d.Reason,
                Status = d.Status.ToString(),        // Chuyển Enum thành chữ (Pending, Approved...)
                CreatedAt = d.CreatedAt,

                // Tách hoặc bọc chuỗi ảnh vào List
                EvidenceImages = string.IsNullOrEmpty(d.EvidenceString)
                    ? new List<string>()
                    : new List<string> { d.EvidenceString }
            });
        }
    }
}