using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using Task = System.Threading.Tasks.Task;

namespace SWP_BE.Repositories
{
    public interface IProjectLabelRepository
    {
        Task<IEnumerable<ProjectLabel>> GetByProjectIdAsync(Guid projectId);
        Task<ProjectLabel?> GetByIdAsync(int projectLabelId);
        Task<bool> ExistsInProjectAsync(Guid projectId, int labelId);
        Task AddAsync(ProjectLabel projectLabel);
        Task AddRangeAsync(IEnumerable<ProjectLabel> projectLabels);
        Task UpdateAsync(ProjectLabel projectLabel);
        Task DeleteAsync(ProjectLabel projectLabel);
        Task SaveChangesAsync();
    }

    public class ProjectLabelRepository : IProjectLabelRepository
    {
        private readonly AppDbContext _context;
        public ProjectLabelRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<ProjectLabel>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectLabels
                .Include(pl => pl.Label)
                .Where(pl => pl.ProjectID == projectId)
                .ToListAsync();
        }

        public async Task<ProjectLabel?> GetByIdAsync(int projectLabelId)
        {
            return await _context.ProjectLabels
                .Include(pl => pl.Label)
                .FirstOrDefaultAsync(pl => pl.ProjectLabelID == projectLabelId);
        }

        public async Task<bool> ExistsInProjectAsync(Guid projectId, int labelId)
        {
            return await _context.ProjectLabels
                .AnyAsync(pl => pl.ProjectID == projectId && pl.LabelID == labelId);
        }

        public async Task AddAsync(ProjectLabel projectLabel) { await _context.ProjectLabels.AddAsync(projectLabel); }

        public async Task AddRangeAsync(IEnumerable<ProjectLabel> projectLabels) { await _context.ProjectLabels.AddRangeAsync(projectLabels); }

        public async Task UpdateAsync(ProjectLabel projectLabel) { _context.ProjectLabels.Update(projectLabel); await Task.CompletedTask; }

        public async Task DeleteAsync(ProjectLabel projectLabel) { _context.ProjectLabels.Remove(projectLabel); await Task.CompletedTask; }

        public async Task SaveChangesAsync() { await _context.SaveChangesAsync(); }
    }
}