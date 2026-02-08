namespace orchestrator_portal.Dto
{
    //associated with a repo type and it will contains resources
    public class ServiceConnectionUpdateDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Scope { get; set; }
        public required string RepoType { get; set; }
        public required string Description { get; set; }
    }
}
