using Newtonsoft.Json.Linq;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.Models.Data;

public class RegionDataBlock : DataBlock, ILocation, IBookmarkable
{
    protected int X { get; set; }
    protected int Y { get; set; }
    protected PlaneType Plane { get; }

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
    PlaneType ILocation.Plane => (PlaneType)Id;

    public int GetX() => X;
    public int GetY() => Y;
    public int GetPlane() => (int)Plane;

    public override string IdToString()
    {
        return $"{X}, {Y}";
    }

    public RegionDataBlock(string id): base(id) {
        Type = BlockType.REGION;
        SetLocation(id);
    }

    protected override string GetLabelSuffix()
    {
        return ((ILocation)this).Label;
    }

    private void SetLocation(string id)
    {
        // Note: only REGION and BATTLE blocks have X Y Id values
        int planeValue;
        SetFlags((int)Flag.BLOCKID_BIT1);
        if (Utils.Converters.ExtractCoordinates(id, out int x, out int y, out planeValue) && planeValue > 0)
        {
            // special plane
            SetFlags((int)Flag.BLOCKID_BIT0);
        }
        X = x;
        Y = y;
        // TODO: distinguish plane and Id
        Id = planeValue;
    }

    // return true if region is visible
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

    // return true if region has people
    public bool IsWithPeople() => (Flags & (int)Flag.REGION_SEEN) != 0;
}    
