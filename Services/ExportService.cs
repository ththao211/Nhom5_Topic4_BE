
using Microsoft.EntityFrameworkCore;
using SWP_BE.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace SWP_BE.Services
{
   

    public class ExportService
    {
        private readonly AppDbContext _context;

        public ExportService(AppDbContext context)
        {
            _context = context;
        }
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
                .Where(t => t.ProjectID == projectId &&
                            t.Status == Models.Task.TaskStatus.Approved)
                .ToListAsync();

            if (!tasks.Any())
                throw new Exception("Không có task Approved");

            ValidateData(tasks);

            // ===== TEMP FOLDER =====
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var imageFolder = Path.Combine(tempFolder, "images");
            var labelFolder = Path.Combine(tempFolder, "labels");

            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(imageFolder);
            Directory.CreateDirectory(labelFolder);

            var project = tasks.First().Project;

            // ===== LABEL MAP =====
            var labelMap = project.ProjectLabels
             .GroupBy(pl => pl.LabelID)
              .Select((g, index) => new { LabelID = g.Key, index })
              .ToDictionary(x => x.LabelID, x => x.index);

            // ===== classes.txt =====
            var classNames = project.ProjectLabels
                .Select(pl => pl.CustomName ?? pl.Label.LabelName);

            await File.WriteAllLinesAsync(
                Path.Combine(tempFolder, "classes.txt"),
                classNames
            );

            // ===== EXPORT DATASET =====
            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null)
                        continue;

                    var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    item.DataItem.FilePath
);

                    if (!File.Exists(imagePath))
                        continue;

                    // ===== COPY IMAGE =====
                    var destImagePath = Path.Combine(
                        imageFolder,
                        item.DataItem.FileName
                    );

                    File.Copy(imagePath, destImagePath, true);

                    var lines = new List<string>();

                    foreach (var detail in item.TaskItemDetails
                                 .Where(d => d.IsApproved))
                    {
                        try
                        {
                            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                detail.AnnotationData
                            );

                            if (data == null)
                                continue;

                            int labelId = Convert.ToInt32(data["labelId"]);

                            if (!labelMap.ContainsKey(labelId))
                                continue;

                            double x = Convert.ToDouble(data["x"]);
                            double y = Convert.ToDouble(data["y"]);
                            double w = Convert.ToDouble(data["w"]);
                            double h = Convert.ToDouble(data["h"]);

                            lines.Add(
                                $"{labelMap[labelId]} {x} {y} {w} {h}"
                            );
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (!lines.Any())
                        continue;

                    // ===== LABEL FILE =====
                    var labelName =
                        Path.GetFileNameWithoutExtension(item.DataItem.FileName)
                        + ".txt";

                    var labelPath = Path.Combine(labelFolder, labelName);

                    await File.WriteAllLinesAsync(labelPath, lines);
                }
            }

            // ===== CREATE ZIP =====
            var zipPath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}.zip"
            );

            ZipFile.CreateFromDirectory(tempFolder, zipPath);

            var bytes = await File.ReadAllBytesAsync(zipPath);

            return (bytes, "yolo_dataset.zip");
        }

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
                throw new Exception("Không có task Approved");

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
                        width = item.DataItem.Width,
                        height = item.DataItem.Height
                    });

                    foreach (var detail in item.TaskItemDetails.Where(d => d.IsApproved))
                    {
                        try
                        {
                            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(detail.AnnotationData);
                            if (data == null) continue;

                            int labelId = Convert.ToInt32(data["labelId"]);
                            if (!labelMap.ContainsKey(labelId)) continue;

                            double x = Convert.ToDouble(data["x"]);
                            double y = Convert.ToDouble(data["y"]);
                            double w = Convert.ToDouble(data["w"]);
                            double h = Convert.ToDouble(data["h"]);

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

            return (Encoding.UTF8.GetBytes(json), "coco_export.json");
        }


        private void ValidateData(List<Models.Task> tasks)
        {
            foreach (var task in tasks)
            {
                foreach (var item in task.TaskItems)
                {
                    if (item.DataItem == null)
                        throw new Exception($"Item {item.ItemID} thiếu DataItem");

                    if (!item.TaskItemDetails.Any())
                        throw new Exception($"Item {item.ItemID} chưa có annotation");

                    if (item.DataItem.Width == null || item.DataItem.Height == null)
                        continue;
                }
            }
        }

    }

}
