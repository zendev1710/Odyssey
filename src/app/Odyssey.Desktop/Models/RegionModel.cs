
using Odyssey.Models.Data;
using Odyssey.Models.Documents;

namespace Odyssey.Models;

public class RegionModel : EntityModel, ILocation
{
    private int X { get; set; }
    private int Y { get; set; }
    private int Plane { get; }
    private int _terrain;
    private int _flags;

    int ILocation.X
    {
        get => X;
        set => X = value;
    }
    int ILocation.Y
    {
        get => Y;
        set => Y = value;
    }
    
    PlaneType ILocation.Plane => (PlaneType)Plane;

    public int GetX() => X;
    public int GetY() => Y;
    public int GetPlane() => Plane;

    public int Terrain => _terrain;
    public int Flags => _flags;

    public bool IsWithPeople => (Flags & (int)Flag.REGION_SEEN) != 0;

    public string Name { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string IslandName { get; private set; } = string.Empty;

    // TODO: set island id instead of 0 when island is defined for the region
    public RegionModel(in RegionDataBlock region) : base(region, 0)
    {
        X = region.GetX();
        Y = region.GetY();
        Plane = region.GetPlane();
        _id = Coordinates.GetId(X, Y, (PlaneType)Plane);
        //_encodedId = Utils.Converters.IdToString(_id);
        // TODO: later, we should set an island Id
        _locationId = _id;
    }

    public RegionDataBlock Region { get => (DataBlock as RegionDataBlock)!; }

    public bool IsVisible(out Flag visibilityFlag)
    {
        visibilityFlag = Flag.None;
        if ((Flags & (int)Flag.REGION_SEEN) != 0)
        {
            visibilityFlag |= Flag.REGION_SEEN;
        }
        else if ((Flags & (int)Flag.TRAVEL) != 0)
        {
            visibilityFlag |= Flag.TRAVEL;
        }
        else if ((Flags & (int)Flag.LIGHTHOUSE) != 0)
        {
            visibilityFlag |= Flag.LIGHTHOUSE;
        }
        return visibilityFlag != Flag.None;
    }

    public void CollectData()
    {
        _terrain = Region.GetTerrain();
        _flags = Region.GetFlags();
        Label = Region.GetUILabel();

        string name = Region.Value(KeyType.NAME);
        if (string.IsNullOrEmpty(name))
        {
            // fallback to lowercase name or coordinates if no name
            name = Region.Value(KeyType.LOWERCASE_NAME);
        }
        Name = name;

        // TODO: set main properties :
        // - Name
        // - Description...
    }

    internal void UpdateIslandName()
    {
        IslandName = (Region.GetAttachment() as RegionAttachment)?.Island ?? string.Empty;
    }
}
