
using Odyssey.Models.Data;
using System.Collections.Generic;

namespace Odyssey.Models;

public class RegionModel
{
    public List<BuildingModel> Buildings { get; private set; } = [];
    public List<ShipModel> Ships { get; private set; } = [];

    public RegionModel(DataBlock region)
    {
    }

    public void AddBuilding(BuildingModel buildingModel)
    {
        Buildings.Add(buildingModel);
    }

    public void AddShip(ShipModel shipModel)
    {
        Ships.Add(shipModel);
    }
}
