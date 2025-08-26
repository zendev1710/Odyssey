// =============================================
// 1. HelpSystem.cs - Gestionnaire principal
// =============================================
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Odyssey.Help
{
    public class HelpSystem
    {
        private static HelpSystem? _instance;
        private readonly Dictionary<string, HelpTopic> _topics = new();
        private string _currentLanguage = "en";
        private HelpWindow? _helpWindow;
        private readonly string _helpContentPath;

        public static HelpSystem Instance => _instance ??= new HelpSystem();

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                LanguageChanged?.Invoke(_currentLanguage);
            }
        }

        public event Action<string>? LanguageChanged;

        private HelpSystem()
        {
            _helpContentPath = Path.Combine("Assets", "Help");
            LoadTopics();
        }

        public async Task ShowHelp(string? topicId = null)
        {
            if (_helpWindow == null)
            {
                _helpWindow = new HelpWindow();
                _helpWindow.Closed += (s, e) => _helpWindow = null;
            }

            if (!string.IsNullOrEmpty(topicId))
            {
                await _helpWindow.NavigateToTopic(topicId);
            }

            _helpWindow.Show();
            _helpWindow.Activate();
        }

        public HelpTopic? GetTopic(string topicId)
        {
            var key = $"{_currentLanguage}_{topicId}";
            return _topics.TryGetValue(key, out var topic) ? topic : null;
        }

        public List<HelpTopic> GetAllTopics()
        {
            return _topics.Values
                .Where(t => t.Language == _currentLanguage)
                .OrderBy(t => t.Order)
                .ToList();
        }

        public string GetTopicUrl(string topicId)
        {
            var htmlPath = Path.Combine(_helpContentPath, "generated", _currentLanguage, $"{topicId}.html");
            if (File.Exists(htmlPath))
            {
                return new Uri(Path.GetFullPath(htmlPath)).ToString();
            }
            
            // Fallback - generate basic HTML
            return GenerateBasicHtml(topicId);
        }

        private void LoadTopics()
        {
            // Load from topics.json if exists, otherwise use defaults
            var topicsFile = Path.Combine(_helpContentPath, "topics.json");
            
            if (File.Exists(topicsFile))
            {
                LoadTopicsFromJson(topicsFile);
            }
            else
            {
                LoadDefaultTopics();
            }
        }

        private void LoadTopicsFromJson(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var topicsData = JsonSerializer.Deserialize<Dictionary<string, List<TopicData>>>(json);
                
                if (topicsData != null)
                {
                    foreach (var (language, topics) in topicsData)
                    {
                        foreach (var topicData in topics)
                        {
                            var topic = new HelpTopic(
                                topicData.Id,
                                topicData.Title,
                                topicData.Icon,
                                topicData.Order,
                                language
                            );
                            _topics[$"{language}_{topic.Id}"] = topic;
                        }
                    }
                }
            }
            catch
            {
                LoadDefaultTopics();
            }
        }

        private void LoadDefaultTopics()
        {
            var languages = new[] { "en", "fr", "de", "es" };
            
            foreach (var lang in languages)
            {
                var topics = GetDefaultTopicsForLanguage(lang);
                foreach (var topic in topics)
                {
                    _topics[$"{lang}_{topic.Id}"] = topic;
                }
            }
        }

        private List<HelpTopic> GetDefaultTopicsForLanguage(string language)
        {
            var translations = new Dictionary<string, Dictionary<string, (string title, string icon)>>
            {
                ["en"] = new()
                {
                    ["getting-started"] = ("Getting Started", "🚀"),
                    ["user-interface"] = ("User Interface", "🖥️"),
                    ["file-operations"] = ("File Operations", "📁"),
                    ["advanced-features"] = ("Advanced Features", "⚙️"),
                    ["troubleshooting"] = ("Troubleshooting", "🔧"),
                    ["keyboard-shortcuts"] = ("Keyboard Shortcuts", "⌨️"),
                    ["about"] = ("About", "ℹ️")
                },
                ["fr"] = new()
                {
                    ["getting-started"] = ("Démarrage", "🚀"),
                    ["user-interface"] = ("Interface Utilisateur", "🖥️"),
                    ["file-operations"] = ("Opérations Fichiers", "📁"),
                    ["advanced-features"] = ("Fonctionnalités Avancées", "⚙️"),
                    ["troubleshooting"] = ("Dépannage", "🔧"),
                    ["keyboard-shortcuts"] = ("Raccourcis Clavier", "⌨️"),
                    ["about"] = ("À Propos", "ℹ️")
                }
            };

            var langDict = translations.TryGetValue(language, out var dict) 
                ? dict 
                : translations["en"];

            var topics = new List<HelpTopic>();
            int order = 1;
            
            foreach (var (id, (title, icon)) in langDict)
            {
                topics.Add(new HelpTopic(id, title, icon, order++, language));
            }

            return topics;
        }

        private string GenerateBasicHtml(string topicId)
        {
            var topic = GetTopic(topicId);
            var title = topic?.Title ?? topicId;
            var icon = topic?.Icon ?? "📄";

            var html = $@"
<!DOCTYPE html>
<html lang='{_currentLanguage}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        {GetDefaultCSS()}
    </style>
</head>
<body>
    <div class='container'>
        <header>
            <h1>{icon} {title}</h1>
        </header>
        <main>
            {GetTopicContent(topicId)}
        </main>
    </div>
    <script>
        {GetDefaultJavaScript()}
    </script>
</body>
</html>";

            // Save to temp file
            var tempFile = Path.Combine(Path.GetTempPath(), $"help_{topicId}_{_currentLanguage}.html");
            File.WriteAllText(tempFile, html);
            return new Uri(Path.GetFullPath(tempFile)).ToString();
        }

        private string GetDefaultCSS()
        {
            return @"
                :root {
                    --primary-color: #0066cc;
                    --secondary-color: #f5f5f5;
                    --text-color: #333;
                    --border-color: #ddd;
                }

                * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }

                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    color: var(--text-color);
                    background-color: white;
                }

                .container {
                    max-width: 800px;
                    margin: 0 auto;
                    padding: 20px;
                }

                header {
                    border-bottom: 2px solid var(--border-color);
                    padding-bottom: 20px;
                    margin-bottom: 30px;
                }

                h1 {
                    color: var(--primary-color);
                    font-size: 2.5em;
                    margin-bottom: 10px;
                }

                h2 {
                    color: var(--primary-color);
                    font-size: 1.8em;
                    margin: 30px 0 15px 0;
                    border-bottom: 1px solid var(--border-color);
                    padding-bottom: 5px;
                }

                h3 {
                    color: var(--text-color);
                    font-size: 1.3em;
                    margin: 20px 0 10px 0;
                }

                p {
                    margin-bottom: 15px;
                    text-align: justify;
                }

                ul, ol {
                    margin: 15px 0 15px 30px;
                }

                li {
                    margin-bottom: 5px;
                }

                .info-box {
                    background-color: #e3f2fd;
                    border-left: 4px solid var(--primary-color);
                    padding: 15px;
                    margin: 20px 0;
                    border-radius: 4px;
                }

                .warning-box {
                    background-color: #fff3e0;
                    border-left: 4px solid #ff9800;
                    padding: 15px;
                    margin: 20px 0;
                    border-radius: 4px;
                }

                .code-block {
                    background-color: var(--secondary-color);
                    border: 1px solid var(--border-color);
                    border-radius: 4px;
                    padding: 15px;
                    margin: 15px 0;
                    font-family: 'Consolas', 'Monaco', monospace;
                    overflow-x: auto;
                }

                .keyboard-shortcut {
                    background-color: #f0f0f0;
                    border: 1px solid #ccc;
                    border-radius: 3px;
                    padding: 2px 6px;
                    font-family: monospace;
                    font-size: 0.9em;
                }

                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin: 20px 0;
                }

                th, td {
                    border: 1px solid var(--border-color);
                    padding: 12px;
                    text-align: left;
                }

                th {
                    background-color: var(--secondary-color);
                    font-weight: bold;
                }

                @media (max-width: 768px) {
                    .container {
                        padding: 10px;
                    }
                    
                    h1 {
                        font-size: 2em;
                    }
                    
                    h2 {
                        font-size: 1.5em;
                    }
                }
            ";
        }

        private string GetDefaultJavaScript()
        {
            return @"
                // Smooth scrolling pour les liens internes
                document.querySelectorAll('a[href^=""#""]').forEach(anchor => {
                    anchor.addEventListener('click', function (e) {
                        e.preventDefault();
                        const target = document.querySelector(this.getAttribute('href'));
                        if (target) {
                            target.scrollIntoView({ behavior: 'smooth' });
                        }
                    });
                });

                // Highlight code blocks
                document.querySelectorAll('.code-block').forEach(block => {
                    block.style.position = 'relative';
                });
            ";
        }

        private string GetTopicContent(string topicId)
        {
            var contentMap = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new()
                {
                    ["getting-started"] = @"
                        <h2>Welcome to Odyssey!</h2>
                        <p>Odyssey is a powerful mapping application designed to help you visualize and analyze geographic data with ease.</p>
                        
                        <h3>First Steps</h3>
                        <ol>
                            <li>Launch the application</li>
                            <li>Open your first map file using <span class='keyboard-shortcut'>Ctrl+O</span></li>
                            <li>Explore the interface and tools</li>
                        </ol>

                        <div class='info-box'>
                            <strong>💡 Tip:</strong> Press <span class='keyboard-shortcut'>F1</span> at any time to access this help system.
                        </div>
                    ",
                    ["user-interface"] = @"
                        <h2>Understanding the Interface</h2>
                        <p>The Odyssey interface is designed for maximum productivity and ease of use.</p>
                        
                        <h3>Main Components</h3>
                        <ul>
                            <li><strong>Menu Bar:</strong> Access all application functions</li>
                            <li><strong>Toolbar:</strong> Quick access to common tools</li>
                            <li><strong>Map Canvas:</strong> Main viewing area</li>
                            <li><strong>Side Panels:</strong> Properties and data views</li>
                        </ul>
                    ",
                    ["file-operations"] = @"
                        <h2>Working with Files</h2>
                        <p>Odyssey supports various file formats for importing and exporting map data.</p>
                        
                        <h3>Supported Formats</h3>
                        <table>
                            <tr><th>Format</th><th>Extension</th><th>Description</th></tr>
                            <tr><td>Odyssey Native</td><td>.csmx</td><td>Native format with full feature support</td></tr>
                            <tr><td>Text Report</td><td>.txt</td><td>Plain text reports</td></tr>
                        </table>

                        <div class='code-block'>
// Example: Loading a file programmatically
var result = ReportFileService.LoadFile(pathname, out report, out error);
if (result) {
    Console.WriteLine(""File loaded successfully"");
}
                        </div>
                    ",
                    ["keyboard-shortcuts"] = @"
                        <h2>Keyboard Shortcuts</h2>
                        <p>Master these shortcuts to boost your productivity:</p>
                        
                        <table>
                            <tr><th>Action</th><th>Shortcut</th></tr>
                            <tr><td>Open File</td><td><span class='keyboard-shortcut'>Ctrl+O</span></td></tr>
                            <tr><td>Save File</td><td><span class='keyboard-shortcut'>Ctrl+S</span></td></tr>
                            <tr><td>Help</td><td><span class='keyboard-shortcut'>F1</span></td></tr>
                            <tr><td>Exit</td><td><span class='keyboard-shortcut'>Alt+F4</span></td></tr>
                        </table>
                    "
                },
                ["fr"] = new()
                {
                    ["getting-started"] = @"
                        <h2>Bienvenue dans Odyssey !</h2>
                        <p>Odyssey est une application de cartographie puissante conçue pour vous aider à visualiser et analyser des données géographiques facilement.</p>
                        
                        <h3>Premiers Pas</h3>
                        <ol>
                            <li>Lancez l'application</li>
                            <li>Ouvrez votre premier fichier carte avec <span class='keyboard-shortcut'>Ctrl+O</span></li>
                            <li>Explorez l'interface et les outils</li>
                        </ol>

                        <div class='info-box'>
                            <strong>💡 Astuce :</strong> Appuyez sur <span class='keyboard-shortcut'>F1</span> à tout moment pour accéder à ce système d'aide.
                        </div>
                    ",
                    ["keyboard-shortcuts"] = @"
                        <h2>Raccourcis Clavier</h2>
                        <p>Maîtrisez ces raccourcis pour améliorer votre productivité :</p>
                        
                        <table>
                            <tr><th>Action</th><th>Raccourci</th></tr>
                            <tr><td>Ouvrir Fichier</td><td><span class='keyboard-shortcut'>Ctrl+O</span></td></tr>
                            <tr><td>Sauvegarder</td><td><span class='keyboard-shortcut'>Ctrl+S</span></td></tr>
                            <tr><td>Aide</td><td><span class='keyboard-shortcut'>F1</span></td></tr>
                            <tr><td>Quitter</td><td><span class='keyboard-shortcut'>Alt+F4</span></td></tr>
                        </table>
                    "
                }
            };

            var langContent = contentMap.TryGetValue(_currentLanguage, out var content) 
                ? content 
                : contentMap["en"];

            return langContent.TryGetValue(topicId, out var topicContent) 
                ? topicContent 
                : $"<p>Content for '{topicId}' not available in {_currentLanguage}.</p>";
        }
    }

    // Data classes
    public class TopicData
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Order { get; set; }
    }
}