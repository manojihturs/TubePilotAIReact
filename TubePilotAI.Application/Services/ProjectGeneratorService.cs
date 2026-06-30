using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Interfaces;
using TubePilotAI.Application.Services;
using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Application.Services
{
    /// <summary>
    /// Coordinates project generation. For level 0 this performs synchronous generation and records logs in the in-memory store.
    /// Uses IAIProviderResolver to pick a registered provider from existing infrastructure.
    /// </summary>
    public class ProjectGeneratorService : IProjectGeneratorService
    {
        private readonly IAIProviderResolver _providerResolver;
        private readonly IImageService _imageService;

        public ProjectGeneratorService(IAIProviderResolver providerResolver, IImageService imageService)
        {
            _providerResolver = providerResolver;
            _imageService = imageService;
        }

        public async Task<GenerateProjectResponseDto> EnqueueGenerationAsync(GenerateProjectRequestDto request)
        {
            var projectId = Guid.NewGuid().ToString();
            var state = new GenerationStateStore.State
            {
                ProjectId = projectId,
                Status = "Running",
                Progress = 0
            };
            state.Logs.Add($"Starting generation for topic '{request.Topic}'");
            GenerationStateStore.Set(state);

            try
            {
                var baseFolder = string.IsNullOrWhiteSpace(request.OutputFolder)
                    ? Path.Combine(Directory.GetCurrentDirectory(), "GeneratedProjects")
                    : request.OutputFolder;

                var projectFolder = Path.Combine(baseFolder, MakeSafeFolderName(request.Topic + "_" + projectId));
                Directory.CreateDirectory(projectFolder);

                // Create folders
                var input = Path.Combine(projectFolder, "Input");
                var research = Path.Combine(projectFolder, "Research");
                var data = Path.Combine(projectFolder, "Data");
                var script = Path.Combine(projectFolder, "Script");
                var thumbnail = Path.Combine(projectFolder, "Thumbnail");
                var metadata = Path.Combine(projectFolder, "Metadata");
                var assets = Path.Combine(projectFolder, "Assets");
                var export = Path.Combine(projectFolder, "Export");

                Directory.CreateDirectory(input);
                Directory.CreateDirectory(research);
                Directory.CreateDirectory(data);
                Directory.CreateDirectory(script);
                Directory.CreateDirectory(thumbnail);
                Directory.CreateDirectory(metadata);
                Directory.CreateDirectory(assets);
                Directory.CreateDirectory(export);

                state.Progress = 10;
                state.Logs.Add("Created project folders");

                // Generate simple input files
                await File.WriteAllTextAsync(Path.Combine(input, "prompt.txt"), request.PromptTemplate ?? string.Empty, Encoding.UTF8);
                await File.WriteAllTextAsync(Path.Combine(input, "title.txt"), request.Topic ?? string.Empty, Encoding.UTF8);
                state.Progress = 20;
                state.Logs.Add("Wrote input files");

                // Resolve provider
                var provider = ResolveProvider(request.AiProvider);
                state.Logs.Add($"Using AI provider: {provider.ProviderName}");

                // Create AI requests and invoke provider
                var researchReq = new AIProviderRequest { Prompt = request.PromptTemplate ?? ("Research: " + request.Topic), SystemMessage = "Generate research notes and references." };
                var researchResp = await provider.GenerateAsync(researchReq);
                await File.WriteAllTextAsync(Path.Combine(research, "research.md"), researchResp.Content, Encoding.UTF8);
                await File.WriteAllTextAsync(Path.Combine(research, "references.txt"), "(references not provided by mock provider)", Encoding.UTF8);

                state.Progress = 40;
                state.Logs.Add("Generated research via AI provider");

                var scriptReq = new AIProviderRequest { Prompt = request.PromptTemplate ?? ("Write a script for: " + request.Topic), SystemMessage = "Produce a narration and scene list." };
                var scriptResp = await provider.GenerateAsync(scriptReq);
                await File.WriteAllTextAsync(Path.Combine(script, "narration.txt"), scriptResp.Content, Encoding.UTF8);
                await File.WriteAllTextAsync(Path.Combine(script, "scenes.txt"), "(scene list not parsed)", Encoding.UTF8);

                state.Progress = 60;
                state.Logs.Add("Generated script via AI provider");

                // Thumbnails and metadata
                var thumbReq = new AIProviderRequest { Prompt = "Generate thumbnail prompts for: " + request.Topic, SystemMessage = "Return a short thumbnail prompt." };
                var thumbResp = await provider.GenerateAsync(thumbReq);
                await File.WriteAllTextAsync(Path.Combine(thumbnail, "thumbnail_prompt.txt"), thumbResp.Content, Encoding.UTF8);
                await File.WriteAllTextAsync(Path.Combine(metadata, "title.txt"), request.Topic, Encoding.UTF8);
                await File.WriteAllTextAsync(Path.Combine(metadata, "description.txt"), researchResp.Content, Encoding.UTF8);

                state.Progress = 75;
                state.Logs.Add("Generated thumbnail prompts and metadata");

                // Create data.csv header
                var csvPath = Path.Combine(data, "data.csv");
                var csvHeader = "ID,Title,Year,Category,Ranking,Interesting Fact,Description,Image File,Thumbnail File,Poster File,Source URL";
                await File.WriteAllTextAsync(csvPath, csvHeader + Environment.NewLine, Encoding.UTF8);

                state.Progress = 85;
                state.Logs.Add("Created data.csv header");

                // Download one sample image using a placeholder image for Level 0
                var assetsFolder = Path.Combine(assets, "Downloaded");
                Directory.CreateDirectory(assetsFolder);
                var placeholder = $"https://via.placeholder.com/1280x720.png?text={Uri.EscapeDataString(request.Topic)}";
                var asset = await _imageService.DownloadImageAsync(placeholder, assetsFolder);
                if (asset != null)
                {
                    state.Logs.Add($"Downloaded placeholder asset {asset.FileName}");
                }

                state.Progress = 95;
                state.Logs.Add("Downloaded sample images");

                state.Progress = 100;
                state.Status = "Completed";
                state.Logs.Add("Generation completed");
            }
            catch (Exception ex)
            {
                state.Status = "Failed";
                state.Logs.Add($"Error: {ex.Message}");
            }

            return new GenerateProjectResponseDto { ProjectId = projectId, Status = state.Status };
        }

        public Task<GenerationStatusDto> GetStatusAsync(string projectId)
        {
            var s = GenerationStateStore.Get(projectId);
            if (s == null)
            {
                return Task.FromResult(new GenerationStatusDto { ProjectId = projectId, Status = "NotFound", ProgressPercent = 0, Logs = new string[0] });
            }

            return Task.FromResult(new GenerationStatusDto { ProjectId = s.ProjectId, Status = s.Status, ProgressPercent = s.Progress, Logs = s.Logs.ToArray() });
        }

        private static string MakeSafeFolderName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }

        private TubePilotAI.Application.Abstractions.ExternalServices.IAIProvider ResolveProvider(string? providerName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(providerName) && Enum.TryParse<AIProviderType>(providerName, true, out var pType))
                {
                    return _providerResolver.GetProvider(pType);
                }

                // fallback to first registered
                return _providerResolver.GetAll().First();
            }
            catch
            {
                return _providerResolver.GetAll().First();
            }
        }
    }
}
