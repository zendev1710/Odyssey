using System;

namespace Odyssey.Models.Documents;

[Flags]
public enum SelectionChangeTypes
{
    None = 0,
    Region = 1 << 0,
    //UnknownRegions = 1 << 1,
    Buildings = 1 << 1,
    //Faction = 1 << 2,
    Ships = 1 << 2,
    Building = 1 << 3,
    Ship = 1 << 4,
    Unit = 1 << 5,
    Cleared = 1 << 6
}