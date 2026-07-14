using Odyssey.Models.Localization;
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

    public class WorldPlane
    {
        public string Name { get; }
        public PlaneType Type { get; }

        public WorldPlane(PlaneType planeType)
        {
            Type = planeType;
            Name = GetName(planeType);
        }

        public static string GetName(PlaneType planeType)
        {
            switch (planeType)
            {
                case PlaneType.WORLD:
                    return Labels.Localize(Labels.PLANE_WORLD);
                case PlaneType.ASTRAL:
                    return Labels.Localize(Labels.PLANE_ASTRAL);
                case PlaneType.ARENA:
                    return Labels.Localize(Labels.PLANE_ARENA);
                case PlaneType.ETERNATH:
                    return Labels.Localize(Labels.PLANE_ETERNATH);
                case PlaneType.CHRISTMAS_ISLAND:
                    return Labels.Localize(Labels.PLANE_CHRISTMAS_ISLAND);
                default:
                    return Labels.Localize(Labels.PLANE_UNKNOWN, [$"{planeType}"]);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
