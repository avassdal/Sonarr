export default interface IndexerOptions {
  minimumAge: number;
  retention: number;
  maximumSize: number;
  rssSyncInterval: number;
  aiEpisodeMatchingEnabled: boolean;
  aiEpisodeMatchingProvider: string;
  aiEpisodeMatchingApiKey: string;
  aiEpisodeMatchingModel: string;
}
