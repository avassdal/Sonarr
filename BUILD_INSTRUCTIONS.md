# Build Instructions - Sonarr AI Episode Matching Edition

## Automated Build (GitHub Actions)

### Windows x64 Installer

The workflow automatically builds a Windows x64 installer when you push to the `ai-matching-v4` branch.

**Trigger the build:**
```bash
git push origin ai-matching-v4
```

**Or manually trigger:**
1. Go to GitHub Actions
2. Select "Build Windows x64 Installer"
3. Click "Run workflow"
4. Select branch: `ai-matching-v4`

**Artifacts produced:**
- `Sonarr.{version}.windows.x64-installer.exe` - Windows installer
- `Sonarr.{version}.windows.x64.zip` - Portable archive

**Download artifacts:**
1. Go to Actions → Build Windows x64 Installer
2. Click on the latest successful run
3. Download artifacts at the bottom

---

## Manual Build (Local)

### Prerequisites

**Windows:**
- .NET 6.0 SDK
- Node.js 18+ (with Volta)
- Yarn
- Inno Setup (for installer)

**Install dependencies:**
```bash
# Install .NET SDK
winget install Microsoft.DotNet.SDK.6

# Install Node.js and Yarn
winget install Volta.Volta
volta install node@18
volta install yarn

# Install Inno Setup
winget install JRSoftware.InnoSetup
```

### Build Steps

**1. Clone and checkout:**
```bash
git clone https://github.com/avassdal/Sonarr.git
cd Sonarr
git checkout ai-matching-v4
```

**2. Build backend:**
```bash
./build.sh --backend --enable-extra-platforms --packages
```

**3. Build frontend:**
```bash
yarn install
yarn build --env production
```

**4. Create Windows installer:**
```bash
cd distribution/windows/setup
SET RUNTIME=win-x64
build.bat
```

**5. Find your installer:**
```
_artifacts/Sonarr.{version}.windows.x64-installer.exe
```

---

## Version Numbering

- **Base version**: 4.0.15 (from v4.0.15.2941 tag)
- **Build number**: Auto-incremented by GitHub Actions
- **Format**: `4.0.15.{build_number}`

Example: `4.0.15.10801`

---

## Testing the Build

### Install and Configure

1. **Install Sonarr:**
   - Run the installer
   - Follow installation wizard
   - Start Sonarr service

2. **Configure AI Matching:**
   - Open Sonarr UI (http://localhost:8989)
   - Go to Settings → Indexers → Options
   - Scroll to "AI Episode Matching"
   - Enable and configure:
     - ✅ Enable AI Episode Matching
     - Provider: OpenAI (or Anthropic/Gemini)
     - API Key: Your API key
     - Model: gpt-4o-mini (recommended)

3. **Test with a release:**
   - Add a series (e.g., Formula 1)
   - Send a release with non-standard naming
   - Check logs for AI matching activity

### Verify AI Matching Works

**Check logs:**
```
Settings → General → Log Level → Debug
```

**Look for:**
```
Traditional matching failed, attempting AI-based matching for [release]
AI matching succeeded where traditional matching failed for [release]
```

**Test releases:**
- Formula 1: `Formula1.2025.R18.Singapore.Race.2160p.UHDTV.H.265`
- Non-standard anime: `[Group] Show Name - Special Episode`
- Sports events: `WWE.Raw.2025.01.15.720p.HDTV`

---

## Troubleshooting

### Build Fails

**Backend build error:**
```bash
# Clean and rebuild
./build.sh --clean
./build.sh --backend --enable-extra-platforms --packages
```

**Frontend build error:**
```bash
# Clean node modules
rm -rf node_modules
yarn install
yarn build --env production
```

**Installer creation fails:**
- Ensure Inno Setup is installed
- Check `distribution/windows/setup/build.bat`
- Verify `_output` directory exists

### AI Matching Not Working

1. **Check if enabled:**
   - Settings → Indexers → Options → AI Episode Matching

2. **Verify API key:**
   - Test API key with curl:
   ```bash
   curl https://api.openai.com/v1/models \
     -H "Authorization: Bearer YOUR_API_KEY"
   ```

3. **Check logs:**
   - Look for "AI matching" entries
   - Check for API errors

4. **Verify series exists:**
   - Series must be added to Sonarr
   - Episodes must be populated
   - Season/episode metadata must match

---

## Distribution

### GitHub Release

**Create a release:**
```bash
# Tag the commit
git tag -a v4.0.15.10801 -m "Sonarr AI Matching Edition v4.0.15.10801"
git push origin v4.0.15.10801
```

The workflow will automatically create a GitHub release with:
- Windows x64 installer
- Windows x64 portable archive
- Release notes

### Manual Distribution

**Upload to your own server:**
```bash
# Upload installer
scp _artifacts/Sonarr.*.windows.x64-installer.exe user@server:/path/

# Upload archive
scp _artifacts/Sonarr.*.windows.x64.zip user@server:/path/
```

---

## Development

### Making Changes

**Backend changes:**
```bash
# Edit C# files in src/
# Rebuild backend
./build.sh --backend
```

**Frontend changes:**
```bash
# Edit files in frontend/src/
# Rebuild frontend
yarn build
```

**Test changes:**
```bash
# Run tests
dotnet test src/NzbDrone.Core.Test/ParserTests/AiEpisodeMatchingServiceFixture.cs
```

### Commit and Push

```bash
git add .
git commit -m "Your changes"
git push origin ai-matching-v4
```

GitHub Actions will automatically build a new installer.

---

## Support

- **Documentation**: See `AI_EPISODE_MATCHING.md`
- **Development Guide**: See `AI_MATCHING_DEV_GUIDE.md`
- **Port Details**: See `AI_MATCHING_V4_PORT.md`
- **Issues**: Create an issue on GitHub

---

## License

Sonarr is licensed under GPL-3.0. This custom build includes AI Episode Matching functionality.
