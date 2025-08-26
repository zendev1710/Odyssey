// =============================================
// 2. HelpTopic.cs - Modèle de données
// =============================================
using System.IO;
using System.Threading.Tasks;

namespace CsMapX.Help
{
    public class HelpTopic
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public string Language { get; set; }
        public string? FilePath { get; set; }
        public string? Content { get; set; }

        public HelpTopic(string id, string title, string icon, int order, string language)
        {
            Id = id;
            Title = title;
            Icon = icon;
            Order = order;
            Language = language;
        }

        public async Task<string> LoadContentAsync()
        {
            if (!string.IsNullOrEmpty(Content))
                return Content;

            if (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath))
            {
                return await File.ReadAllTextAsync(FilePath);
            }

            return $"# {Title}\n\nContent not available.";
        }
    }
}