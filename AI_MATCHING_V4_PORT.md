# AI Episode Matching - Sonarr v4 Port

## Overview

This document describes the port of the AI Episode Matching feature from Sonarr v5 to v4.0.15.2941.

## Branch Information

- **Base Version**: v4.0.15.2941
- **Branch Name**: `ai-matching-v4`
- **Original Branch**: `AI-match-release` (v5)

## Changes Made

### 1. Core Implementation (Cherry-picked from v5)

The following commits were successfully cherry-picked from the v5 branch:

- `98057739d` - Add AI-powered episode matching as fallback
- `5e558151b` - Add implementation summary for AI episode matching  
- `b3a3442a5` - Add Google Gemini AI provider support
- `58b060540` - Add development guide for AI episode matching

These include:
- `src/NzbDrone.Core/Parser/IAiEpisodeMatchingService.cs` - Interface
- `src/NzbDrone.Core/Parser/AiEpisodeMatchingService.cs` - Implementation
- `src/NzbDrone.Core.Test/ParserTests/AiEpisodeMatchingServiceFixture.cs` - Tests
- `src/NzbDrone.Core/Configuration/IConfigService.cs` - Configuration interface
- `src/NzbDrone.Core/Configuration/ConfigService.cs` - Configuration implementation
- `src/NzbDrone.Core/Parser/ParsingService.cs` - Integration with parsing logic
- Documentation files (AI_EPISODE_MATCHING.md, AI_MATCHING_SUMMARY.md, AI_MATCHING_DEV_GUIDE.md)

### 2. API Layer Adaptation (v5 → v3)

**File**: `src/Sonarr.Api.V3/Config/IndexerConfigResource.cs`

Added AI matching properties:
```csharp
// AI Episode Matching
public bool AiEpisodeMatchingEnabled { get; set; }
public string AiEpisodeMatchingProvider { get; set; }
public string AiEpisodeMatchingApiKey { get; set; }
public string AiEpisodeMatchingModel { get; set; }
```

Updated mapper to include AI properties in `ToResource()` method.

**File**: `src/Sonarr.Api.V3/Config/IndexerConfigController.cs`

Added validation rules:
- Provider must be 'openai', 'anthropic', or 'gemini'
- API Key is required when AI matching is enabled

### 3. Localization

**File**: `src/NzbDrone.Core/Localization/Core/en.json`

Added localization strings:
- `AiApiKey` - "AI API Key"
- `AiApiKeyHelpText` - Help text for API key
- `AiEpisodeMatching` - "AI Episode Matching"
- `AiModel` - "AI Model"
- `AiModelHelpText` - Help text for model selection
- `AiProvider` - "AI Provider"
- `AiProviderHelpText` - Help text for provider selection
- `EnableAiEpisodeMatching` - "Enable AI Episode Matching"
- `EnableAiEpisodeMatchingHelpText` - Help text for enabling feature

## What's Different from v5

1. **API Namespace**: Uses `Sonarr.Api.V3` instead of `Sonarr.Api.V5`
2. **Frontend**: Frontend implementation not yet ported (pending)
3. **Build Workflow**: No build workflow changes (v4 uses different CI/CD)

## Supported AI Providers

1. **OpenAI** (GPT-4o-mini, GPT-4o, GPT-3.5-turbo)
2. **Anthropic** (Claude 3 Haiku, Opus, Sonnet)
3. **Google Gemini** (Gemini Pro, Gemini 1.5 Flash)

## Configuration

AI Episode Matching can be configured via:
- API: `PUT /api/v3/config/indexer`
- Settings: Indexers → Options (UI pending)

Required settings:
- `aiEpisodeMatchingEnabled`: true/false
- `aiEpisodeMatchingProvider`: "openai" | "anthropic" | "gemini"
- `aiEpisodeMatchingApiKey`: Your API key
- `aiEpisodeMatchingModel`: Model name (e.g., "gpt-4o-mini")

## How It Works

1. Traditional regex-based parsing attempts to match episodes
2. If parsing fails, AI matching is triggered (if enabled)
3. AI analyzes the release title and series information
4. Returns matched episode(s) or empty list if no match

## Testing

Run the test suite:
```bash
dotnet test src/NzbDrone.Core.Test/ParserTests/AiEpisodeMatchingServiceFixture.cs
```

## Next Steps

1. **Frontend Implementation**: Port the React/TypeScript UI components from v5
2. **Testing**: Comprehensive testing with real-world releases
3. **Documentation**: Update user-facing documentation

## Commits

```
1476801c9 - Add AI Episode Matching API support for v4 (Sonarr.Api.V3)
58b060540 - Add development guide for AI episode matching
b3a3442a5 - Add Google Gemini AI provider support
5e558151b - Add implementation summary for AI episode matching
98057739d - Add AI-powered episode matching as fallback
```

## Notes

- The core AI matching logic is identical to v5
- API structure adapted to v3 conventions
- Frontend UI components need to be ported separately (v4 uses different component structure)
- All backend functionality is complete and ready for testing
