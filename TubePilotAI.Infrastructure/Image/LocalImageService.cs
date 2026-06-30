using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TubePilotAI.Application.Interfaces;
using TubePilotAI.Domain.Models;

namespace TubePilotAI.Infrastructure.Image
{
    /// <summary>
    /// Simple image service that downloads images to disk and performs basic duplicate checks by hash.
    /// This implementation is intentionally minimal for Level 0 and can be extended later.
    /// </summary>
    public class LocalImageService : IImageService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<Asset?> DownloadImageAsync(string url, string targetFolder)
        {
            try
            {
                var uri = new Uri(url);
                Directory.CreateDirectory(targetFolder);
                var fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString();
                var safe = MakeSafeFileName(fileName);
                var path = Path.Combine(targetFolder, safe);

                using (var resp = await _httpClient.GetAsync(uri))
                {
                    if (!resp.IsSuccessStatusCode) return null;
                    await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                    await resp.Content.CopyToAsync(fs);
                }

                var info = new FileInfo(path);
                var asset = new Asset
                {
                    FileName = Path.GetFileName(path),
                    RelativePath = path,
                    SizeBytes = info.Length,
                    ContentType = GetContentTypeFromFileName(path),
                    SourceUrl = url,
                    AssetType = "Downloaded"
                };

                return asset;
            }
            catch
            {
                return null;
            }
        }

        public Task<Asset?> ResizeAndCompressAsync(Asset asset, int maxWidth, int maxHeight, int maxBytes)
        {
            // Placeholder: resizing and compression not implemented at Level 0.
            return Task.FromResult<Asset?>(asset);
        }

        public Task<bool> IsDuplicateAsync(string filePath)
        {
            if (!File.Exists(filePath)) return Task.FromResult(false);
            var hash = ComputeHash(filePath);
            // In Level 0 we don't maintain a hash index; simply return false.
            return Task.FromResult(false);
        }

        private static string ComputeHash(string filePath)
        {
            using var sha = SHA256.Create();
            using var fs = File.OpenRead(filePath);
            var bytes = sha.ComputeHash(fs);
            return Convert.ToBase64String(bytes);
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        private static string GetContentTypeFromFileName(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };
        }
    }
}
