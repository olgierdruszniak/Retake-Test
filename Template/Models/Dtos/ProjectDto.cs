namespace Template.Models.Dtos;

public class ProjectDto
{
    public int ProjectId { get; set; }
    public String Objective { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ArtifactDto Artifact { get; set; }
    public List<staffAssignmentDto> StaffAssignments { get; set; }
}

public class ArtifactDto
{
    public String Name { get; set; } = String.Empty;
    public DateTime OriginDate { get; set; }
    public InstitutionDto Institution { get; set; }
}

public class InstitutionDto
{
    public int InstitutionId { get; set; }
    public String Name { get; set; } = String.Empty;
    public int FoundedYear { get; set; }
}

public class staffAssignmentDto
{
    public String FirstName { get; set; }
    public String LastName { get; set; } = String.Empty;
    public DateTime HireDate { get; set; }
    public String Role { get; set; } = String.Empty;
}