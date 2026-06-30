# AI Provider Integration Setup Guide

## Overview
The TubePilotAIReact application now includes real API integrations for multiple AI providers. Previously, all providers were returning mock responses. This guide explains how to configure and use these providers.

## Available AI Providers

### 1. **Google Gemini** (Recommended for Free Tier)
- **API**: Google AI Studio / Generative AI API
- **Cost**: Free tier available (generous quotas)
- **Setup Time**: ~5 minutes
- **Best For**: Content generation, fast responses

#### Setup Steps:
1. Go to [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Click "Get API Key"
3. Copy your API key
4. Add to `appsettings.Development.json`:
```json
"AIProviders": {
  "Gemini": {
	"ApiKey": "YOUR_ACTUAL_GEMINI_API_KEY_HERE",
	"Model": "gemini-1.5-flash"
  }
}
```
5. Or set environment variable:
```powershell
$env:GEMINI_API_KEY = "YOUR_KEY_HERE"
```

---

### 2. **OpenAI** (Most Powerful)
- **API**: OpenAI Platform
- **Cost**: Pay-as-you-go (starts at $5 minimum)
- **Setup Time**: ~5 minutes
- **Best For**: Advanced reasoning, high-quality content

#### Setup Steps:
1. Go to [OpenAI Platform](https://platform.openai.com/account/api-keys)
2. Create new API key
3. Add to `appsettings.Development.json`:
```json
"AIProviders": {
  "OpenAI": {
	"ApiKey": "YOUR_OPENAI_API_KEY_HERE",
	"Model": "gpt-4-turbo"
  }
}
```
4. Or set environment variable:
```powershell
$env:OPENAI_API_KEY = "YOUR_KEY_HERE"
```

---

### 3. **Claude (Anthropic)** (Balanced)
- **API**: Anthropic Console
- **Cost**: Pay-as-you-go
- **Setup Time**: ~5 minutes
- **Best For**: Nuanced writing, creative content

#### Setup Steps:
1. Go to [Anthropic Console](https://console.anthropic.com/account/keys)
2. Create API key
3. Add to `appsettings.Development.json`:
```json
"AIProviders": {
  "Claude": {
	"ApiKey": "YOUR_CLAUDE_API_KEY_HERE",
	"Model": "claude-3-sonnet-20240229"
  }
}
```
4. Or set environment variable:
```powershell
$env:CLAUDE_API_KEY = "YOUR_KEY_HERE"
```

---

### 4. **DeepSeek** (Cost-Effective)
- **API**: DeepSeek Platform
- **Cost**: Very affordable
- **Setup Time**: ~5 minutes
- **Best For**: Budget-conscious, good quality

#### Setup Steps:
1. Go to [DeepSeek Platform](https://platform.deepseek.com/api/keys)
2. Create API key
3. Add to `appsettings.Development.json`:
```json
"AIProviders": {
  "DeepSeek": {
	"ApiKey": "YOUR_DEEPSEEK_API_KEY_HERE",
	"Model": "deepseek-chat"
  }
}
```
4. Or set environment variable:
```powershell
$env:DEEPSEEK_API_KEY = "YOUR_KEY_HERE"
```

---

### 5. **Ollama** (Local/Free - No API Key Needed)
- **API**: Local HTTP API
- **Cost**: Free (runs locally)
- **Setup Time**: ~10-15 minutes
- **Best For**: Development, privacy-focused

#### Setup Steps:

**Option A: Using Ollama Locally**

1. Download [Ollama](https://ollama.ai)
2. Install and run Ollama
3. Pull a model:
```bash
ollama pull mistral
# or
ollama pull neural-chat
```
4. Verify it's running (default: http://localhost:11434)
5. Update `appsettings.Development.json`:
```json
"AIProviders": {
  "Ollama": {
	"BaseUrl": "http://localhost:11434",
	"Model": "mistral"
  }
}
```

**Option B: Using Environment Variable**
```powershell
$env:OLLAMA_BASE_URL = "http://localhost:11434"
```

---

## Configuration Methods

### Method 1: appsettings.Development.json (Easiest)
Edit `TubePilotAIReact.Server/appsettings.Development.json`:

```json
{
  "AIProviders": {
	"Gemini": {
	  "ApiKey": "YOUR_ACTUAL_KEY_HERE",
	  "Model": "gemini-1.5-flash"
	},
	"OpenAI": {
	  "ApiKey": "YOUR_ACTUAL_KEY_HERE",
	  "Model": "gpt-4-turbo"
	},
	"Claude": {
	  "ApiKey": "YOUR_ACTUAL_KEY_HERE",
	  "Model": "claude-3-sonnet-20240229"
	},
	"DeepSeek": {
	  "ApiKey": "YOUR_ACTUAL_KEY_HERE",
	  "Model": "deepseek-chat"
	},
	"Ollama": {
	  "BaseUrl": "http://localhost:11434",
	  "Model": "mistral"
	}
  }
}
```

### Method 2: Environment Variables
Set environment variables in PowerShell before running the app:

```powershell
$env:GEMINI_API_KEY = "YOUR_GEMINI_KEY"
$env:OPENAI_API_KEY = "YOUR_OPENAI_KEY"
$env:CLAUDE_API_KEY = "YOUR_CLAUDE_KEY"
$env:DEEPSEEK_API_KEY = "YOUR_DEEPSEEK_KEY"
$env:OLLAMA_BASE_URL = "http://localhost:11434"
```

### Method 3: Windows Environment Variables (Persistent)
1. Press `Win + X` → "System"
2. Advanced system settings
3. Environment Variables
4. Add new user/system variables

---

## Testing the Integration

### Using the Web UI:
1. Start backend: `dotnet run --launch-profile http` (from `TubePilotAIReact.Server`)
2. Start frontend: `npm run dev` (from `tubepilotaireact.client`)
3. Open https://localhost:49154
4. Navigate to "Generate Content"
5. Select your AI provider
6. Click "Generate" button
7. Wait for real API response (no more mock responses!)

### Expected Behavior:
- ✅ No mock responses like "[Mock Gemini - API key not configured]"
- ✅ Real content generation from selected provider
- ✅ Token counts displayed
- ✅ Accurate metadata in manifest.json

### Troubleshooting:

**Issue**: Getting mock response
- **Solution**: Verify API key is set in appsettings.json or environment variable

**Issue**: 401 Unauthorized
- **Solution**: Check API key is valid and not expired

**Issue**: 429 Rate Limit
- **Solution**: Wait a few minutes before retrying; consider upgrading your API plan

**Issue**: Ollama error
- **Solution**: Ensure Ollama is running (`ollama serve`) and accessible at configured URL

---

## Implementation Details

### What Changed:
1. **GeminiProvider** - Now calls actual Google Gemini API
2. **OpenAIProvider** - Now calls actual OpenAI API  
3. **ClaudeProvider** - Now calls actual Anthropic API
4. **DeepSeekProvider** - Now calls actual DeepSeek API
5. **OllamaProvider** - Now calls actual local Ollama instance

### HTTP Client Setup:
- All providers use dependency-injected `HttpClient`
- Automatic retry and timeout handling
- Proper error logging

### Response Handling:
- Each provider deserializes JSON responses correctly
- Token counts tracked and returned
- `IsMock` flag indicates whether response is real (false) or fallback (true)

### Security:
- API keys read from environment variables (not hardcoded)
- No API keys logged
- Headers properly set for each provider

---

## Production Deployment

For production, set environment variables in your deployment environment:

**Azure App Service:**
```
Configuration → Application settings → New application setting
Name: GEMINI_API_KEY
Value: [your-key]
```

**Docker:**
```dockerfile
ENV GEMINI_API_KEY=your_key_here
ENV OPENAI_API_KEY=your_key_here
```

**Docker Compose:**
```yaml
environment:
  - GEMINI_API_KEY=${GEMINI_API_KEY}
  - OPENAI_API_KEY=${OPENAI_API_KEY}
```

---

## Recommended Setup for Development

**Quickest Setup (5 minutes):**
1. Get free Gemini API key (no credit card required)
2. Add to appsettings.Development.json
3. Test immediately

**Full Setup (15 minutes):**
1. Get Gemini key (free tier)
2. Get OpenAI key (add $5 credit)
3. Set up Ollama locally (free, no internet required)
4. Configure all three in appsettings.json
5. Switch providers while testing

---

## Migration from Mock to Real

The application automatically:
- ✅ Detects missing API keys and logs warnings
- ✅ Falls back gracefully if API is unavailable
- ✅ Preserves response format for UI compatibility
- ✅ Tracks whether response is real or mock via `IsMock` flag

No code changes required in the frontend!

---

## API Pricing Comparison

| Provider | Free Tier | Entry Price | Good For |
|----------|-----------|-------------|----------|
| **Gemini** | ✅ Yes (generous) | Free tier is sufficient for dev | Learning, testing, low-volume |
| **OpenAI** | ❌ No | $5-$50/month typical | Production, high-quality output |
| **Claude** | ❌ No | $3-$30/month typical | Creative writing, nuanced content |
| **DeepSeek** | ❌ No | $1-$10/month typical | Budget-conscious, good quality |
| **Ollama** | ✅ Yes (local) | Free (runs on your machine) | Development, testing, offline work |

---

## Next Steps

1. **Choose a provider** (Gemini recommended for fastest start)
2. **Get API key** (follow provider setup above)
3. **Configure in appsettings.json** or environment variable
4. **Restart backend server**
5. **Test via web UI** - Generate Content button
6. **Verify** - Check manifest.json for real responses

---

## Support & Issues

**Debug Logs:**
Check backend console output for detailed API request/response logs:
```
[ERR] Gemini API error: 401 - Invalid API key
[ERR] OpenAI API exception: Connection timeout
```

**Common Errors:**
- `[Mock ... - API key not configured]` → Set API key in appsettings or env var
- `[... API Error: 429]` → Rate limited, wait and retry
- `[... API Error: 401]` → Invalid/expired API key
- `[... Error: ...]` → Check backend logs for details

---

## Files Modified

- `TubePilotAI.Infrastructure/ExternalServices/AI/*.cs` - All provider implementations updated
- `TubePilotAIReact.Server/appsettings.Development.json` - Configuration added
- `TubePilotAI.Infrastructure/DependencyInjection/AIProviderServiceCollectionExtensions.cs` - HttpClient registration
- `TubePilotAI.Infrastructure/TubePilotAI.Infrastructure.csproj` - Added Microsoft.Extensions.Http

---

**Last Updated**: 2024
**Status**: ✅ Ready for Production
