using Avalonia.Data.Converters;
using Odyssey.Icons;
using Odyssey.Models.Tools;
using Odyssey.Services;
using System;

namespace Odyssey.Converters
{
    public class UnitItemConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not DataProperty data)
            {
                return null;
            }

            return data.Category switch
            {
                DataProperty.Categories.Skill => IconCatalogService.ResolvePath(IconCategories.Skill, data.Name, "skills"),
                DataProperty.Categories.Item => IconCatalogService.ResolvePath(IconCategories.Item, data.Name, "weapons"),
                DataProperty.Categories.Effect => IconCatalogService.ResolvePath(IconCategories.Effect, data.Name),
                DataProperty.Categories.Status => IconCatalogService.ResolvePath(IconCategories.Status, data.Name),
                DataProperty.Categories.CombatStatus => IconCatalogService.ResolvePath(IconCategories.Status, data.Name),
                DataProperty.Categories.CombatSpell => IconCatalogService.ResolvePath(IconCategories.Spell, data.Name, "spell"),
                DataProperty.Categories.Spell => IconCatalogService.ResolvePath(IconCategories.Spell, data.Name, "spell"),
                DataProperty.Categories.Building => IconCatalogService.ResolvePath(IconCategories.Building, data.Name, "buildings"),
                DataProperty.Categories.Ship => IconCatalogService.ResolvePath(IconCategories.Ship, data.Name, "ships"),
                DataProperty.Categories.Race => IconCatalogService.ResolvePath(IconCategories.Race, data.Name, "race"),
                _ => null,
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
