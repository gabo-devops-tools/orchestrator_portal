
using Microsoft.AspNetCore.Mvc;

namespace orchestrator_portal.Dto
{
    public class BuildListResponse
    {
        public int Count { get; set; }
        public List<Build> Value { get; set; } = new();
    }
}