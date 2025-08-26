using Odyssey.Models.Localization;
using Odyssey.ViewModels;

namespace Odyssey.Utils
{
    /// <summary>
    /// Utilities about dates.
    /// </summary>
    public static class DateUtils
    {
        private struct Season
        {
            public int index;
            public int offset;
        }

        /// <summary>
        /// Gets translated game turn date information. 
        /// </summary>
        /// <param name="turn"></param>
        /// <returns>translated game turn date information</returns>
        public static string GameTurnToDateLabel(int turn)
        {
            Season[] seasons =
            [
                new Season { index = 1, offset = 3 },
                new Season { index = 2, offset = 0 },
                new Season { index = 2, offset = 3 },
                new Season { index = 3, offset = 0 },
                new Season { index = 3, offset = 3 },
                new Season { index = 3, offset = 6 },
                new Season { index = 0, offset = 0 },
                new Season { index = 0, offset = 3 },
                new Season { index = 1, offset = 0 }
            ];

            int since_epoch = (turn < 184) ? turn : (turn - 184);
            int year = 1 + since_epoch / 27;
            int week = since_epoch % 27;
            int month = week / 3;
            int seasonIndex = seasons[month].index;
            int week_of_month = 1 + seasons[month].offset + week % 3;
            string inSeasonLabel = Labels.Localize($"{Labels.IN_SEASON}_{seasonIndex}");
            return Labels.Localize(Labels.GAME_DATE_INFO, [$"{week_of_month}", inSeasonLabel, $"{year}"]);
        }
    }
}
