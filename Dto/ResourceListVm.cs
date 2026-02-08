using orchestrator_portal.Db;

namespace orchestrator_portal.Dto
{
    public class ResourceListVm
    {
        public IEnumerable<Resources> Resources { get; set; } = [];
        public IEnumerable<string> RbacOptions { get; set; } = [];
        public IEnumerable<string> typeOptions { get; set; } = [];
    }
}
