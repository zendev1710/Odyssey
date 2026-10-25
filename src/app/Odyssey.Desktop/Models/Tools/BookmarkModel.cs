using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// A bookmark on an entity of a CR document: a region, a unit, a ship or a building.
/// The bookmarked entity is an <see cref="EntityModel"/> of the document.
/// The data block of the entity is only used to identify it in the bookmarks file, and to label it.
/// </summary>
public class BookmarkModel
{
    public enum BookmarkType
    {
        Unknown,
        Region,
        Unit,
        Ship,
        Building,
        Island
    }

    public BookmarkType Type { get; private set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Label { get; private set; }

    /// <summary>
    /// The bookmarked entity, or null when it cannot be resolved in the current document
    /// (an entity that does not exist any more, or a kind of entity having no entity model, as islands).
    /// </summary>
    public EntityModel? Target { get; private set; }

    private const string UnknownTypeValue = "UNKNOWN";
    private const string RegionTypeValue = "REGION";
    private const string UnitTypeValue = "UNIT";
    private const string ShipTypeValue = "SHIP";
    private const string BuildingTypeValue = "BUILDING";
    private const string IslandTypeValue = "ISLAND";

    // The type of a bookmark is saved (and read) as one of the type values above. The comparison
    // is not case-sensitive: files saved by previous versions use the upper case values, and
    // files saved with the enum names ("Region", "Unit", ...) must still be read.
    private static readonly Dictionary<string, BookmarkType> TypeMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { UnknownTypeValue, BookmarkType.Unknown },
        { RegionTypeValue, BookmarkType.Region },
        { UnitTypeValue, BookmarkType.Unit },
        { ShipTypeValue, BookmarkType.Ship },
        { BuildingTypeValue, BookmarkType.Building },
        { IslandTypeValue, BookmarkType.Island }
    };

    public BookmarkModel(BookmarkType type, EntityModel target)
    {
        Type = type;
        // The data block of the entity gives the id it is saved with, and its name and label.
        Id = target.DataBlock.GetStringId();
        Name = target.DataBlock.GetUIName();
        Label = target.DataBlock.GetUILabel();
        Target = target;
    }

    /// <summary>
    /// Builds a bookmark on the specified entity. The type of the bookmark is deduced from the
    /// entity itself (see <see cref="GetBookmarkType"/>).
    /// </summary>
    public static BookmarkModel FromObject(EntityModel target)
    {
        return new BookmarkModel(GetBookmarkType(target), target);
    }

    /// <summary>
    /// Gets the type of bookmark that can be set on the specified entity: a region, a unit, a ship
    /// or a building, or <see cref="BookmarkType.Unknown"/> when the entity cannot be bookmarked.
    /// </summary>
    public static BookmarkType GetBookmarkType(EntityModel? entity)
    {
        // TODO: handle island ? handle faction in a given region ?
        return entity?.Type switch
        {
            EntityModel.EntityType.Region => BookmarkType.Region,
            EntityModel.EntityType.Unit => BookmarkType.Unit,
            EntityModel.EntityType.Ship => BookmarkType.Ship,
            EntityModel.EntityType.Building => BookmarkType.Building,
            _ => BookmarkType.Unknown
        };
    }

    /// <summary>
    /// Builds a bookmark from the values saved in a bookmarks file.
    /// </summary>
    /// <param name="type">type of the bookmarked entity, as saved</param>
    /// <param name="encodedId">id of the bookmarked entity, as saved (a position for a region)</param>
    /// <param name="name">name of the bookmarked entity, as saved</param>
    /// <param name="cr">the CR document the bookmark belongs to</param>
    /// <returns>null when the type is unknown or when the entity does not exist in the document</returns>
    public static BookmarkModel? FromXml(string? type, string? encodedId, string? name, CRDocument? cr)
    {
        if (type == null || encodedId == null || cr == null)
        {
            return null;
        }

        // The type saved in the file tells how to find the entity in the document; the type of the
        // bookmark is then the type of the found entity (see GetBookmarkType), so that it is always
        // coherent with the bookmarked entity.
        BookmarkType savedType = TypeMapping.GetValueOrDefault(type, BookmarkType.Unknown);
        EntityModel? target = ResolveTarget(savedType, encodedId, cr);
        if (target is null)
        {
            return null;
        }

        BookmarkModel bookmarkModel = new(GetBookmarkType(target), target);
        if (!string.IsNullOrEmpty(name))
        {
            // Keep the name saved in the file: it is the one saved back.
            bookmarkModel.Name = name;
        }
        return bookmarkModel;
    }

    /// <summary>
    /// Gets the entity of the specified document matching a bookmarked entity.
    /// Regions are saved as their position, the other entities as their encoded id.
    /// </summary>
    private static EntityModel? ResolveTarget(BookmarkType type, string encodedId, CRDocument cr)
    {
        if (type == BookmarkType.Region)
        {
            return Converters.ExtractCoordinatesWithComma(encodedId, out int x, out int y) &&
                   cr.FindRegion(x, y, PlaneType.WORLD, out RegionModel? region)
                ? region
                : null;
        }

        int key = Converters.DecodeBase36(encodedId);
        return type switch
        {
            BookmarkType.Unit => cr.GetUnit(key),
            BookmarkType.Ship => cr.GetShip(key),
            BookmarkType.Building => cr.GetBuilding(key),
            // Islands have no entity model (islands are not displayed yet)
            _ => null,
        };
    }

    /// <summary>
    /// Gets the type value a bookmark of the specified type is saved as.
    /// </summary>
    private static string GetTypeValue(BookmarkType type)
    {
        return type switch
        {
            BookmarkType.Region => RegionTypeValue,
            BookmarkType.Unit => UnitTypeValue,
            BookmarkType.Ship => ShipTypeValue,
            BookmarkType.Building => BuildingTypeValue,
            BookmarkType.Island => IslandTypeValue,
            _ => UnknownTypeValue
        };
    }

    public XElement ToXmlElement()
    {
        return new XElement("bookmark",
            new XAttribute("type", GetTypeValue(Type)),
            new XAttribute("id", Id),
            new XAttribute("name", Name)
        );
    }

    /// <summary>
    /// Returns true when the specified entity is the bookmarked one.
    /// </summary>
    public bool IsSameObject(EntityModel? entityModel)
    {
        // The id alone is not enough to identify the entity: the id spaces of units, ships and
        // buildings are distinct, so two entities of different types can have the same id.
        return entityModel is not null &&
               Type == GetBookmarkType(entityModel) &&
               entityModel.DataBlock.GetStringId() == Id;
    }

    public override string ToString()
    {
        return Label;
    }
}
