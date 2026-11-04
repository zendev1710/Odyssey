using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using System.IO;
using Odyssey.Extensions;
using Odyssey.Utils;

namespace Odyssey.ViewModels;

internal static class StorageService
{
    public const string XmlExt = "xml";
    public const string TxtExt = "txt";
    public const string ZipExt = "zip";
    public const string JsonExt = "json";

    public const string CrExt = "cr";
    public const string BookmarksExt = XmlExt;

    public static readonly string XmlPattern = FileUtils.GetFileExtensionPattern(XmlExt);
    public static readonly string CrPattern = FileUtils.GetFileExtensionPattern(CrExt);
    public static readonly string TxtPattern = FileUtils.GetFileExtensionPattern(TxtExt);
    public static readonly string ZipPattern = FileUtils.GetFileExtensionPattern(ZipExt);
    public static readonly string JsonPattern = FileUtils.GetFileExtensionPattern(JsonExt);

    public static readonly string PlainTextMimeType = "text/plain";
    public static readonly string ZipMimeType = $"application/{ZipExt}";
    public static readonly string JsonMimeType = $"application/{JsonExt}";
    public static readonly string XmlMimeType = $"application/{XmlExt}";

    public static readonly string PlainTextAppleUniformTypeIdentifiers = "public.plain-text";
    public static readonly string ZipAppleUniformTypeIdentifiers = $"public.{ZipExt}";
    public static readonly string JsonAppleUniformTypeIdentifiers = $"public.{JsonExt}";
    public static readonly string XmlAppleUniformTypeIdentifiers = $"public.{XmlExt}";

    public static FilePickerFileType All { get; } = new("All")
    {
        Patterns = ["*.*"],
        MimeTypes = ["*/*"]
    };

    public static FilePickerFileType Json { get; } = new(JsonExt.ToTitleCase())
    {
        Patterns = [JsonPattern],
        AppleUniformTypeIdentifiers = [JsonAppleUniformTypeIdentifiers],
        MimeTypes = [JsonMimeType]
    };

    public static FilePickerFileType Text { get; } = new(TxtExt.ToTitleCase())
    {
        Patterns = [TxtPattern],
        AppleUniformTypeIdentifiers = [PlainTextAppleUniformTypeIdentifiers],
        MimeTypes = [PlainTextMimeType]
    };

    public static FilePickerFileType Xml { get; } = new(XmlExt.ToTitleCase())
    {
        Patterns = [XmlPattern],
        AppleUniformTypeIdentifiers = [XmlAppleUniformTypeIdentifiers],
        MimeTypes = [XmlMimeType]
    };

    public static FilePickerFileType Report { get; } = new(CrExt.ToTitleCase())
    {
        Patterns = [CrPattern],
        AppleUniformTypeIdentifiers = [PlainTextAppleUniformTypeIdentifiers],
        MimeTypes = [PlainTextMimeType]
    };

    public static FilePickerFileType Zip { get; } = new(ZipExt.ToTitleCase())
    {
        Patterns = [ZipPattern],
        AppleUniformTypeIdentifiers = [ZipAppleUniformTypeIdentifiers],
        MimeTypes = [ZipMimeType]
    };

    public static FilePickerFileType Orders { get { return Text; } }

    public static FilePickerFileType Bookmarks { get { return Xml; } }

    public static FilePickerFileType ReportEresseaFiles { get; } = new("Report Eressea files")
    {
        Patterns = new[] { CrPattern, ZipPattern },
        AppleUniformTypeIdentifiers = [PlainTextAppleUniformTypeIdentifiers, ZipAppleUniformTypeIdentifiers],
        MimeTypes = new[] { PlainTextMimeType, PlainTextMimeType }
    };

    public static FilePickerFileType EresseaFiles { get; } = new("Eressea files")
    {
        Patterns = new[] { CrPattern, TxtPattern, ZipPattern },
        AppleUniformTypeIdentifiers = [PlainTextAppleUniformTypeIdentifiers, PlainTextAppleUniformTypeIdentifiers, ZipAppleUniformTypeIdentifiers],
        MimeTypes = new[] { PlainTextMimeType, PlainTextMimeType, ZipMimeType }
    };

    public static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            return window.StorageProvider;
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
        {
            var visualRoot = mainView.GetPresentationSource().RootVisual;
            if (visualRoot is TopLevel topLevel)
            {
                return topLevel.StorageProvider;
            }
        }

        return null;
    }
}
