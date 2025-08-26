using System;

namespace Odyssey.Models.Data
{
    public enum PlaneType
    {
        WORLD,
        ASTRAL,
    }
    public class WorldPlane(string name, PlaneType planeType)
    {
        public String Name { get; set; } = name;
        public PlaneType Type { get; set; } = planeType;
    }
}
