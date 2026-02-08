namespace orchestrator_portal.Dto
{
    public class SettingsSelectionRequest
    {
        public required string Terraform_Project { get; set; }
        public required string Project { get; set; }
        public required string org { get; set; }
    }
}