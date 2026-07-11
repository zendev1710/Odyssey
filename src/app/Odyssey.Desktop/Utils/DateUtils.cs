using Microsoft.VisualBasic;
using Odyssey.Models.Localization;

namespace Odyssey.Utils;

/// <summary>
/// Utilities about dates.
/// </summary>
public static class DateUtils
{
    private struct SeasonData
    {
        public int index;
        public int offset;
    }

    private static SeasonData[] _seasonData =
    [
        new SeasonData { index = 1, offset = 3 },
        new SeasonData { index = 2, offset = 0 },
        new SeasonData { index = 2, offset = 3 },
        new SeasonData { index = 3, offset = 0 },
        new SeasonData { index = 3, offset = 3 },
        new SeasonData { index = 3, offset = 6 },
        new SeasonData { index = 0, offset = 0 },
        new SeasonData { index = 0, offset = 3 },
        new SeasonData { index = 1, offset = 0 }
    ];

    /// <summary>
    /// Gets season from the specified game turn. 
    /// </summary>
    /// <param name="gameTurn">Game turn</param>
    /// <returns>the season according to the game turn</returns>
    public static Seasons GetGameSeason(int gameTurn)
    {
        RetrieveGameDateInfo(gameTurn, out _, out int season, out _, out _);
        return (Seasons)season;
    }

    public static void RetrieveGameDateInfo(int gameTurn, out int year, out int season, out int month, out int weekOfMonth)
    {
        int sinceEpoch = (gameTurn < 184) ? gameTurn : (gameTurn - 184);
        year = 1 + sinceEpoch / 27;
        int week = sinceEpoch % 27;
        month = week / 3;
        season = _seasonData[month].index + 1;
        weekOfMonth = 1 + _seasonData[month].offset + week % 3;
    }

    /// <summary>
    /// Gets translated game turn date full information. 
    /// </summary>
    /// <param name="gameTurn">Game turn</param>
    /// <returns>translated game turn date full information</returns>
    public static string GetGameDateLabel(int gameTurn)
    {
        RetrieveGameDateInfo(gameTurn, out int year, out int season, out _, out int weekOfMonth);
        string inSeasonLabel = Labels.Localize($"{Labels.IN_SEASON}_{season}");
        return Labels.Localize(Labels.GAME_DATE_INFO, [$"{weekOfMonth}", inSeasonLabel, $"{year}"]);
    }
}
