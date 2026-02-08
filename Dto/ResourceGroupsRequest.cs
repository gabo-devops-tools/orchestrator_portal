namespace orchestrator_portal.Dto
{
    public class ResourceGroupsRequest
    {
        public List<string> SubscriptionIds { get; set; } = new();
        public String search { get; set; } = "";
    }
}
