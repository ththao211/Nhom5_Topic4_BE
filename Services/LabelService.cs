using SWP_BE.DTOs;
using SWP_BE.Models;
using SWP_BE.Repositories;

namespace SWP_BE.Services
{
    public interface ILabelService
    {
        Task<IEnumerable<LabelDto>> GetLabelsAsync(string? category);
        Task<LabelDto?> GetLabelByIdAsync(int id);
        Task<LabelDto> CreateLabelAsync(CreateLabelDto dto);
        Task<bool> UpdateLabelAsync(int id, UpdateLabelDto dto);
        Task<(bool success, string message)> DeleteLabelAsync(int id);
    }

    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepo;
        public LabelService(ILabelRepository labelRepo) { _labelRepo = labelRepo; }

        public async Task<IEnumerable<LabelDto>> GetLabelsAsync(string? category)
        {
            var labels = await _labelRepo.GetLabelsAsync(category);
            return labels.Select(l => new LabelDto
            {
                LabelID = l.LabelID,
                LabelName = l.LabelName,
                DefaultColor = l.DefaultColor,
                Category = l.Category
            });
        }

        public async Task<LabelDto?> GetLabelByIdAsync(int id)
        {
            var l = await _labelRepo.GetByIdAsync(id);
            return l == null ? null : new LabelDto
            {
                LabelID = l.LabelID,
                LabelName = l.LabelName,
                DefaultColor = l.DefaultColor,
                Category = l.Category
            };
        }

        public async Task<LabelDto> CreateLabelAsync(CreateLabelDto dto)
        {
            var label = new Label
            {
                LabelName = dto.LabelName,
                DefaultColor = dto.DefaultColor,
                Category = dto.Category
            };

            await _labelRepo.AddAsync(label);
            await _labelRepo.SaveChangesAsync();

            return new LabelDto
            {
                LabelID = label.LabelID,
                LabelName = label.LabelName,
                DefaultColor = label.DefaultColor,
                Category = label.Category
            };
        }

        public async Task<bool> UpdateLabelAsync(int id, UpdateLabelDto dto)
        {
            var label = await _labelRepo.GetByIdAsync(id);
            if (label == null) return false;

            if (!string.IsNullOrEmpty(dto.LabelName)) label.LabelName = dto.LabelName;
            if (!string.IsNullOrEmpty(dto.DefaultColor)) label.DefaultColor = dto.DefaultColor;
            if (!string.IsNullOrEmpty(dto.Category)) label.Category = dto.Category;

            await _labelRepo.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> DeleteLabelAsync(int id)
        {
            var label = await _labelRepo.GetByIdAsync(id);
            if (label == null) return (false, "Nhãn không tồn tại.");

            // Kiểm tra điều kiện xóa
            var isUsed = await _labelRepo.IsLabelUsedInProjectsAsync(id);
            if (isUsed) return (false, "Không thể xóa vì nhãn này đang được sử dụng trong dự án.");

            await _labelRepo.DeleteAsync(label);
            await _labelRepo.SaveChangesAsync();

            return (true, "Xóa nhãn thành công.");
        }
    }
}