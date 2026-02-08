using orchestrator_portal.Dto;
using System.ComponentModel.DataAnnotations;

namespace orchestrator_portal.Db
{
    public class VariableGroup
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; } //name of variable group
        public required string Description { get; set; }
    }
}
