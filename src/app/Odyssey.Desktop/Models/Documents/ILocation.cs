using Odyssey.Extensions;
using Odyssey.Models.Data;

namespace Odyssey.Models.Documents;

public interface ILocation
{
    int X { get; set; }
    int Y { get; set; }

    PlaneType Plane { get; }

    string Label
    {
        get
        {
            if (Plane != PlaneType.WORLD)
            {
                // Plane name is part of name for regions and battles
                return $"({X}, {Y}, {WorldPlane.GetName(Plane)})";
            }
            return $"({X}, {Y})";
        }
    }

}