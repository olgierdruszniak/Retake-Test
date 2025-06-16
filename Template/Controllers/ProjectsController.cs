using Microsoft.AspNetCore.Mvc;
using Template.Exceptions;
using Template.Models.Dtos;
using Template.Services;

namespace Template.Controllers;

[Route("api")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ProjectsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("[controller]/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var response = await _dbService.GetByIdAsync(id);
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("artifacts")]
    public async Task<IActionResult> Post([FromBody] CreateArtifactProjectDto dto)
    {
        try
        {
            await _dbService.CreateArtifactAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = dto.Project.ProjectId }, dto);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}