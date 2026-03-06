using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public interface IProjectLabelService
    {
        Task<IEnumerable<ProjectLabelDto>> GetLabelsByProjectIdAsync(Guid projectId);
        Task<(bool success, string message)> ImportLabelsAsync(Guid projectId, ImportProjectLabelsDto dto);
        Task<ProjectLabelDto?> CreateCustomLabelAsync(Guid projectId, CreateCustomProjectLabelDto dto);
        Task<bool> UpdateProjectLabelAsync(int projectLabelId, UpdateProjectLabelDto dto);
        Task<(bool success, string message)> DeleteProjectLabelAsync(int projectLabelId);
    }

    public class ProjectLabelService : IProjectLabelService
    {
        private readonly IProjectLabelRepository _projectLabelRepo;
        private readonly ILabelRepository _labelRepo;

        public ProjectLabelService(IProjectLabelRepository projectLabelRepo, ILabelRepository labelRepo)
        {
            _projectLabelRepo = projectLabelRepo;
            _labelRepo = labelRepo;
        }

        public async Task<IEnumerable<ProjectLabelDto>> GetLabelsByProjectIdAsync(Guid projectId)
        {
            var projectLabels = await _projectLabelRepo.GetByProjectIdAsync(projectId);
            return projectLabels.Select(pl => new ProjectLabelDto
            {
                ProjectLabelID = pl.ProjectLabelID,
                ProjectID = pl.ProjectID,
                LabelID = pl.LabelID,
                CustomName = pl.CustomName,
                LabelName = pl.Label?.LabelName ?? string.Empty,
                DefaultColor = pl.Label?.DefaultColor ?? string.Empty
            });
        }

        public async Task<(bool success, string message)> ImportLabelsAsync(Guid projectId, ImportProjectLabelsDto dto)
        {
            var labelsToImport = new List<ProjectLabel>();

            foreach (var labelId in dto.LabelIDs)
            {
                if (await _projectLabelRepo.ExistsInProjectAsync(projectId, labelId)) continue;

                var label = await _labelRepo.GetByIdAsync(labelId);
                if (label != null)
                {
                    labelsToImport.Add(new ProjectLabel
                    {
                        ProjectID = projectId,
                        LabelID = labelId,
                        CustomName = label.LabelName // Khởi tạo CustomName bằng tên gốc
                    });
                }
            }

            if (!labelsToImport.Any()) return (false, "Không có nhãn mới nào hợp lệ để thêm vào dự án.");

            await _projectLabelRepo.AddRangeAsync(labelsToImport);
            await _projectLabelRepo.SaveChangesAsync();
            return (true, "Import nhãn thành công.");
        }

        public async Task<ProjectLabelDto?> CreateCustomLabelAsync(Guid projectId, CreateCustomProjectLabelDto dto)
        {
            // 1. Luôn tạo Label vào kho tổng (Để lấy LabelID)
            var newLabel = new Label
            {
                LabelName = dto.CustomName,
                DefaultColor = dto.DefaultColor,
                Category = dto.SaveToLibrary ? dto.Category : "Project_Custom"
            };

            await _labelRepo.AddAsync(newLabel);
            await _labelRepo.SaveChangesAsync();

            // 2. Gán Label vừa tạo vào Project
            var projectLabel = new ProjectLabel
            {
                ProjectID = projectId,
                LabelID = newLabel.LabelID,
                CustomName = dto.CustomName
            };

            await _projectLabelRepo.AddAsync(projectLabel);
            await _projectLabelRepo.SaveChangesAsync();

            return new ProjectLabelDto
            {
                ProjectLabelID = projectLabel.ProjectLabelID,
                ProjectID = projectLabel.ProjectID,
                LabelID = projectLabel.LabelID,
                CustomName = projectLabel.CustomName,
                LabelName = newLabel.LabelName,
                DefaultColor = newLabel.DefaultColor
            };
        }

        public async Task<bool> UpdateProjectLabelAsync(int projectLabelId, UpdateProjectLabelDto dto)
        {
            var projectLabel = await _projectLabelRepo.GetByIdAsync(projectLabelId);
            if (projectLabel == null) return false;

            projectLabel.CustomName = dto.CustomName;

            await _projectLabelRepo.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> DeleteProjectLabelAsync(int projectLabelId)
        {
            var projectLabel = await _projectLabelRepo.GetByIdAsync(projectLabelId);
            if (projectLabel == null) return (false, "Nhãn dự án không tồn tại.");

            // TODO: Nếu bạn có bảng TaskItemDetail (lưu thông tin ảnh đã được gán nhãn nào), 
            // bạn cần check thêm ở đây xem projectLabelId đã được dùng chưa trước khi xóa.

            await _projectLabelRepo.DeleteAsync(projectLabel);
            await _projectLabelRepo.SaveChangesAsync();

            return (true, "Đã gỡ nhãn khỏi dự án.");
        }
    }
}