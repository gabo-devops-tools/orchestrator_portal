using orchestrator_portal.Db;

namespace orchestrator_portal.Dto
{
    public class VariableGroupVm
    {
        public IEnumerable<VariableGroup> VariableGroup { get; set; } = [];
        public IEnumerable<VariableGroupAssociation> VariableGroupsAssociation { get; set; } = [];
    }
}
