using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using orchestrator_portal.Models;
using System.Diagnostics;

namespace orchestrator_portal.Controllers
{
    [Authorize]
    [AuthorizeForScopes(Scopes = new[] { "https://management.azure.com/user_impersonation" })]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AzureArmService _azurearm;

        public HomeController(ILogger<HomeController> logger, AzureArmService azurearm)
        {
            _logger = logger;
            _azurearm = azurearm;
        }
        // public IActionResult Index()
        public async Task<IActionResult> CreateProjects()
        {

            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                await _azurearm.SyncUserSubscriptionsAsync(User, tenantId);
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
