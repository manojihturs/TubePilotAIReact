# Copilot Instructions

## Project Guidelines
- tubepilotaireact.client/vite.config.ts saved: proxy config uses target from ASPNETCORE_HTTPS_PORT or ASPNETCORE_URLS (fallback http://localhost:5285); changeOrigin=false; DEV_SERVER_PORT default 49153; reads dev HTTPS cert from ASP.NET dev-certs path. Use for future dev-server/proxy troubleshooting.