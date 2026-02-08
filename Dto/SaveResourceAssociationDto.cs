namespace orchestrator_portal.Dto
{
    public class SaveResourceAssociationDto
    {
        public int ServiceConnectionId { get; set; }
        public int ResourceAssociationId { get; set; }
        public string AzureResourceId { get; set; } = string.Empty;
    }
}
