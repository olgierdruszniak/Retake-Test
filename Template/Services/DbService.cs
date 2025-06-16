using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Template.Exceptions;
using Template.Models.Dtos;

namespace Template.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<ProjectDto> GetByIdAsync(int id)
    {
        var query = @"SELECT pp.project_id, pp.objective, pp.start_date, pp.end_date, a.name, a.origin_date, i.institution_id, i.name, i.founded_year, s.first_name, s.last_name, s.hire_date, sa.role
                    FROM Staff s 
                    JOIN Staff_Assignment sa ON s.staff_id = sa.staff_id
                    JOIN Preservation_Project pp ON sa.project_id = pp.project_id
                    JOIN Artifact a ON pp.artifact_id = a.artifact_id
                    JOIN Institution i ON i.institution_id = a.institution_id
                    WHERE pp.project_id = @id;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();
        
        ProjectDto? dto = null;
        var staffAssignments = new List<staffAssignmentDto>();
        while (await reader.ReadAsync())
        {
            if (dto is null)
            {
                dto = new ProjectDto()
                {
                    ProjectId = reader.GetInt32(0),
                    Objective = reader.GetString(1),
                    StartDate = reader.GetDateTime(2),
                    EndDate = reader.GetDateTime(3),
                    Artifact = new ArtifactDto()
                    {
                        Name = reader.GetString(4),
                        OriginDate = reader.GetDateTime(5),
                        Institution = new InstitutionDto()
                        {
                            InstitutionId = reader.GetInt32(6),
                            Name = reader.GetString(7),
                            FoundedYear = reader.GetInt32(8),
                        }
                    },
                    StaffAssignments = staffAssignments
                };
            }

            if (!reader.IsDBNull(9))
            {
                staffAssignments.Add(new staffAssignmentDto()
                {
                    FirstName = reader.GetString(9),
                    LastName = reader.GetString(10),
                    HireDate = reader.GetDateTime(11),
                    Role = reader.GetString(12),
                });
            }
        }
        return dto;
    }

    public async Task CreateArtifactAsync(CreateArtifactProjectDto request)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            //sprawdzanie czy obiekty ktore sa dodawane juz istnieja badz nie
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Artifact WHERE artifact_id = @ArtifactId;";
            command.Parameters.AddWithValue("@ArtifactId", request.Artifact.ArtifactId);
            
            var artifactExists = await command.ExecuteScalarAsync();
            if (artifactExists != null)
            {
                throw new ConflictException("Such artifact already exists");
            }
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Institution WHERE InstitutionId = @InstitutionId;";
            command.Parameters.AddWithValue("@InstitutionId", request.Artifact.InstitutionId);
             
            var institutionExists = await command.ExecuteScalarAsync();
            if (institutionExists is null)
            {
                throw new NotFoundException("Institution not found");
            }
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Project WHERE project_id = @ProjectId;";
            command.Parameters.AddWithValue("@ProjectId", request.Project.ProjectId);
            
            var projectExists = await command.ExecuteScalarAsync();
            if (projectExists != null)
            {
                throw new ConflictException("Such project already exists");
            }
            
            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO Artifact VALUES(@ArtifactId, @Name, @OriginDate, @InstitutionId);";
            command.Parameters.AddWithValue("@ArtifactId", request.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@Name", request.Artifact.Name);
            command.Parameters.AddWithValue("@OriginDate", request.Artifact.OriginDate);
            command.Parameters.AddWithValue("@InstitutionId", request.Artifact.InstitutionId);
            
            await command.ExecuteNonQueryAsync();
            
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO Preservation_Project VALUES(@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective)";
            command.Parameters.AddWithValue("@ProjectId", request.Project.ProjectId);
            command.Parameters.AddWithValue("@ArtifactId", request.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@StartDate", request.Project.StartDate);
            command.Parameters.AddWithValue("@EndDate", request.Project.EndDate);
            command.Parameters.AddWithValue("@Objective", request.Project.Objective);

            await command.ExecuteNonQueryAsync();




            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}