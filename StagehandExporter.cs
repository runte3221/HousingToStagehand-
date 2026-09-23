using System;
using System.IO;
using Stagehand.Definitions;

namespace HousingToStagehand;

public static class StagehandExporter
{
    public sealed class ExportResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? SavedFilePath { get; init; }
    }

    /// <summary>
    /// Gets the default Stagehand stages folder (Documents\Stages\).
    /// </summary>
    public static string GetDefaultStagesDirectory()
    {
        var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(myDocs, "Stages");
    }

    /// <summary>
    /// Exports a StageDefinition directly to a .json file in the Stages folder.
    /// </summary>
    public static ExportResult ExportToFile(StageDefinition stage, string targetDirectory, string stageName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(targetDirectory))
            {
                targetDirectory = GetDefaultStagesDirectory();
            }

            Directory.CreateDirectory(targetDirectory);

            var safeName = SanitizeFileName(stageName);
            var filePath = Path.Combine(targetDirectory, $"{safeName}.json");

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stage.WriteToJSONStream(stream);
            }

            return new ExportResult
            {
                Success = true,
                Message = $"Successfully saved stage to \"{safeName}.json\"!",
                SavedFilePath = filePath,
            };
        }
        catch (Exception ex)
        {
            return new ExportResult
            {
                Success = false,
                Message = $"Export failed: {ex.Message}",
            };
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0)
                chars[i] = '_';
        }

        var result = new string(chars).Trim();
        return string.IsNullOrEmpty(result) ? "Stage" : result;
    }
}
