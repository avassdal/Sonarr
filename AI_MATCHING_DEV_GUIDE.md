# AI Episode Matching - Development Guide

## Prerequisites

Before working on this feature, ensure you have the development environment set up according to Sonarr's requirements:

### Required Software

- **Node.js 20.x** (required for frontend)
  - Versions 18.x, 16.x, and 21.x are NOT supported
  - Download from: <https://nodejs.org/>
  
- **Yarn** (included with Node 20+)
  - Enable with: `corepack enable`
  - For other Node versions: `npm i -g corepack`

- **.NET SDK** (for backend)
  - .NET 6.0 or later
  - Download from: <https://dotnet.microsoft.com/download>

- **IDE** (recommended)
  - Visual Studio 2022 (Windows)
  - JetBrains Rider (Cross-platform)
  - VS Code with C# extensions

## Setting Up the Development Environment

### 1. Clone and Setup

```bash
# Fork Sonarr on GitHub first, then clone your fork
git clone https://github.com/YOUR_USERNAME/Sonarr.git
cd Sonarr

# Add upstream remote
git remote add upstream https://github.com/Sonarr/Sonarr.git

# Checkout the AI-match branch
git checkout AI-match

# Install frontend dependencies
yarn install
```

### 2. Building the Frontend

```bash
# Start webpack in watch mode (monitors for changes)
yarn start
```

This will automatically rebuild the frontend when you make changes.

### 3. Building the Backend

#### Option A: Visual Studio / Rider

1. Open `src/Sonarr.sln`
2. Set startup project to `Sonarr.Console`
3. Set framework to `net6.0`
4. Build the solution (Ctrl+Shift+B)
5. Run/Debug (F5)
6. Navigate to <http://localhost:8989>

#### Option B: Command Line

```bash
# Clean the solution
dotnet clean src/Sonarr.sln -c Debug

# Restore and build (choose your platform)
# For Linux/macOS:
dotnet msbuild -restore src/Sonarr.sln -p:Configuration=Debug -p:Platform=Posix -t:PublishAllRids

# For Windows:
dotnet msbuild -restore src/Sonarr.sln -p:Configuration=Debug -p:Platform=Windows -t:PublishAllRids

# Run the executable from _output directory
./_output/Sonarr.Console.exe  # Windows
./_output/Sonarr  # Linux/macOS
```

## Testing the AI Episode Matching Feature

### Running Unit Tests

#### From Visual Studio / Rider

1. Open Test Explorer (Test > Test Explorer)
2. Navigate to `NzbDrone.Core.Test.ParserTests`
3. Run `AiEpisodeMatchingServiceFixture` tests
4. Run all parser tests to ensure no regressions

#### From Command Line

```bash
# Run all tests
./test.sh <PLATFORM> <TYPE> <COVERAGE>

# Examples:
./test.sh windows unit false
./test.sh posix unit true

# Run specific test fixture
dotnet test src/NzbDrone.Core.Test/NzbDrone.Core.Test.csproj --filter "FullyQualifiedName~AiEpisodeMatchingServiceFixture"
```

### Manual Testing

1. **Enable AI Matching**
   - Navigate to Settings > Indexers
   - Enable "AI Episode Matching"
   - Configure provider (openai, anthropic, or gemini)
   - Add your API key
   - Select model

2. **Test with Non-Standard Release**
   - Find a release with unusual naming
   - Attempt to import or search
   - Check logs for AI matching attempts
   - Verify correct episode identification

3. **Check Logs**
   ```bash
   # Logs are typically in:
   # Windows: %APPDATA%\Sonarr\logs\
   # Linux: ~/.config/Sonarr/logs/
   # macOS: ~/Library/Application Support/Sonarr/logs/
   ```

## Code Style Guidelines

### Backend (C#)

- **Indentation**: 4 spaces (default in VS 2022)
- **Line Endings**: Unix (LF), not Windows (CRLF)
- **Naming Conventions**:
  - PascalCase for public members
  - _camelCase for private fields
  - camelCase for local variables

### Frontend (TypeScript/JavaScript)

- Run linter before committing:
  ```bash
  yarn lint --fix
  ```

- For CSS changes:
  ```bash
  yarn stylelint-windows --fix
  ```

## Writing Tests for AI Matching

### Test Coverage Requirements

- **Minimum 80% coverage** required for new code
- All public methods should have tests
- Test both success and failure scenarios

### Example Test Structure

```csharp
[TestFixture]
public class AiEpisodeMatchingServiceFixture : TestBase<AiEpisodeMatchingService>
{
    [SetUp]
    public void Setup()
    {
        // Setup mocks and test data
    }

    [Test]
    public void should_return_empty_list_when_disabled()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### Current Test Coverage

The following tests are included in `AiEpisodeMatchingServiceFixture.cs`:

- ✅ `should_return_empty_list_when_disabled`
- ✅ `should_return_empty_list_when_no_api_key`
- ✅ `should_return_empty_list_when_release_title_is_empty`
- ✅ `should_return_empty_list_when_series_is_null`
- ✅ `should_check_is_enabled_property`

### Additional Tests Needed

To reach 80% coverage, consider adding:

- Tests for successful AI matching with mocked API responses
- Tests for different providers (OpenAI, Anthropic, Gemini)
- Tests for JSON parsing with various response formats
- Tests for error handling (network failures, invalid responses)
- Integration tests with ParsingService

## Contributing Your Changes

### Before Submitting a Pull Request

1. **Ensure all tests pass**
   ```bash
   dotnet test src/Sonarr.sln
   ```

2. **Run linters**
   ```bash
   yarn lint --fix
   ```

3. **Check code coverage**
   ```bash
   ./test.sh <PLATFORM> unit true
   ```

4. **Rebase on latest develop**
   ```bash
   git fetch upstream
   git rebase upstream/develop
   ```

5. **Use meaningful commits**
   - Prefix with `New:` for new features
   - Prefix with `Fixed:` for bug fixes
   - Example: `New: Add Google Gemini AI provider support`

### Pull Request Guidelines

1. **Branch Naming**
   - ✅ Good: `ai-episode-matching`, `add-gemini-support`
   - ❌ Bad: `patch`, `develop`, `feature`

2. **Target Branch**
   - Always PR to `develop`, never `main`

3. **PR Description**
   - Explain what the feature does
   - Reference any related GitHub issues
   - Include testing steps
   - Note any breaking changes

4. **One Feature Per PR**
   - Keep PRs focused on a single feature or bug fix
   - Makes review easier and faster

### Example PR Description

```markdown
## Description
Adds AI-powered episode matching as a fallback when traditional regex matching fails.

## Related Issues
Closes #XXXX

## Changes
- New IAiEpisodeMatchingService interface and implementation
- Support for OpenAI, Anthropic, and Google Gemini providers
- Configuration options in Settings
- Integration with ParsingService
- Unit tests with 85% coverage

## Testing Steps
1. Enable AI Episode Matching in Settings
2. Configure API key for chosen provider
3. Import a release with non-standard naming
4. Verify episode is correctly matched in logs

## Breaking Changes
None - feature is disabled by default
```

## Debugging Tips

### Backend Debugging

1. **Set Breakpoints**
   - In `AiEpisodeMatchingService.MatchEpisodes()`
   - In `ParsingService.GetStandardEpisodes()` where AI fallback occurs

2. **Watch Variables**
   - `releaseTitle` - The title being parsed
   - `aiMatchedEpisodes` - Results from AI
   - `result` - Final episode list

3. **Check Logs**
   - AI matching attempts are logged at Info level
   - Errors are logged at Error level with stack traces

### Common Issues

1. **API Key Not Working**
   - Verify key is correctly saved in config
   - Check network connectivity
   - Verify provider is correctly selected

2. **No Episodes Returned**
   - Check if traditional matching is succeeding (AI won't run)
   - Verify AI matching is enabled
   - Check logs for API errors

3. **Build Errors**
   - Clean and rebuild: `dotnet clean && dotnet build`
   - Delete `bin` and `obj` folders
   - Restore packages: `dotnet restore`

## Performance Considerations

### During Development

- AI API calls add latency (1-5 seconds)
- Use mock responses for rapid testing
- Consider implementing a test mode that skips actual API calls

### For Production

- Monitor API usage and costs
- Implement caching (future enhancement)
- Add rate limiting if needed

## Next Steps for This Feature

### Immediate (Required for PR)

1. ✅ Core implementation complete
2. ✅ Basic unit tests added
3. ⚠️ Need to reach 80% test coverage
4. ⚠️ Need integration tests
5. ⚠️ Need UI implementation for settings

### Future Enhancements

1. Response caching
2. Confidence scoring
3. Batch processing
4. Additional providers (Azure OpenAI)
5. Local LLM support

## Getting Help

If you have questions about the AI Episode Matching feature:

1. Check the documentation:
   - `AI_EPISODE_MATCHING.md` - Feature overview
   - `AI_MATCHING_SUMMARY.md` - Implementation details
   - This file - Development guide

2. Review the code:
   - `src/NzbDrone.Core/Parser/AiEpisodeMatchingService.cs`
   - `src/NzbDrone.Core/Parser/ParsingService.cs`

3. Ask on Discord:
   - Sonarr Discord server
   - #development channel

4. Check existing issues:
   - GitHub Issues for Sonarr
   - Search for "AI matching" or "episode parsing"

## Resources

- [Sonarr GitHub Repository](https://github.com/Sonarr/Sonarr)
- [Sonarr Wiki](https://wiki.servarr.com/sonarr)
- [Sonarr Discord](https://discord.gg/sonarr)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [NUnit Documentation](https://docs.nunit.org/)

---

**Last Updated:** 2025-10-06  
**Branch:** AI-match  
**Status:** In Development
