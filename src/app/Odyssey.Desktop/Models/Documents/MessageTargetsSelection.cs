using Odyssey.Models.Tools;
using System.Collections.Generic;
using static Odyssey.Models.EntityModel;

namespace Odyssey.Models.Documents;

public class MessageTargetsSelection : EntitySelection
{
    public UnitModel? Unit { get; private set; }
    public ShipModel? Ship { get; private set; }
    public BuildingModel? Building { get; private set; }

    // TODO: handle a IRegionResolver and receive it as a parameter
    public MessageTargetsSelection(List<EntityModel> targets/*, IRegionResolver regionResolver*/)
    {
        RegionModel? region = Region;
        //FactionModel? faction = null;
        foreach (EntityModel target in targets)
        {
            var bt = target.Type;
            if (bt == EntityType.Region)
            {
                region = target as RegionModel;
            }
            else if (bt == EntityType.Unit)
            {
                Unit = target as UnitModel;
            }
            else if (bt == EntityType.Ship)
            {
                Ship = target as ShipModel;
            }
            else if (bt == EntityType.Building)
            {
                Building = target as BuildingModel;
            }
        }

        // TODO: handle a IRegionResolver and give it as parameter
        EntityModel? entityModel = GetDefaultTarget();
        SetItem(entityModel, region/*, faction*/);
    }

    public EntityModel? GetDefaultTarget()
    {
        if (Unit is not null) { return Unit; }
        if (Ship is not null) { return Ship; }
        if (Building is not null) { return Building; }
        return Region;
    }

    public override string ToString()
    {
        return SelectionType != SelectionTypes.None ? $"sel[{Item!}]" : string.Empty;
    }
}
