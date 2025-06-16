using Template.Models.Dtos;

namespace Template.Services;

public interface IDbService 
{
    Task<ProjectDto> GetByIdAsync(int id);
    Task CreateArtifactAsync(CreateArtifactProjectDto request);
}