using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Assets;
using Odyssey.Icons;
using Odyssey.Services;
using Avalonia;
using Avalonia.Media;

namespace Odyssey.ViewModels.Tools;

public partial class UnitSkillViewModel : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private int _change;

    public string LevelText => Level.ToString();
    public string TooltipLabel
    {
        get
        {
            string key = Change > 0
                ? "unit_skill_tooltip_increased"
                : Change < 0
                    ? "unit_skill_tooltip_decreased"
                    : "unit_skill_tooltip_unchanged";
            string template = Lang.ResourceManager.GetString(key, Lang.Culture)
                ?? "{0}: level {1}";
            return string.Format(Lang.Culture, template, Name, Level, Math.Abs(Change));
        }
    }

    public string LevelBackground => Change > 0 ? "#2f8f46" : Change < 0 ? "#a33c3c" : "#4b5563";
    public IBrush ChangeBorderBrush => Change > 0 ? Brushes.ForestGreen : Change < 0 ? Brushes.Firebrick : new SolidColorBrush(Color.Parse("#cbd5e1"));
    public Thickness ChangeBorderThickness => new(4);

    public string IconPath => string.IsNullOrEmpty(Key)
        ? string.Empty
        : IconCatalogService.ResolvePath(IconCategories.Skill, Key, "skills") ?? string.Empty;

    public IBrush ChangeCircleBackground => Change > 0
        ? new SolidColorBrush(Color.FromArgb(13, 46, 160, 67))
        : Change < 0
            ? new SolidColorBrush(Color.FromArgb(13, 190, 45, 45))
            : new SolidColorBrush(Color.Parse("#cfe6f0"));

    public IBrush ChangeCircleOverlay => Change > 0
        ? new SolidColorBrush(Color.FromArgb(102, 46, 160, 67))
        : Change < 0
            ? new SolidColorBrush(Color.FromArgb(102, 190, 45, 45))
            : Brushes.Transparent;

    public UnitSkillViewModel(string key, string displayName, int level, int change = 0)
    {
        Key = key ?? string.Empty;
        Name = string.IsNullOrWhiteSpace(displayName) ? Key : displayName;
        Level = level;
        Change = change;
    }
}
