using System.Threading.Tasks;
using TubePilotAI.Domain.Models;

namespace TubePilotAI.Application.Interfaces
{
    /// <summary>
    /// Image service responsible for downloading, resizing, compressing, and validating images.
    /// </summary>
    public interface IImageService
    {
        Task<Asset?> DownloadImageAsync(string url, string targetFolder);

        Task<Asset?> ResizeAndCompressAsync(Asset asset, int maxWidth, int maxHeight, int maxBytes);

        Task<bool> IsDuplicateAsync(string filePath);
    }
}
