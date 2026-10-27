using System;
using System.IO;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Platform;
using Odyssey.Settings;

namespace Odyssey.Services;

public static class IconAssetResolver
{
    public static string? ResolvePreferredPath(string? assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return null;
        }

        try
        {
            var configured = GlobalSettings.Get<int>(GlobalSettings.ICON_ASSET_FORMAT);
            if (Enum.IsDefined(typeof(IconAssetFormatMode), configured))
            {
                return ResolvePreferredPath(assetPath, (IconAssetFormatMode)configured);
            }
        }
        catch
        {
            // Fall back to the transparent PNG default below.
        }

        return ResolvePreferredPath(assetPath, IconAssetFormatMode.TransparentPng);
    }

    public static string? ResolvePreferredPath(string? assetPath, IconAssetFormatMode format)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return null;
        }

        var normalized = assetPath.Replace('\\', '/').Trim();
        if (!normalized.StartsWith("/Assets/", StringComparison.OrdinalIgnoreCase) && !normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return assetPath;
        }

        var relative = normalized.TrimStart('/');
        if (relative.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            relative = relative.Substring("Assets/".Length);
        }

        var directory = Path.GetDirectoryName(relative)?.Replace('\\', '/') ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relative);
        var extensions = GetPreferredExtensions(format);
        bool useDarkVariant = ShouldUseDarkVariant();

        foreach (var extension in extensions)
        {
            if (useDarkVariant)
            {
                var darkCandidate = string.IsNullOrEmpty(directory)
                    ? $"{fileNameWithoutExtension}-dark{extension}"
                    : $"{directory}/{fileNameWithoutExtension}-dark{extension}";

                var darkCandidateUri = new Uri($"avares://Odyssey/Assets/{darkCandidate}");
                if (AssetLoader.Exists(darkCandidateUri))
                {
                    return "/Assets/" + darkCandidate.Replace('\\', '/');
                }
            }

            var candidate = string.IsNullOrEmpty(directory)
                ? $"{fileNameWithoutExtension}{extension}"
                : $"{directory}/{fileNameWithoutExtension}{extension}";

            var candidateUri = new Uri($"avares://Odyssey/Assets/{candidate}");
            if (AssetLoader.Exists(candidateUri))
            {
                return "/Assets/" + candidate.Replace('\\', '/');
            }
        }

        return "/Assets/" + relative.Replace('\\', '/');
    }

    private static string[] GetPreferredExtensions(IconAssetFormatMode format) => format switch
    {
        IconAssetFormatMode.LegacyGif => [".gif"],
        IconAssetFormatMode.SvgTheme => [".svg", ".png", ".gif"],
        _ => [".png", ".gif"],
    };

    private static bool ShouldUseDarkVariant()
    {
        var app = Application.Current;
        if (app is null)
        {
            return false;
        }

        var actualTheme = app.ActualThemeVariant;
        return actualTheme == ThemeVariant.Dark;
    }
}
