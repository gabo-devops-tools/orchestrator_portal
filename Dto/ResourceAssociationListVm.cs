using orchestrator_portal.Db;

namespace orchestrator_portal.Dto
{
    public class ResourceAssociationListVm
    {
        public string Mode { get; set; } = "";  
        public IEnumerable<ResourceAssociation> ResourceAssociation { get; set; } = [];
        public IEnumerable<ResourceAssociationRowVm> Rows { get; set; } = [];
        public IEnumerable<Resources> Resources { get; set; } = [];
        public IEnumerable<ServiceConnection> ServiceConnection { get; set; } = [];
    }
}
