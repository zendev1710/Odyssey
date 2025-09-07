using System;

namespace Odyssey.Models.Data
{
    public enum PlaneType
    {
        WORLD = 0,
        ASTRAL = 1,
        ARENA = 1137,
        ETERNATH = 59034966,
        CHRISTMAS_ISLAND = 2000,
        OTHER = 5
    }

    public class WorldPlane(string name, PlaneType planeType)
    {
        public String Name { get; set; } = name;
        public PlaneType Type { get; set; } = planeType;
    }
}
