using Odyssey.Models.Data;
using System;

namespace Odyssey.Extensions;

public static class SpecialPlaneExtensions
{
    public static bool IsSpecialPlane(this int p)
    {
        return !p.IsStandardPlane();
    }

    public static bool IsStandardPlane(this int p)
    {
        return p == (int)PlaneType.WORLD;
    }
}
