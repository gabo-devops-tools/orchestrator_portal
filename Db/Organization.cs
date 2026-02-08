using System.ComponentModel.DataAnnotations;

public class Organization
{
    [Key]
    public int Id { get; set; }
    public required string name { get; set; }
    public required bool IsActive { get; set; } = false;
    public required string AutomationProjectname { get; set; }
    public required string terraformProjectname { get; set; }
}