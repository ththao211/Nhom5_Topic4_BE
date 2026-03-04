using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetProjectsAsync(Guid managerId);
        Task<ProjectResponseDto?> GetProjectByIdAsync(Guid projectId, Guid managerId);
        Task<Guid> CreateProjectAsync(CreateProjectDto dto, Guid managerId);
        Task<bool> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid managerId);
        Task<bool> ChangeStatusAsync(Guid projectId, string status, Guid managerId);
        Task<bool> UpdateGuidelineAsync(Guid projectId, string url, Guid managerId);
        Task<bool> UploadDataAsync(Guid projectId, UploadDataDto dto, Guid managerId);
        Task<SplitTaskResultDto?> SplitTasksAsync(Guid projectId, SplitTaskDto dto, Guid managerId);
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        public ProjectService(IProjectRepository projectRepo) { _projectRepo = projectRepo; }

        public async Task<IEnumerable<ProjectResponseDto>> GetProjectsAsync(Guid managerId)
        {
            var projects = await _projectRepo.GetAllByManagerIdAsync(managerId);
            return projects.Select(p => MapToResponseDto(p));
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid projectId, Guid managerId)
        {
            var project = await _projectRepo.GetByIdAndManagerAsync(projectId, managerId);
            return project == null ? null : MapToResponseDto(project);
        }

        private ProjectResponseDto MapToResponseDto(Project p)
        {
            return new ProjectResponseDto
            {
                ProjectID = p.ProjectID,
                ProjectName = p.ProjectName,
                Description = p.Description,
                Topic = p.Topic,
                ProjectType = p.ProjectType,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                GuidelineUrl = p.GuidelineUrl,
                TotalDataItems = p.DataItems?.Count ?? 0,
                DataItems = p.DataItems?.Select(d => new DataItemDto
                {
                    DataID = d.DataID,
                    FilePath = d.FilePath,
                    FileName = d.FileName,
                    FileType = d.FileType,
                    IsAssigned = d.IsAssigned
                }).ToList() ?? new List<DataItemDto>()
            };
        }

        public async Task<Guid> CreateProjectAsync(CreateProjectDto dto, Guid managerId)
        {
            var project = new Project
            {
                ProjectID = Guid.NewGuid(), // TẠO NGẪU NHIÊN ID PROJECT
                ProjectName = dto.ProjectName,
                Description = dto.Description,
                Topic = dto.Topic,
                ProjectType = dto.ProjectType,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                ManagerID = managerId,
                GuidelineUrl = dto.GuidelineUrl ?? ""
            };
            await _projectRepo.AddAsync(project);
            await _projectRepo.SaveChangesAsync();
            return project.ProjectID;
        }

        public async Task<bool> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid managerId)
        {
            var project = await _projectRepo.GetByIdAndManagerAsync(projectId, managerId);
            if (project == null) return false;

            project.ProjectName = dto.ProjectName;
            project.Description = dto.Description;
            project.Topic = dto.Topic;
            project.ProjectType = dto.ProjectType;
            project.GuidelineUrl = dto.GuidelineUrl; // Tối ưu: Cập nhật luôn Guideline

            await _projectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeStatusAsync(Guid projectId, string status, Guid managerId)
        {
            var project = await _projectRepo.GetByIdAndManagerAsync(projectId, managerId);
            if (project == null) return false;
            project.Status = status;
            await _projectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateGuidelineAsync(Guid projectId, string url, Guid managerId)
        {
            var project = await _projectRepo.GetByIdAndManagerAsync(projectId, managerId);
            if (project == null) return false;
            project.GuidelineUrl = url;
            await _projectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadDataAsync(Guid projectId, UploadDataDto dto, Guid managerId)
        {
            var project = await _projectRepo.GetByIdAndManagerAsync(projectId, managerId);
            if (project == null) return false;

            var dataItems = dto.FileUrls.Select(url => new DataItem
            {
                DataID = Guid.NewGuid(), // TẠO NGẪU NHIÊN ID CHO TỪNG ẢNH
                ProjectID = projectId,
                FilePath = url,
                FileType = dto.FileType,
                FileName = $"Data_{DateTime.Now.Ticks}", // Tối ưu: Đặt tên file động
                IsAssigned = false
            }).ToList();

            await _projectRepo.AddDataItemsAsync(dataItems);
            await _projectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<SplitTaskResultDto?> SplitTasksAsync(Guid projectId, SplitTaskDto dto, Guid managerId)
        {
            var project = await _projectRepo.GetByIdAndManagerAsync(projectId, managerId);
            if (project == null) return null;

            var unassignedData = await _projectRepo.GetUnassignedDataAsync(projectId, dto.NumberOfItemsPerTask);
            if (!unassignedData.Any()) throw new Exception("Không còn dữ liệu chưa gán.");

            var newTask = new Models.Task
            {
                TaskID = Guid.NewGuid(), // TẠO NGẪU NHIÊN ID TASK
                ProjectID = projectId,
                TaskName = $"{dto.TaskPrefix} - {DateTime.Now:dd/MM HH:mm}",
                Status = "Draft",
                Deadline = DateTime.UtcNow.AddDays(7)
            };

            await _projectRepo.AddTaskAsync(newTask);
            await _projectRepo.SaveChangesAsync();

            var taskItems = unassignedData.Select(d => new TaskItem
            {
                ItemID = Guid.NewGuid(), // Nếu TaskItem dùng Guid
                TaskID = newTask.TaskID,
                DataID = d.DataID
            }).ToList();

            await _projectRepo.AddTaskItemsAsync(taskItems);

            foreach (var item in unassignedData) item.IsAssigned = true;
            await _projectRepo.SaveChangesAsync();

            return new SplitTaskResultDto { TaskId = newTask.TaskID, ItemCount = unassignedData.Count };
        }
    }
}