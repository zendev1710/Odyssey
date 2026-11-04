using Odyssey.Models.Data;
using System;

namespace Odyssey.Extensions;

public static class SpecialFactionExtensions
{
    public static bool IsMonster(this int f)
    {
        return f == (int)SpecialFaction.MONSTER || f == (int)SpecialFaction.MONSTER_OLD;
    }

    public static bool IsKnownFaction(this int f)
    {
        return f > (int)SpecialFaction.MONSTER_OLD && f != (int)SpecialFaction.MONSTER;
    }
}
