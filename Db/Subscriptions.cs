using System.ComponentModel.DataAnnotations;

public class Subscriptions
{
    [Key]
    public int Id { get; set; }
    public required string displayName { get; set; }
    public required string subscriptionId { get; set; }
    public required string state { get; set; }
    public required bool fordeploy { get; set; }

}


