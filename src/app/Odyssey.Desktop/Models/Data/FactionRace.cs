using System;

namespace Odyssey.Models.Data;

public enum FactionRace
{
    Unknown,
    Humans,
    Dwarves,
    Elves,
    Orcs,
    Goblins,
    Trolls,
    Demons,
    Insects,
    Cats,
    Aquarians,
    Halflings,
    Ghouls,
    Ghosts,
    Zombies,
    Skeletons,
    Vampires,
    Wolves,
    Rats,
    Ents,
    Centaurs,
    Dragons,
}

public static class FactionRaceParser
{
    public static FactionRace Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FactionRace.Unknown;
        }

        string normalized = value.Trim();

        if (string.Equals(normalized, Strings.DE_HUMANS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Humans;
        if (string.Equals(normalized, Strings.DE_DWARVES, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Dwarves;
        if (string.Equals(normalized, Strings.DE_ELVES, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Elves;
        if (string.Equals(normalized, Strings.DE_ORCS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Orcs;
        if (string.Equals(normalized, Strings.DE_GOBLINS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Goblins;
        if (string.Equals(normalized, Strings.DE_TROLLS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Trolls;
        if (string.Equals(normalized, Strings.DE_DEMONS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Demons;
        if (string.Equals(normalized, Strings.DE_INSECTS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Insects;
        if (string.Equals(normalized, Strings.DE_CATS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Cats;
        if (string.Equals(normalized, Strings.DE_AQUARIANS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Aquarians;
        if (string.Equals(normalized, Strings.DE_HALFLINGS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Halflings;
        if (string.Equals(normalized, Strings.DE_GHOULS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Ghouls;
        if (string.Equals(normalized, Strings.DE_GHOSTS, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Ghost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Ghast", StringComparison.OrdinalIgnoreCase))
            return FactionRace.Ghosts;
        if (string.Equals(normalized, Strings.DE_ZOMBIES, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Zombies;
        if (string.Equals(normalized, Strings.DE_SKELETONS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Skeletons;
        if (string.Equals(normalized, Strings.DE_VAMPIRES, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Vampires;
        if (string.Equals(normalized, Strings.DE_WOLVES, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Wolves;
        if (string.Equals(normalized, Strings.DE_RATS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Rats;
        if (string.Equals(normalized, Strings.DE_ENTS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Ents;
        if (string.Equals(normalized, Strings.DE_CENTAURS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Centaurs;
        if (string.Equals(normalized, Strings.DE_DRAGONS, StringComparison.OrdinalIgnoreCase))
            return FactionRace.Dragons;

        return FactionRace.Unknown;
    }
}
