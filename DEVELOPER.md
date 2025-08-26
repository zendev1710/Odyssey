# Development information

This application is developed with [Avalonia UI](https://docs.avaloniaui.net/docs/overview/what-is-avalonia) in C# language, using the [MVVM pattern](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/) and targeting .NET 9.0.

Code was inspired from the [CsMapFX](https://github.com/ennorehling/csmapfx) application (written in C/C++ language).

## Development features

## Dependencies

- [Avalonia UI](https://github.com/AvaloniaUI/Avalonia): cross-platform UI framework for Windows, Linux and MacOS operating systems
- [.NET Community Toolkit](https://github.com/CommunityToolkit/dotnet): helpes for .NET development, using the MVVM pattern, diagnostic and high performance helpers
- [Dock](https://github.com/wieslawsoltes/Dock):docking layout system for Avalonia desktop applications
- [Prism.Avalonia](https://github.com/AvaloniaCommunity/Prism.Avalonia): [Prism](https://prismlibrary.github.io/docs/) for Avalonia, used mainly for Event Aggregator
- [ConfigFactory.Avalonia](https://www.nuget.org/packages/ConfigFactory.Avalonia) for GUI settings pages edition, load and save
- [AsyncImageLoader.Avalonia](https://www.nuget.org/packages/AsyncImageLoader.Avalonia/): asynchroneous images loader and cache management
- [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit/): text editor for orders editing (unit orders view)
- [TextMateSharp](https://github.com/danipen/TextMateSharp): grammar for auto-completion and syntax color for orders editing  (unit orders view)

## How-to

## Notes and references for development

### Detect Windows system theme (in InitializeThemes())

    var uiSettings = new UISettings();
    var isSystemDarkTheme = uiSettings.GetColorValue(UIColorType.Background).ToString() == "#FF000000";

### Get ActiveDocument using Dock system (in ToolViewModelBase::GetDocument())

            // TODO: set Context somewhere because always null here
            if (Context is IRootDock root && root.ActiveDockable is IDock active)
            {
                var rf = root.Factory;
                var rc = rf?.ContextLocator;
                var rd = rf?.DockableLocator;
                var rfiles = rf?.GetDockable<IDocumentDock>("Files");

                var activeFile = rfiles?.ActiveDockable as FileViewModel;
                if (activeFile != null)
                {
                    return activeFile.Cr;
                }

                if (active.Factory?.FindDockable(active, (d) => d.Id == "Files") is IDock files)
                {
                    var f = files.ActiveDockable as FileViewModel;
                    if (f != null) {
                        return f.Cr;
                    }
                }
            }
            return new CRDocument();

## Greetings

Some parts of this code have been inspired from other works available in the following GitHub repositories:

- [DevToys](https://github.com/DevToys-app/DevToys) (settings management, in particular theme handling)

