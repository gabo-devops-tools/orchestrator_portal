using orchestrator_portal.Dto;

namespace orchestrator_portal.dto
{
    public class DevopsProjectResponse
    {
        public required List<DevOpsProject> value { get; set; }
        public int Count { get; set; }                     // corresponds to "count"
    }
}
