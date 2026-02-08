using orchestrator_portal.Db;

namespace orchestrator_portal.Dto
{
    public class ServiceConnectionVm
    {
        public IEnumerable<ServiceConnection> ServiceConnection { get; set; } = [];
        public IEnumerable<string> RepoType { get; set; } = [];
    }
}
