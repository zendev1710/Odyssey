using Odyssey.Models.Documents;
using System;


namespace Odyssey.Models.Data;

public class BattleDataBlock : DataBlock, ILocation
{
    // TODO: should resolve location obtaining a regiondatablock and keep it
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

    public BattleDataBlock(string id) : base(id)
    {
        Type = BlockType.BATTLE;
        SetLocation(id);
    }

    protected override string GetLabelSuffix()
    {
        return ((ILocation)this).Label;
    }

    private void SetLocation(string id)
    {
        // Note: only REGION and BATTLE blocks have X Y Id values
        int idValue;
        SetFlags((int)Flag.BLOCKID_BIT1);
        if (Utils.Converters.ExtractCoordinates(id, out int x, out int y, out idValue) && idValue > 0)
        {
            // special plane
            SetFlags((int)Flag.BLOCKID_BIT0);
        }
        X = x;
        Y = y;

    }
}
