using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Odyssey.Models.Documents;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Odyssey.Models.Tools
{
    [Flags]
    public enum BelongsToStatus
    {
        None = 0,
        UnitInShip = 1,
        UnitInBuilding = 2,
        ShipInNotActiveFaction = 4,
        BuildingInNotActiveFaction = 8,
    }

}
