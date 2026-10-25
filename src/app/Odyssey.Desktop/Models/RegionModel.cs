
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Numerics;

namespace Odyssey.Models;

public class RegionModel : EntityModel
{
    private int X { get; set; }
    private int Y { get; set; }
    private int Plane { get; }

    /*
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
    */

    public int GetX() => X;
    public int GetY() => Y;
    public int GetPlane() => Plane;

    public int Terrain { get => Region.GetTerrain(); }
    public int Flags { get => Region.GetFlags(); }

    public bool IsWithPeople => Region.IsWithPeople();

    public string Name { get; private set; } = string.Empty;

    public List<BuildingModel> Buildings { get; private set; } = [];
    public List<ShipModel> Ships { get; private set; } = [];

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
        return Region.IsVisible(out visibilityFlag);
    }

    public void AddBuilding(in BuildingModel buildingModel)
    {
        Buildings.Add(buildingModel);
    }

    public void AddShip(in ShipModel shipModel)
    {
        Ships.Add(shipModel);
    }

    public void CollectData()
    {
        // Get readable name from the datablock
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
        /*
        Region?.CollectData();
        foreach (var building in Buildings)
        {
            building.CollectData();
        }
        foreach (var ship in Ships)
        {
            ship.CollectData();
        }
        */
    }
}
