// =============================================
// 1. HelpSystem.cs - Gestionnaire principal
// =============================================
using Avalonia.Controls;
using Avalonia.Threading;
using CsMapX.Views;
using Markdown.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CsMapX.Help
{
    public class HelpSystem
    {
        private static HelpSystem? _instance;
        private readonly Dictionary<string, HelpTopic> _topics = new();
        private string _currentLanguage = "en";
        private HelpWindow? _helpWindow;

        public static HelpSystem Instance => _instance ??= new HelpSystem();

        // LATER: use App currrent language
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                RefreshContent();
            }
        }

        public event Action<string>? LanguageChanged;

        private HelpSystem()
        {
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

        private void LoadTopics()
        {
            var languages = new[] { "en", "fr", "de", "es" };

            foreach (var lang in languages)
            {
                var contentPath = Path.Combine("Assets", "Help", "Content", lang);
                if (Directory.Exists(contentPath))
                {
                    LoadTopicsForLanguage(lang, contentPath);
                }
            }
        }

        private void LoadTopicsForLanguage(string language, string contentPath)
        {
            var topics = new[]
            {
                new HelpTopic("getting-started", "Getting Started", "🚀", 1, language),
                new HelpTopic("user-interface", "User Interface", "🖥️", 2, language),
                new HelpTopic("file-operations", "File Operations", "📁", 3, language),
                new HelpTopic("advanced-features", "Advanced Features", "⚙️", 4, language),
                new HelpTopic("troubleshooting", "Troubleshooting", "🔧", 5, language),
                new HelpTopic("keyboard-shortcuts", "Keyboard Shortcuts", "⌨️", 6, language),
                new HelpTopic("about", "About", "ℹ️", 7, language)
            };

            foreach (var topic in topics)
            {
                var filePath = Path.Combine(contentPath, $"{topic.Id}.md");
                if (File.Exists(filePath))
                {
                    topic.FilePath = filePath;
                }
                else
                {
                    // Contenu par défaut si le fichier n'existe pas
                    topic.Content = CreateDefaultContent(topic.Id, language);
                }

                _topics[$"{language}_{topic.Id}"] = topic;
            }
        }

        private string CreateDefaultContent(string topicId, string language)
        {
            var translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new()
                {
                    ["getting-started"] = "# Getting Started\n\nWelcome to CsMapX! This section will help you get started with the application.",
                    ["user-interface"] = "# User Interface\n\nLearn about the main interface elements and how to navigate the application.",
                    ["file-operations"] = "# File Operations\n\nHow to open, save, and manage files in CsMapX.",
                    ["advanced-features"] = "# Advanced Features\n\nExplore the advanced capabilities of CsMapX.",
                    ["troubleshooting"] = "# Troubleshooting\n\nCommon issues and how to resolve them.",
                    ["keyboard-shortcuts"] = "# Keyboard Shortcuts\n\n| Action | Shortcut |\n|--------|----------|\n| Open File | Ctrl+O |\n| Save File | Ctrl+S |\n| Help | F1 |",
                    ["about"] = "# About CsMapX\n\nCsMapX is a powerful mapping application built with Avalonia UI."
                },
                ["fr"] = new()
                {
                    ["getting-started"] = "# Démarrage\n\nBienvenue dans CsMapX ! Cette section vous aidera à démarrer avec l'application.",
                    ["user-interface"] = "# Interface Utilisateur\n\nDécouvrez les éléments principaux de l'interface et comment naviguer dans l'application.",
                    ["file-operations"] = "# Opérations sur les Fichiers\n\nComment ouvrir, sauvegarder et gérer les fichiers dans CsMapX.",
                    ["advanced-features"] = "# Fonctionnalités Avancées\n\nExplorez les capacités avancées de CsMapX.",
                    ["troubleshooting"] = "# Dépannage\n\nProblèmes courants et comment les résoudre.",
                    ["keyboard-shortcuts"] = "# Raccourcis Clavier\n\n| Action | Raccourci |\n|--------|----------|\n| Ouvrir Fichier | Ctrl+O |\n| Sauvegarder | Ctrl+S |\n| Aide | F1 |",
                    ["about"] = "# À Propos de CsMapX\n\nCsMapX est une application de cartographie puissante construite avec Avalonia UI."
                }
            };

            return translations.TryGetValue(language, out var langDict) && langDict.TryGetValue(topicId, out var content)
                ? content
                : $"# {topicId}\n\nContent not available in {language}.";
        }

        private void RefreshContent()
        {
            LanguageChanged?.Invoke(_currentLanguage);
        }
    }
}