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
using Odyssey.Views;
using Odyssey.Core.Help.Models;

namespace Odyssey.Help
{
    public class HelpSystem
    {
        private static HelpSystem? _instance;
        private readonly Dictionary<string, HelpTocItem> _topics = new();
        private HelpView? _helpWindow;

        public static HelpSystem Instance => _instance ??= new HelpSystem();

        private HelpSystem()
        {
        }

        /*
        public async Task ShowHelp(string? topicId = null)
        {
            /*
            if (_helpWindow == null)
            {
                _helpWindow = new HelpView();
                _helpWindow.Closed += (s, e) => _helpWindow = null;
            }

            if (!string.IsNullOrEmpty(topicId))
            {
                await _helpWindow.NavigateToTopic(topicId);
            }

            _helpWindow.Show();
            _helpWindow.Activate();
 
        }
        */

        /*
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
        */
    }
}
