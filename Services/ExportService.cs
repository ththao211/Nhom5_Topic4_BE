using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using SWP_BE.Models;

namespace SWP_BE.Services
{
    public class ExportService
    {
        private readonly AppDbContext _context;

        public ExportService(AppDbContext context)
        {
            _context = context;
        }

        // ================= YOLO EXPORT =================
        public async Task<(byte[] fileBytes, string fileName)> ExportYoloZipAsync(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.DataItem)
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectLabels)
                        .ThenInclude(pl => pl.Label)
                .Where(t => t.ProjectID == projectId && t.Status == Models.Task.TaskStatus.Approved)
                .ToListAsync();

            if (!tasks.Any())
                throw new Exception("Dự án chưa có Task nào được duyệt (Approved) để xuất dữ liệu.");

            var project = tasks.First().Project;

            var taskItemIds = tasks
                .SelectMany(t => t.TaskItems)
                .Select(i => i.ItemID)
                .ToList();

            var details = await _context.TaskItemDetails
                .Where(d => taskItemIds.Contains(d.TaskItemID)
                    && !string.IsNullOrWhiteSpace(d.AnnotationData))
                .Select(d => new
                {
                    d.TaskItemID,
                    d.AnnotationData
                })
                .ToListAsync();

            Console.WriteLine($"DETAIL COUNT = {details.Count}");

            var detailLookup = details
                .GroupBy(d => d.TaskItemID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var groupedLabels = project.ProjectLabels
                .GroupBy(pl => pl.LabelID)
                .Select(g => g.First())
                .ToList();

            var labelMap = groupedLabels
                .Select((pl, index) => new { pl.LabelID, index })
                .ToDictionary(x => x.LabelID, x => x.index);

            var classNames = groupedLabels
                .Select(pl => pl.CustomName ?? pl.Label.LabelName);

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var imageFolder = Path.Combine(tempFolder, "images");
            var labelFolder = Path.Combine(tempFolder, "labels");

            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(imageFolder);
            Directory.CreateDirectory(labelFolder);

            await File.WriteAllLinesAsync(
                Path.Combine(tempFolder, "classes.txt"),
                classNames
            );

            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null) continue;

                    if (!detailLookup.ContainsKey(item.ItemID))
                        continue;

                    var filePath = item.DataItem.FilePath;

                    if (string.IsNullOrEmpty(filePath))
                        continue;

                    var extension = Path.GetExtension(filePath);
                    if (string.IsNullOrEmpty(extension))
                        extension = ".jpg";

                    var fileName =
                        Path.GetFileNameWithoutExtension(item.DataItem.FileName)
                        + extension;

                    var destImagePath = Path.Combine(imageFolder, fileName);

                    if (filePath.StartsWith("http"))
                    {
                        using var httpClient = new HttpClient();
                        var imageBytes = await httpClient.GetByteArrayAsync(filePath);

                        if (imageBytes.Length == 0)
                            continue;

                        await File.WriteAllBytesAsync(destImagePath, imageBytes);
                    }
                    else
                    {
                        var imagePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            filePath
                        );

                        if (!File.Exists(imagePath))
                            continue;

                        File.Copy(imagePath, destImagePath, true);
                    }

                    var lines = new List<string>();

                    foreach (var detail in detailLookup[item.ItemID])
                    {
                        try
                        {
                            Console.WriteLine($"RAW JSON = {detail.AnnotationData}");

                            var data =
                                JsonSerializer.Deserialize<
                                    Dictionary<string, JsonElement>
                                >(detail.AnnotationData);

                            if (data == null)
                                continue;


                            int labelId = -1;

                            // cố gắng đọc labelId nếu có
                            if (data.ContainsKey("labelId"))
                                labelId = data["labelId"].GetInt32();

                            else if (data.ContainsKey("labelID"))
                                labelId = data["labelID"].GetInt32();

                            else if (data.ContainsKey("classId"))
                                labelId = data["classId"].GetInt32();


                            // nếu không có labelId → fallback class 0
                            int labelIndex = 0;

                            if (labelId != -1)
                            {
                                if (!labelMap.ContainsKey(labelId))
                                    continue;

                                labelIndex = labelMap[labelId];
                            }

                            double x = 0, y = 0, w = 0, h = 0;

                            if (data.ContainsKey("x")
                                && data.ContainsKey("y")
                                && data.ContainsKey("w")
                                && data.ContainsKey("h"))
                            {
                                x = data["x"].GetDouble();
                                y = data["y"].GetDouble();
                                w = data["w"].GetDouble();
                                h = data["h"].GetDouble();
                            }
                            else if (data.ContainsKey("x")
                                && data.ContainsKey("y")
                                && data.ContainsKey("width")
                                && data.ContainsKey("height"))
                            {
                                var imgW = item.DataItem.Width ?? 1;
                                var imgH = item.DataItem.Height ?? 1;

                                var px = data["x"].GetDouble();
                                var py = data["y"].GetDouble();
                                var pw = data["width"].GetDouble();
                                var ph = data["height"].GetDouble();

                                x = px / imgW;
                                y = py / imgH;
                                w = pw / imgW;
                                h = ph / imgH;
                            }
                            else
                            {
                                continue;
                            }

                            Console.WriteLine(
                                $"ADD LABEL: {labelId} {x} {y} {w} {h}"
                            );

                            lines.Add($"{labelIndex} {x} {y} {w} {h}");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"PARSE ERROR: {ex.Message}");
                            continue;
                        }
                        catch { }
                    }

                    if (!lines.Any())
                        continue;

                    var labelName =
                        Path.GetFileNameWithoutExtension(fileName) + ".txt";

                    await File.WriteAllLinesAsync(
                        Path.Combine(labelFolder, labelName),
                        lines
                    );
                }
            }

            var zipPath =
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

            ZipFile.CreateFromDirectory(tempFolder, zipPath);

            var bytes = await File.ReadAllBytesAsync(zipPath);
            var projectExists = await _context.Projects
            .AnyAsync(p => p.ProjectID == projectId);

            if (!projectExists)
                throw new Exception("Project không tồn tại.");

            _context.ExportHistories.Add(new ExportHistory
            {
                ExportID = Guid.NewGuid(),
                Format = "YOLO",
                CreatedAt = DateTime.UtcNow,
                ProjectID = projectId
            });

            await _context.SaveChangesAsync();
            return (bytes, "yolo_dataset.zip");
        }


        // ================= COCO EXPORT =================
        public async Task<(byte[] fileBytes, string fileName)> ExportCocoFileAsync(Guid projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskItems)
                    .ThenInclude(i => i.DataItem)
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectLabels)
                        .ThenInclude(pl => pl.Label)
                .Where(t => t.ProjectID == projectId &&
                            t.Status == Models.Task.TaskStatus.Approved)
                .ToListAsync();

            if (!tasks.Any())
                throw new Exception("Không có task Approved.");

            var project = tasks.First().Project;

            var groupedLabels = project.ProjectLabels
                .GroupBy(pl => pl.LabelID)
                .Select(g => g.First())
                .ToList();

            var labelMap = groupedLabels
                .Select((pl, index) => new { pl.LabelID, index })
                .ToDictionary(x => x.LabelID, x => x.index);

            var categories = groupedLabels
                .Select((pl, index) => new
                {
                    id = index,
                    name = pl.CustomName ?? pl.Label.LabelName
                });

            int imageId = 1;
            int annotationId = 1;

            var images = new List<object>();
            var annotations = new List<object>();

            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null)
                        continue;

                    images.Add(new
                    {
                        id = imageId,
                        file_name = item.DataItem.FileName,
                        width = item.DataItem.Width ?? 1,
                        height = item.DataItem.Height ?? 1
                    });

                    foreach (var detail in item.TaskItemDetails)
                    {
                        try
                        {
                            var data = JsonSerializer.Deserialize<
                                Dictionary<string, JsonElement>
                            >(detail.AnnotationData);

                            if (data == null)
                                continue;

                            if (!data.ContainsKey("labelId"))
                                continue;

                            var labelId =
                                data["labelId"].GetInt32();

                            if (!labelMap.ContainsKey(labelId))
                                continue;

                            var x = data["x"].GetDouble();
                            var y = data["y"].GetDouble();
                            var w = data["w"].GetDouble();
                            var h = data["h"].GetDouble();

                            var width = item.DataItem.Width ?? 1;
                            var height = item.DataItem.Height ?? 1;

                            var cocoX = (x - w / 2) * width;
                            var cocoY = (y - h / 2) * height;

                            annotations.Add(new
                            {
                                id = annotationId++,
                                image_id = imageId,
                                category_id = labelMap[labelId],
                                bbox = new[]
                                {
                                    cocoX,
                                    cocoY,
                                    w * width,
                                    h * height
                                },
                                area = w * h * width * height,
                                iscrowd = 0
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    imageId++;
                }
            }

            var coco = new
            {
                images,
                annotations,
                categories
            };

            var json =
                JsonSerializer.Serialize(coco,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            _context.ExportHistories.Add(new ExportHistory
            {
                ExportID = Guid.NewGuid(),
                Format = "COCO",
                CreatedAt = DateTime.UtcNow,
                ProjectID = projectId
            });

            await _context.SaveChangesAsync();

            return (
                Encoding.UTF8.GetBytes(json),
                "coco_export.json"
            );
        }
    }
}