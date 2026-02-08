namespace orchestrator_portal.Dto
{
    public class ResourceAssociationRowVm
    {
        public int Id { get; set; }

        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = "";
        public string ResourceRbac { get; set; } = "";
        public string ResourceType { get; set; } = "";
        public int ServiceConnectionId { get; set; }
        public string ServiceConnectionName { get; set; } = "";
        public string ServiceConnectionDescription { get; set; } = "";
        public string RepoType { get; set; } = "";
    }
}
