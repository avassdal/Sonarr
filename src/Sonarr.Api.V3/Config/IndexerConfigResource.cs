using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Config
{
    public class IndexerConfigResource : RestResource
    {
        public int MinimumAge { get; set; }
        public int Retention { get; set; }
        public int MaximumSize { get; set; }
        public int RssSyncInterval { get; set; }

        // AI Episode Matching
        public bool AiEpisodeMatchingEnabled { get; set; }
        public string AiEpisodeMatchingProvider { get; set; }
        public string AiEpisodeMatchingApiKey { get; set; }
        public string AiEpisodeMatchingModel { get; set; }
    }

    public static class IndexerConfigResourceMapper
    {
        public static IndexerConfigResource ToResource(IConfigService model)
        {
            return new IndexerConfigResource
            {
                MinimumAge = model.MinimumAge,
                Retention = model.Retention,
                MaximumSize = model.MaximumSize,
                RssSyncInterval = model.RssSyncInterval,
                AiEpisodeMatchingEnabled = model.AiEpisodeMatchingEnabled,
                AiEpisodeMatchingProvider = model.AiEpisodeMatchingProvider,
                AiEpisodeMatchingApiKey = model.AiEpisodeMatchingApiKey,
                AiEpisodeMatchingModel = model.AiEpisodeMatchingModel
            };
        }
    }
}
