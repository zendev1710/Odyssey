using Odyssey.Icons;
using Odyssey.Models.Localization;
using Odyssey.Services;

namespace Odyssey.ViewModels;

public sealed class SkillOrderItemViewModel
{
    public string Key { get; }
    public string Name { get; }
    public string IconPath { get; }

    public SkillOrderItemViewModel(string key)
    {
        Key = key;
        Name = Labels.Localize(Odyssey.Models.Tools.DataProperty.Categories.Skill, key);
        IconPath = IconCatalogService.ResolvePath(IconCategories.Skill, key, "skills") ?? string.Empty;
    }
}