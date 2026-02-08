
using Microsoft.AspNetCore.Mvc;

namespace orchestrator_portal.Dto
{
    public class Build
    {
        public int Id { get; set; }
        public string Status { get; set; } = "";
    }
}