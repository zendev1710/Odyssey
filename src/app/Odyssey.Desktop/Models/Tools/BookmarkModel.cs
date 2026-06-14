using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Utils;
using System.Xml.Linq;

public class BookmarkModel
{
    public string Type { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Label { get; private set; }
    public DataBlock Target { get; set; }

    private const string UnknownTypeValue = "UNKNOWN";
    private const string RegionTypeValue = "REGION";
    private const string UnitTypeValue = "UNIT";
    private const string ShipTypeValue = "SHIP";
    private const string BuildingTypeValue = "BUILDING";
    private const string IslandTypeValue = "ISLAND";

    public BookmarkModel(string type, DataBlock dt)
    {
        // FIXME: dt is null when loading from OpenLayout() deserialization
        if (dt is not null)
        {
            Id = dt.GetStringId();
            Name = dt.GetUIName();
            Label = dt.GetUILabel();
        }
        Type = type;
        Target = dt;
    }

    public static BookmarkModel? FromObject(DataBlock dt)
    {
        var bt = dt.GetBlockType();
        return bt switch
        {
            BlockType.ISLAND => new BookmarkModel(IslandTypeValue, dt),
            BlockType.REGION => new BookmarkModel(RegionTypeValue, dt),
            BlockType.UNIT => new BookmarkModel(UnitTypeValue, dt),
            BlockType.SHIP => new BookmarkModel(ShipTypeValue, dt),
            BlockType.BUILDING => new BookmarkModel(BuildingTypeValue, dt),
            _ => new BookmarkModel(UnknownTypeValue, dt),
        };
    }

    public static BookmarkModel? FromXml(string? type, string? id, string? name, CRDocument? cr)
    {
        if (type == null || id == null || cr == null)
        {
            return null;
        }
        DataBlock? obj = null;
        if (type == RegionTypeValue)
        {
            // TODO: handle island type id
            if (Converters.ExtractCoordinatesWithComma(id, out int x, out int y))
            {
                obj = cr.FindRegionFromPosition(x, y, 0);
            }
        }
        else if (type == IslandTypeValue)
        {
            obj = cr.FindIsland(Converters.StringToInt(id));
        } 
        else
        {
            int key = Converters.DecodeBase36(id);
            obj = type switch
            {
                UnitTypeValue => cr.FindUnit(key),
                ShipTypeValue => cr.FindShip(key),
                BuildingTypeValue => cr.FindBuilding(key),
                _ => null,
            };
        }
        if (obj == null) 
        { 
            return null; 
        }
        return new BookmarkModel(type, obj);
    }

    public XElement ToXmlElement()
    {
        return new XElement("bookmark",
            new XAttribute("type", Type),
            new XAttribute("id", Id),
            new XAttribute("name", Name)
        );
    }

    public bool IsSameObject(object obj)
    {
        if (obj is DataBlock db)
        {
            return db.GetStringId() == Id;
        }
        return false;
    }

    public override string ToString()
    {
        return Label!;
    }
}