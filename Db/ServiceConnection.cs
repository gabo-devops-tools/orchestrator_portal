using orchestrator_portal.Dto;
using System.ComponentModel.DataAnnotations;

namespace orchestrator_portal.Db
{
    //associated with a repo type and it will contains resources
    public class ServiceConnection
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Scope { get; set; }
        public required RepoType RepoType { get; set; }
        public required string Description { get; set; }

    }
}
