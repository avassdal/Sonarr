using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IAiEpisodeMatchingService
    {
        /// <summary>
        /// Attempts to match episodes using AI when traditional pattern-based matching fails
        /// </summary>
        /// <param name="releaseTitle">The release title to parse</param>
        /// <param name="series">The series to match against</param>
        /// <param name="seasonNumber">The season number if known</param>
        /// <returns>List of matched episodes, or empty list if no match found</returns>
        List<Episode> MatchEpisodes(string releaseTitle, Series series, int? seasonNumber = null);

        /// <summary>
        /// Checks if AI matching is enabled in configuration
        /// </summary>
        bool IsEnabled { get; }
    }
}
