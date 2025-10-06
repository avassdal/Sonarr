# AI Episode Matching

## Overview

This feature adds AI-powered episode matching as a fallback mechanism when traditional pattern-based parsing fails to identify episodes. When Sonarr cannot match a release title using its extensive regex patterns, it can optionally use an AI service (OpenAI or Anthropic) to intelligently match the release to the correct episode(s).

## How It Works

1. **Traditional Matching First**: Sonarr always attempts to match episodes using its existing regex-based parser
2. **AI Fallback**: If traditional matching fails and AI matching is enabled, Sonarr will:
   - Build a context-aware prompt with the series information and available episodes
   - Send the release title to the configured AI provider
   - Parse the AI response to identify the matching episode(s)
   - Return the matched episodes for further processing

## Configuration

AI Episode Matching can be configured in Settings with the following options:

### Settings

- **Enable AI Episode Matching** (`AiEpisodeMatchingEnabled`): Toggle to enable/disable the feature (default: `false`)
- **AI Provider** (`AiEpisodeMatchingProvider`): Choose between `openai`, `anthropic`, or `gemini` (default: `openai`)
- **API Key** (`AiEpisodeMatchingApiKey`): Your API key for the selected provider (required)
- **Model** (`AiEpisodeMatchingModel`): The AI model to use (default: `gpt-4o-mini`)

### Supported Providers

#### OpenAI

- Models: `gpt-4o-mini`, `gpt-4o`, `gpt-4-turbo`, `gpt-3.5-turbo`
- API Key: Obtain from <https://platform.openai.com/api-keys>
- Cost: Varies by model (gpt-4o-mini is most cost-effective)

#### Anthropic

- Models: `claude-3-5-sonnet-20241022`, `claude-3-opus-20240229`, `claude-3-haiku-20240307`
- API Key: Obtain from <https://console.anthropic.com/>
- Cost: Varies by model

#### Google Gemini

- Models: `gemini-1.5-flash`, `gemini-1.5-pro`, `gemini-1.0-pro`
- API Key: Obtain from <https://aistudio.google.com/app/apikey>
- Cost: Varies by model (gemini-1.5-flash is most cost-effective)

## Use Cases

This feature is particularly useful for:

1. **Non-standard Release Naming**: Releases that don't follow typical naming conventions
2. **Foreign Language Releases**: Releases with titles in languages other than English
3. **Special Episodes**: Special episodes that don't fit standard patterns
4. **Anime**: Complex anime naming schemes that vary by release group
5. **Custom Naming**: Releases from sources that use unique naming conventions

## Implementation Details

### Files Added/Modified

#### New Files
- `src/NzbDrone.Core/Parser/IAiEpisodeMatchingService.cs` - Service interface
- `src/NzbDrone.Core/Parser/AiEpisodeMatchingService.cs` - Service implementation
- `src/NzbDrone.Core.Test/ParserTests/AiEpisodeMatchingServiceFixture.cs` - Unit tests
- `AI_EPISODE_MATCHING.md` - This documentation

#### Modified Files
- `src/NzbDrone.Core/Parser/ParsingService.cs` - Integrated AI matching as fallback
- `src/NzbDrone.Core/Configuration/IConfigService.cs` - Added configuration properties
- `src/NzbDrone.Core/Configuration/ConfigService.cs` - Implemented configuration properties

### Integration Points

AI matching is integrated into two key methods in `ParsingService`:

1. **GetStandardEpisodes**: For standard episode matching (S01E01 format)
2. **GetAnimeEpisodes**: For anime absolute episode number matching

Both methods now attempt AI matching when traditional matching returns no results.

## Security Considerations

1. **API Key Storage**: API keys are stored in the Sonarr configuration database
2. **HTTPS Only**: All API calls are made over HTTPS
3. **No Data Retention**: Release titles are sent to AI providers but not stored by them (per provider policies)
4. **Rate Limiting**: Consider implementing rate limiting to avoid excessive API costs

## Performance Considerations

1. **Timeout**: AI API calls have a 30-second timeout
2. **Fallback Only**: AI matching only occurs when traditional matching fails, minimizing API usage
3. **Episode Limit**: Only the first 100 episodes are sent to the AI to avoid token limits
4. **Async Operations**: API calls are made asynchronously to avoid blocking

## Cost Estimation

Typical costs per AI matching request (approximate):

- **OpenAI gpt-4o-mini**: $0.0001 - $0.0005 per request
- **OpenAI gpt-4o**: $0.001 - $0.005 per request
- **Anthropic Claude Haiku**: $0.0001 - $0.0005 per request
- **Anthropic Claude Sonnet**: $0.001 - $0.005 per request
- **Google Gemini Flash**: $0.00005 - $0.0002 per request (most cost-effective)
- **Google Gemini Pro**: $0.0005 - $0.002 per request

For most users, costs will be minimal as AI matching only triggers when traditional matching fails.

## Future Enhancements

Potential improvements for future versions:

1. **Caching**: Cache AI responses to avoid repeated API calls for the same release
2. **Confidence Scoring**: Return confidence scores from AI and only use high-confidence matches
3. **Local AI Models**: Support for local LLM models to avoid API costs
4. **Batch Processing**: Process multiple releases in a single API call
5. **Learning**: Store successful AI matches to improve traditional regex patterns
6. **Additional Providers**: Support for more AI providers (Azure OpenAI, etc.)

## Troubleshooting

### AI Matching Not Working

1. Verify AI Episode Matching is enabled in Settings
2. Check that a valid API key is configured
3. Ensure the selected provider and model are correct
4. Check Sonarr logs for AI matching attempts and errors
5. Verify network connectivity to AI provider APIs

### High API Costs

1. Review which releases are triggering AI matching in logs
2. Consider using a more cost-effective model (e.g., gemini-1.5-flash or gpt-4o-mini)
3. Improve traditional regex patterns to reduce AI fallback usage
4. Implement rate limiting or daily cost caps

### Incorrect Matches

1. Review the AI matching logs to see what was sent and received
2. Try a different AI model (some are better at specific tasks)
3. Report issues with specific release patterns for regex improvements
4. Consider disabling AI matching for specific series types

## Testing

Run the unit tests:

```bash
dotnet test src/NzbDrone.Core.Test/ParserTests/AiEpisodeMatchingServiceFixture.cs
```

## Contributing

When contributing to this feature:

1. Maintain backward compatibility
2. Add unit tests for new functionality
3. Update this documentation
4. Consider API cost implications
5. Follow existing code style and patterns
