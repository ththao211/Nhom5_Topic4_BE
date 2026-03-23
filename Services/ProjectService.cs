using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskModel = SWP_BE.Models.Task;

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
                ProjectID = Guid.NewGuid(),
                ProjectName = dto.ProjectName,
                Description = dto.Description,
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
            project.ProjectType = dto.ProjectType;
            project.GuidelineUrl = dto.GuidelineUrl;

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
                DataID = Guid.NewGuid(),
                ProjectID = projectId,
                FilePath = url,
                FileType = dto.FileType,
                FileName = $"Data_{DateTime.Now.Ticks}",
                IsAssigned = false
            }).ToList();

            await _projectRepo.AddDataItemsAsync(dataItems);
            await _projectRepo.SaveChangesAsync();
            return true;
        }
    }
}