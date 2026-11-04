using System.IO;
using Odyssey.Models.Localization;
using Odyssey.ViewModels;

namespace Odyssey.Utils
{
    /// <summary>
    /// Utilities about files.
    /// </summary>
    public static class FileUtils
    {
        public static string GetFileExtensionWithoutDot(string pathname)
        {
            return Path.GetExtension(pathname)[1..];
        }

        public static string GetFileExtensionWithDot(string ext)
        {
            return $".{ext}";
        }

        public static string GetFileExtensionPattern(string ext)
        {
            return $"*.{ext}";
        }
    }
}
