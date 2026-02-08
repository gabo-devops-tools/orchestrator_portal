namespace orchestrator_portal.Dto
{
    public class StorageAccount
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public StorageSku Sku { get; set; } = new StorageSku();
    }
}
