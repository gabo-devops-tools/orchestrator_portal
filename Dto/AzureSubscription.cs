namespace orchestrator_portal.Dto
{
    public class AzureSubscription
    {
        public  string? Id { get; set; }
        public required string displayName { get; set; }
        public required string subscriptionId { get; set; }
        public required string state { get; set; }

    }

}
