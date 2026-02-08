using System.ComponentModel.DataAnnotations;

namespace orchestrator_portal.Db
{
    public class ResourceAssociation
    {
        [Key]
        public int Id { get; set; }
        public required int  ResourceId { get; set; } //from resource definiton db
        public  string? AzureResourceId { get; set; } //from azure resource itself , discovered on runtime
        public required int ServiceConnectionId { get; set; } //from service connection definiton db
    }
}
