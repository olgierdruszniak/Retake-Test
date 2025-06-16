namespace Template.Models.Dtos;


public class CreateArtifactProjectDto
{
    public CreateArtifactDto Artifact { get; set; }
    public CreateProjectDto Project { get; set; }
}

public class CreateArtifactDto
{
    public int ArtifactId { get; set; }
    public string Name { get; set; } = String.Empty;
    public DateTime OriginDate { get; set; }
    public int InstitutionId { get; set; }
}

public class CreateProjectDto
{
    public int ProjectId { get; set; }
    public String Objective { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }//spytac o typ DateTime
}