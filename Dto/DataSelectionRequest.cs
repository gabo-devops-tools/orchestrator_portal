
namespace orchestrator_portal.Dto
{
    public class DataSelectionRequest
    {
        public required List<string> Subscriptions { get; set; }
        public required DataSelectServiceOptions Options { get; set; }
        public required List<string> Requiredrepos { get; set; }
        public required List<DataSelectServiceConnection> ServiceConnections { get; set; }
        public required List<DataSelectVargroup> Variablegroups { get; set; }
    }
    public class DataSelectAzureResource
    {
        public required string AzureResourceId { get; set; }
        public required string Rbac { get; set; }
    }

    public class DataSelectVargroupkey
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
    }

    public class DataSelectVargroup
    {
        public required string Name { get; set; }
        public required List<DataSelectVargroupkey> Resources { get; set; }
    }

    public class DataSelectServiceConnection
    {
        public required string Name { get; set; }
        public required string Scope { get; set; }
        public required List<DataSelectAzureResource> Resources { get; set; }
    }

    public class DataSelectServiceOptions
    {
        public required string Recreate_project { get; set; }
        public required string Project_name { get; set; }
        public required string Create_rbac { get; set; }
    }
}
