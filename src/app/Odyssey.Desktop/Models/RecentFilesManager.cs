using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class RecentFilesManager
{
    private const int MaxRecentFiles = 10;
    private readonly string _storagePath;

    public List<string> RecentFiles { get; private set; } = new();

    public RecentFilesManager(string appName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, appName);
        Directory.CreateDirectory(folder);
        _storagePath = Path.Combine(folder, "recentfiles.json");
        Load();
    }

    public void AddFile(string filePath)
    {
        filePath = Path.GetFullPath(filePath);
        RecentFiles.Remove(filePath); // Remove if already exists
        RecentFiles.Insert(0, filePath); // Add to top
        if (RecentFiles.Count > MaxRecentFiles)
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        Save();
    }

    public void Save()
    {
        File.WriteAllText(_storagePath, JsonSerializer.Serialize(RecentFiles));
    }

    public void Load()
    {
        if (File.Exists(_storagePath))
        {
            try
            {
                var json = File.ReadAllText(_storagePath);
                RecentFiles = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                RecentFiles = new List<string>();
            }
        }
    }
}