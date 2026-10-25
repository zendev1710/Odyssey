namespace Odyssey.Icons.Generator.Tests;

internal sealed class TempDirectory : IDisposable
{
    public string Root { get; } =
        Path.Combine(Path.GetTempPath(), "odyssey-icons-" + Guid.NewGuid().ToString("N"));

    public TempDirectory()
    {
        Directory.CreateDirectory(Root);
    }

    public string Write(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException)
        {
            // Best effort cleanup.
        }
    }
}
