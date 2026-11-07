using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Odyssey.Models.Localization;
using Odyssey.Settings;
using Odyssey.ViewModels;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels.Tools;

internal static class UnitSkillSorter
{
    public static void Sort(ObservableCollection<UnitSkillViewModel> skillItems)
    {
        int sortMode = GlobalSettings.Get<int>(GlobalSettings.UNIT_SKILL_SORT_MODE);
        bool groupByCategory = GlobalSettings.Get<bool>(GlobalSettings.UNIT_SKILL_GROUP_BY_CATEGORY);
        string[] categoryOrder = SkillCategoryCatalog.NormalizeCategoryOrder(
            (GlobalSettings.Get<string>(GlobalSettings.UNIT_SKILL_CATEGORY_ORDER) ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        Dictionary<string, int> categoryOrderIndexes = categoryOrder
            .Select((key, index) => new { key, index })
            .ToDictionary(item => item.key, item => item.index, StringComparer.Ordinal);
        IOrderedEnumerable<UnitSkillViewModel> sortedItems = skillItems
            .OrderBy(item => groupByCategory
                ? SkillCategoryCatalog.GetCategoryOrderIndex(item.Key, categoryOrderIndexes)
                : 0);
        StringComparer nameComparer = StringComparer.CurrentCultureIgnoreCase;

        if (sortMode == 1)
        {
            sortedItems = sortedItems
                .ThenBy(item => Labels.Localize(Categories.Skill, item.Key), nameComparer)
                .ThenBy(item => item.Key, StringComparer.Ordinal);
        }
        else if (sortMode == 2)
        {
            string[] customOrder = (GlobalSettings.Get<string>(GlobalSettings.UNIT_SKILL_CUSTOM_ORDER) ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Dictionary<string, int> orderIndexes = customOrder
                .Distinct(StringComparer.Ordinal)
                .Select((key, index) => new { key, index })
                .ToDictionary(item => item.key, item => item.index, StringComparer.Ordinal);

            sortedItems = sortedItems
                .ThenBy(item => orderIndexes.TryGetValue(item.Key, out int index) ? index : int.MaxValue)
                .ThenBy(item => Labels.Localize(Categories.Skill, item.Key), nameComparer)
                .ThenBy(item => item.Key, StringComparer.Ordinal);
        }
        else
        {
            sortedItems = sortedItems
                .ThenByDescending(item => item.Level)
                .ThenBy(item => Labels.Localize(Categories.Skill, item.Key), nameComparer)
                .ThenBy(item => item.Key, StringComparer.Ordinal);
        }

        UnitSkillViewModel[] orderedItems = sortedItems.ToArray();
        skillItems.Clear();
        foreach (UnitSkillViewModel item in orderedItems)
        {
            skillItems.Add(item);
        }
    }
}
