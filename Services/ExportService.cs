using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using SWP_BE.Models;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace SWP_BE.Services
{
    public class ExportService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExportService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ==========================================
        // EXPORT YOLO (ZIP)
        // ==========================================
        public async Task<(byte[] fileBytes, string fileName)> ExportYoloZipAsync(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.TaskItemDetails)
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.DataItem)
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectLabels)
                        .ThenInclude(pl => pl.Label)
                .Where(t => t.ProjectID == projectId && t.Status == Models.Task.TaskStatus.Approved)
                .ToListAsync();

            if (!tasks.Any())
                throw new Exception("Dự án chưa có Task nào được duyệt (Approved) để xuất dữ liệu.");

            ValidateData(tasks);

            var project = tasks.First().Project;

            var labelMap = project.ProjectLabels
                .GroupBy(pl => pl.LabelID)
                .Select((g, index) => new { LabelID = g.Key, index })
                .ToDictionary(x => x.LabelID, x => x.index);

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var imageFolder = Path.Combine(tempFolder, "images");
            var labelFolder = Path.Combine(tempFolder, "labels");

            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(imageFolder);
            Directory.CreateDirectory(labelFolder);

            var classNames = project.ProjectLabels.Select(pl => pl.CustomName ?? pl.Label.LabelName);
            await File.WriteAllLinesAsync(Path.Combine(tempFolder, "classes.txt"), classNames);

            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null) continue;

                    try
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), item.DataItem.FilePath);
                        if (File.Exists(imagePath))
                        {
                            var destImagePath = Path.Combine(imageFolder, item.DataItem.FileName);
                            File.Copy(imagePath, destImagePath, true);
                        }
                    }
                    catch { }

                    var lines = new List<string>();

                    // 🔥 ĐÃ SỬA: Lọc theo IsApproved == "True" (Chuỗi string)
                    foreach (var detail in item.TaskItemDetails.Where(d => d.IsApproved == "True"))
                    {
                        try
                        {
                            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(detail.AnnotationData);
                            if (data == null) continue;

                            int labelId = Convert.ToInt32(data["labelId"].ToString());
                            if (!labelMap.ContainsKey(labelId)) continue;

                            double x = Convert.ToDouble(data["x"].ToString());
                            double y = Convert.ToDouble(data["y"].ToString());
                            double w = Convert.ToDouble(data["w"].ToString());
                            double h = Convert.ToDouble(data["h"].ToString());

                            if (x < 0 || x > 1 || y < 0 || y > 1 || w <= 0 || h <= 0)
                                continue;

                            lines.Add($"{labelMap[labelId]} {x} {y} {w} {h}");
                        }
                        catch { }
                    }

                    if (lines.Any())
                    {
                        var labelName = Path.GetFileNameWithoutExtension(item.DataItem.FileName) + ".txt";
                        var labelPath = Path.Combine(labelFolder, labelName);
                        await File.WriteAllLinesAsync(labelPath, lines);
                    }
                }
            }

            var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            ZipFile.CreateFromDirectory(tempFolder, zipPath);

            var bytes = await File.ReadAllBytesAsync(zipPath);

            var managerId = GetManagerId();
            var itemCount = tasks.SelectMany(t => t.TaskItems).Count();

            _context.ExportHistories.Add(new ExportHistory
            {
                ExportID = Guid.NewGuid(),
                Format = "YOLO",
                ItemCount = itemCount,
                CreatedAt = DateTime.UtcNow,
                ProjectID = projectId,
                ManagerID = managerId
            });

            await _context.SaveChangesAsync();
            return (bytes, "yolo_dataset.zip");
        }

        // ==========================================
        // EXPORT COCO (FILE BYTES)
        // ==========================================
        public async Task<(byte[] fileBytes, string fileName)> ExportCocoFileAsync(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.TaskItemDetails)
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.DataItem)
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectLabels)
                        .ThenInclude(pl => pl.Label)
                .Where(t => t.ProjectID == projectId && t.Status == Models.Task.TaskStatus.Approved)
                .ToListAsync();

            if (!tasks.Any())
                throw new Exception("Dự án chưa có Task nào được duyệt (Approved) để xuất dữ liệu.");

            ValidateData(tasks);

            var project = tasks.First().Project;

            var categories = project.ProjectLabels
                .Select((pl, index) => new
                {
                    id = index,
                    name = pl.CustomName ?? pl.Label.LabelName
                }).ToList();

            var labelMap = project.ProjectLabels
                .Select((pl, index) => new { pl.LabelID, index })
                .ToDictionary(x => x.LabelID, x => x.index);

            int imageId = 1;
            int annotationId = 1;

            var images = new List<object>();
            var annotations = new List<object>();

            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null) continue;

                    images.Add(new
                    {
                        id = imageId,
                        file_name = item.DataItem.FileName,
                        width = item.DataItem.Width ?? 1,
                        height = item.DataItem.Height ?? 1
                    });

                    // 🔥 ĐÃ SỬA: Lọc theo IsApproved == "True" (Chuỗi string)
                    foreach (var detail in item.TaskItemDetails.Where(d => d.IsApproved == "True"))
                    {
                        try
                        {
                            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(detail.AnnotationData);
                            if (data == null) continue;

                            int labelId = Convert.ToInt32(data["labelId"].ToString());
                            if (!labelMap.ContainsKey(labelId)) continue;

                            double x = Convert.ToDouble(data["x"].ToString());
                            double y = Convert.ToDouble(data["y"].ToString());
                            double w = Convert.ToDouble(data["w"].ToString());
                            double h = Convert.ToDouble(data["h"].ToString());

                            var width = item.DataItem.Width ?? 1;
                            var height = item.DataItem.Height ?? 1;

                            var cocoX = (x - w / 2) * width;
                            var cocoY = (y - h / 2) * height;
                            var cocoW = w * width;
                            var cocoH = h * height;

                            annotations.Add(new
                            {
                                id = annotationId++,
                                image_id = imageId,
                                category_id = labelMap[labelId],
                                bbox = new[] { cocoX, cocoY, cocoW, cocoH },
                                area = cocoW * cocoH,
                                iscrowd = 0
                            });
                        }
                        catch { }
                    }
                    imageId++;
                }
            }

            var coco = new { images, annotations, categories };

            var json = JsonSerializer.Serialize(coco, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var managerId = GetManagerId();
            var itemCount = tasks.SelectMany(t => t.TaskItems).Count();

            _context.ExportHistories.Add(new ExportHistory
            {
                ExportID = Guid.NewGuid(),
                Format = "COCO",
                ItemCount = itemCount,
                CreatedAt = DateTime.UtcNow,
                ProjectID = projectId,
                ManagerID = managerId
            });

            await _context.SaveChangesAsync();
            return (Encoding.UTF8.GetBytes(json), "coco_export.json");
        }

        private void ValidateData(List<Models.Task> tasks)
        {
            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null)
                        throw new Exception($"Item {item.ItemID} thiếu dữ liệu gốc (DataItem)");
                }
            }
        }

        private Guid GetManagerId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user?.FindFirst("id")?.Value
                      ?? user?.FindFirst("sub")?.Value;

            if (Guid.TryParse(userId, out var guidId))
            {
                return guidId;
            }

            throw new Exception("Hết cứu: Không thể trích xuất ID người dùng từ Token. Vui lòng đăng nhập lại.");
        }
    }
}