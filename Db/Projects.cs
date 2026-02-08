using System.ComponentModel.DataAnnotations;

public class Projects
{
    [Key]
    public int Id { get; set; }
    public required string  Projectname { get; set; }
    public required string ProjectId { get; set; }
    public required string Organization { get; set; }
}