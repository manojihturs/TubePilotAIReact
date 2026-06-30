using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Application.Features.PromptTemplates;

public interface IPromptTemplateService
{
    Task<IReadOnlyList<PromptTemplateDto>> GetAsync(PromptTemplateQuery query, CancellationToken cancellationToken = default);

    Task<PromptTemplateDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PromptTemplateDto> CreateAsync(CreatePromptTemplateRequest request, CancellationToken cancellationToken = default);

    Task<PromptTemplateDto?> UpdateAsync(Guid id, UpdatePromptTemplateRequest request, CancellationToken cancellationToken = default);

    Task<PromptTemplateDto?> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
