using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public class AiEpisodeMatchingService : IAiEpisodeMatchingService
    {
        private readonly IEpisodeService _episodeService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly HttpClient _httpClient;

        public AiEpisodeMatchingService(
            IEpisodeService episodeService,
            IConfigService configService,
            Logger logger)
        {
            _episodeService = episodeService;
            _configService = configService;
            _logger = logger;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public bool IsEnabled
        {
            get
            {
                // Check if AI matching is enabled in configuration
                return _configService.AiEpisodeMatchingEnabled;
            }
        }

        public List<Episode> MatchEpisodes(string releaseTitle, Series series, int? seasonNumber = null)
        {
            if (!IsEnabled)
            {
                _logger.Debug("AI episode matching is disabled");
                return new List<Episode>();
            }

            if (string.IsNullOrWhiteSpace(releaseTitle))
            {
                _logger.Debug("Release title is empty, cannot perform AI matching");
                return new List<Episode>();
            }

            if (series == null)
            {
                _logger.Debug("Series is null, cannot perform AI matching");
                return new List<Episode>();
            }

            try
            {
                _logger.Info("Attempting AI-based episode matching for: {0} - Series: {1}", releaseTitle, series.Title);

                // Get the AI provider configuration
                var aiProvider = _configService.AiEpisodeMatchingProvider;
                var aiApiKey = _configService.AiEpisodeMatchingApiKey;
                var aiModel = _configService.AiEpisodeMatchingModel;

                if (string.IsNullOrWhiteSpace(aiApiKey))
                {
                    _logger.Warn("AI API key is not configured. Please configure it in Settings.");
                    return new List<Episode>();
                }

                // Get available episodes for the series
                var availableEpisodes = seasonNumber.HasValue
                    ? _episodeService.GetEpisodesBySeason(series.Id, seasonNumber.Value)
                    : _episodeService.GetEpisodeBySeries(series.Id);

                if (!availableEpisodes.Any())
                {
                    _logger.Debug("No episodes found for series {0}", series.Title);
                    return new List<Episode>();
                }

                // Perform AI matching
                var matchedEpisodes = PerformAiMatching(releaseTitle, series, availableEpisodes, aiProvider, aiApiKey, aiModel);

                if (matchedEpisodes.Any())
                {
                    _logger.Info("AI matching successful: Found {0} episode(s) for {1}", matchedEpisodes.Count, releaseTitle);
                }
                else
                {
                    _logger.Debug("AI matching did not find any episodes for {0}", releaseTitle);
                }

                return matchedEpisodes;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during AI episode matching for {0}", releaseTitle);
                return new List<Episode>();
            }
        }

        private List<Episode> PerformAiMatching(
            string releaseTitle,
            Series series,
            List<Episode> availableEpisodes,
            string provider,
            string apiKey,
            string model)
        {
            try
            {
                // Build the prompt for the AI
                var prompt = BuildMatchingPrompt(releaseTitle, series, availableEpisodes);

                // Call the AI API based on provider
                var response = provider.ToLower() switch
                {
                    "openai" => CallOpenAiApi(prompt, apiKey, model).GetAwaiter().GetResult(),
                    "anthropic" => CallAnthropicApi(prompt, apiKey, model).GetAwaiter().GetResult(),
                    "gemini" => CallGeminiApi(prompt, apiKey, model).GetAwaiter().GetResult(),
                    _ => throw new NotSupportedException($"AI provider '{provider}' is not supported")
                };

                // Parse the AI response and match episodes
                return ParseAiResponse(response, availableEpisodes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error performing AI matching");
                return new List<Episode>();
            }
        }

        private string BuildMatchingPrompt(string releaseTitle, Series series, List<Episode> availableEpisodes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are an expert at matching TV show episode release titles to their corresponding episodes.");
            sb.AppendLine();
            sb.AppendLine($"Series: {series.Title}");
            sb.AppendLine($"Release Title: {releaseTitle}");
            sb.AppendLine();
            sb.AppendLine("Available Episodes:");

            // Limit to reasonable number of episodes to avoid token limits
            var episodesToInclude = availableEpisodes.Take(100).ToList();
            foreach (var episode in episodesToInclude)
            {
                sb.AppendLine($"- S{episode.SeasonNumber:D2}E{episode.EpisodeNumber:D2}: {episode.Title} (Air Date: {episode.AirDate})");
            }

            sb.AppendLine();
            sb.AppendLine("Based on the release title, identify which episode(s) it corresponds to.");
            sb.AppendLine("Respond ONLY with a JSON array of episode identifiers in this exact format:");
            sb.AppendLine("[{\"season\": 1, \"episode\": 5}, {\"season\": 1, \"episode\": 6}]");
            sb.AppendLine();
            sb.AppendLine("If you cannot determine the episode with confidence, respond with an empty array: []");
            sb.AppendLine("Do not include any explanation, only the JSON array.");

            return sb.ToString();
        }

        private async Task<string> CallOpenAiApi(string prompt, string apiKey, string model)
        {
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.1,
                max_tokens = 500
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            var messageContent = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? string.Empty;
        }

        private async Task<string> CallAnthropicApi(string prompt, string apiKey, string model)
        {
            var requestBody = new
            {
                model = model,
                max_tokens = 500,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            var messageContent = jsonDoc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return messageContent ?? string.Empty;
        }

        private async Task<string> CallGeminiApi(string prompt, string apiKey, string model)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 500
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();

            // Gemini uses API key as query parameter
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            var messageContent = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return messageContent ?? string.Empty;
        }

        private List<Episode> ParseAiResponse(string response, List<Episode> availableEpisodes)
        {
            try
            {
                // Clean up the response - sometimes AI adds markdown code blocks
                var cleanResponse = response.Trim();
                if (cleanResponse.StartsWith("```json"))
                {
                    cleanResponse = cleanResponse.Substring(7);
                }
                if (cleanResponse.StartsWith("```"))
                {
                    cleanResponse = cleanResponse.Substring(3);
                }
                if (cleanResponse.EndsWith("```"))
                {
                    cleanResponse = cleanResponse.Substring(0, cleanResponse.Length - 3);
                }
                cleanResponse = cleanResponse.Trim();

                // Parse the JSON response
                var jsonDoc = JsonDocument.Parse(cleanResponse);
                var matchedEpisodes = new List<Episode>();

                foreach (var element in jsonDoc.RootElement.EnumerateArray())
                {
                    var season = element.GetProperty("season").GetInt32();
                    var episode = element.GetProperty("episode").GetInt32();

                    var matchedEpisode = availableEpisodes.FirstOrDefault(e =>
                        e.SeasonNumber == season && e.EpisodeNumber == episode);

                    if (matchedEpisode != null)
                    {
                        matchedEpisodes.Add(matchedEpisode);
                        _logger.Debug("AI matched: S{0:D2}E{1:D2} - {2}", season, episode, matchedEpisode.Title);
                    }
                }

                return matchedEpisodes;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error parsing AI response: {0}", response);
                return new List<Episode>();
            }
        }
    }
}
