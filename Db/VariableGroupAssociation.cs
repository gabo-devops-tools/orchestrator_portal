using System.ComponentModel.DataAnnotations;

namespace orchestrator_portal.Db
{
    public class VariableGroupAssociation
    {
        [Key]
        public int Id { get; set; }
        public int VariableGroupId { get; set; }
        public required string Key { get; set; } //name of variable group key
        public required string Value { get; set; }
    }
}
