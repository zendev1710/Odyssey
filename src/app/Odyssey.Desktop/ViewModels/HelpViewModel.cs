
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExCSS;
using LiveMarkdown.Avalonia;
using Odyssey.Core.Help.Dto;
using Odyssey.Core.Help.Models;
using Odyssey.Core.Services;
using Odyssey.Models.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels;

public partial class HelpViewModel : ObservableObject
{
    public ObservableStringBuilder MarkdownBuilder { get; } = new();

    public ObservableCollection<HelpTocItem> TocItems { get; }

    [ObservableProperty]
    private HelpTocItem? selectedItem;

    private readonly Dictionary<string, string> _markdownFilesContentCache = new();
    //private CancellationTokenSource? cancellationTokenSource;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    private bool _isSidebarVisible = true;

    [RelayCommand]
    public void ToggleSidebar() => IsSidebarVisible = !IsSidebarVisible;

    partial void OnSelectedItemChanged(HelpTocItem? value)
    {
        if (value != null)
        { 
            string markdown = LoadMarkdown(value);
            MarkdownBuilder.Clear();
            MarkdownBuilder.Append(markdown);
            //_ = RenderMarkdownAsync(markdownName);
        }
    }

    public HelpViewModel(ILanguageService lang)
    {
        _languageService = lang;
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Config", "help-topics.json");
        TocItems = LoadTocItems(path);

        /*
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
        */

        /*
        // We don't use embedded resources here to allow easy modification of sample files.
        var markdownFolderPath = Path.Combine(AppContext.BaseDirectory, "samples");
        foreach (var markdownFilePath in Directory.EnumerateFiles(markdownFolderPath, "*.md")
                     .OrderByDescending(path => path.EndsWith("README.md", StringComparison.OrdinalIgnoreCase))
                     .ThenBy(path => path))
        {
            var fileName = Path.GetFileNameWithoutExtension(markdownFilePath);
            NavigationItems.Add(
                new NavigationBarItem
                {
                    Content = fileName,
                    Route = fileName
                });
        }
        */
        SelectedItem = TocItems.FirstOrDefault();
    }

    public ObservableCollection<HelpTocItem> LoadTocItems(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return new ObservableCollection<HelpTocItem>();

        var json = File.ReadAllText(jsonPath);

        var dtos = JsonSerializer.Deserialize<List<HelpTopicDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (dtos is null)
            return new ObservableCollection<HelpTocItem>();

        return new ObservableCollection<HelpTocItem>(
            dtos.Select(BuildTocItem)
        );
    }

    private HelpTocItem BuildTocItem(HelpTopicDto dto)
    {
        var item = new HelpTocItem
        {
            Title = Labels.Localize(Categories.HelpTopic, dto.Title),
            Id = dto.Id
        };

        if (dto.Children != null)
        {
            foreach (var child in dto.Children)
                item.Children.Add(BuildTocItem(child));
        }

        return item;
    }

    private string LoadMarkdown(HelpTocItem item)
    {
        var uri = ResolveResourceName(item.Id);

        // _markdownFilesContentCache with uris (not item id) as keys
        if (_markdownFilesContentCache.TryGetValue(uri, out var cachedContent))
            return cachedContent;

        // Utilisation de AssetLoader pour lire la ressource Avalonia
        try
        {
            using var stream = AssetLoader.Open(new Uri(uri));
            using var reader = new StreamReader(stream);

            var content = reader.ReadToEnd();
            _markdownFilesContentCache[uri] = content;
        }
        catch (Exception ex)
        {
            // Gérer les erreurs de chargement de la ressource
            Console.Error.WriteLine($"Error loading markdown resource '{uri}': {ex.Message}");
            return $"# Error\nCould not load content for '{item.Title}'.";
        }

        return "";
    }

    private string ResolveResourceName(string id)
    {
        string filename = id;
        if (!filename.Contains('/'))
            filename += "/index";
        var lang = _languageService.CurrentUILanguageCode;
        var uri = $"avares://Odyssey/Assets/Help/Content/{lang}/{filename}.md";
        return uri;
    }

    private string ResolveMarkdownPath(string id, string langCode)
    {
        var lang = _languageService.CurrentUILanguageCode;
        var relative = id.Replace('/', System.IO.Path.DirectorySeparatorChar);

        // Parent → index.md
        var parentDir = System.IO.Path.Combine("help", langCode, relative);
        var indexPath = System.IO.Path.Combine(parentDir, "index.md");
        if (File.Exists(indexPath))
            return indexPath;

        // Enfant → id.md
        return System.IO.Path.Combine("help", langCode, $"{relative}.md");
    }

    private string GetMarkdownPath(HelpTocItem item, string langCode)
    {
        return ResolveMarkdownPath(item.Id, langCode);
    }


    [RelayCommand]
    private static async Task OpenUriAsync(LinkClickedEventArgs args)
    {
        if (args.HRef is { IsAbsoluteUri: true, Scheme: "http" or "https" } url)
        {
            await LaunchUriAsync(url);
        }
    }

    private async static Task LaunchUriAsync(Uri uri)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            var launcher = TopLevel.GetTopLevel(window)?.Launcher;
            if (launcher is not null)
            {
                await launcher.LaunchUriAsync(uri);
            }
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
        {
            var launcher = TopLevel.GetTopLevel(mainView)?.Launcher;
            if (launcher is not null)
            {
                await launcher.LaunchUriAsync(uri);
            }
        }
    }

}
