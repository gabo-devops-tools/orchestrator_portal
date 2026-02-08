using System.ComponentModel.DataAnnotations;

namespace orchestrator_portal.Db
{
    public class Resources
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Rbac { get; set; }
        public required string Description { get; set; }
        public required string type { get; set; }
        public required bool applyforinfra { get; set; }
        public required bool applyforcode { get; set; }


    }
}
