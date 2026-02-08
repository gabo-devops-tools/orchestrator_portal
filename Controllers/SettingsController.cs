using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using orchestrator_portal.Dto;

namespace orchestrator_portal.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly AppDbContext _db;
        private readonly IAzureDevopsFactory _devopsFactory;
        private readonly AzureArmService _azurearm;

        public SettingsController(ILogger<SettingsController> logger, AppDbContext db, IAzureDevopsFactory devopsFactory, AzureArmService azurearm)
        {
            _db = db;
            _logger = logger;
            _devopsFactory = devopsFactory;
            _azurearm = azurearm;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/api/settings/selected")]
        public async Task<IActionResult> saveSettingsData([FromBody] SettingsSelectionRequest request)
        {
            _logger.LogInformation("Project: {Project}", request.Project);

            _logger.LogInformation("Terraform Project: {Project}", request.Terraform_Project);


            _logger.LogInformation("Org: {org}", request.org);

            var organizations = await _db.Organization.ToListAsync();

            foreach (var org in organizations)
            {
                if (org.name == request.org) // assuming your request sends the OrgId
                {
                    org.IsActive = true;
                    org.AutomationProjectname = request.Project;
                    org.terraformProjectname = request.Terraform_Project;
                }
                else
                {
                    org.IsActive = false;
                }
            }

            await _db.SaveChangesAsync();


            string result = "Saved Configuration";


            return Ok(new
            {
                message = result
            });
        }

        [HttpGet("/api/organizations/default")]
        public async Task<IActionResult> GetOrganizationsDefault()
        {
            var query = _db.Organization.AsQueryable();

            query = query.Where(o => o.IsActive == true);

            var list = await query
                .Select(o => o.name)
                .ToListAsync();

            return Ok(list ?? []);
        }

        [HttpGet("/api/project/automation/default")]
        public async Task<IActionResult> GetAutomationprojectDefault()
        {
            var query = _db.Organization.AsQueryable();

            query = query.Where(o => o.IsActive == true);

            var list = await query
                .Select(o => o.AutomationProjectname)
                .ToListAsync();

            return Ok(list ?? []);
        }

        [HttpGet("/api/project/terraform/default")]
        public async Task<IActionResult> GetTerraformprojectDefault()
        {
            var query = _db.Organization.AsQueryable();

            query = query.Where(o => o.IsActive == true);

            var list = await query
                .Select(o => o.terraformProjectname)
                .ToListAsync();

            return Ok(list ?? []);
        }


        [HttpGet("/api/organizations")]
        public async Task<IActionResult> GetOrganizations(string? search)
        {
            var query = _db.Organization.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.name.Contains(search));

            var list = await query
                .Select(o => o.name)
                .ToListAsync();

            return Ok(list ?? []);
        }

        [HttpPost("/api/organizations")]
        public async Task<IActionResult> Post([FromBody] Organization dto)
        {
            // Check if it already exists
            bool exists = await _db.Organization.AnyAsync(o => o.name == dto.name);
            if (!exists)
            {
                _db.Organization.Add(new Organization
                {
                    name = dto.name,
                    IsActive = dto.IsActive,
                    AutomationProjectname = dto.AutomationProjectname,
                    terraformProjectname = dto.terraformProjectname

                });
                await _db.SaveChangesAsync();
            }



            return Ok();
        }

        // ---------------- PROJECTS ----------------
        [HttpGet("/api/Settings/projects")]
        public async Task<IActionResult> GetSettingsProjects(string? search, string organization, string? project_to_exclude)
        {
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                string token = await _devopsFactory.GetDevOpsTokenAsync(User, tenantId);
                //Console.WriteLine($"token : {token}");
                Console.WriteLine($"organization: {organization}");

                try
                {
                    var projects = await _devopsFactory.SearchforProjects(token, organization);


                    // Get existing project IDs to avoid duplicates
                    var existingIds = (await _db.Projects
                        .Where(p => p.Organization == organization)
                        .Select(p => p.ProjectId)
                        .ToListAsync())
                        .ToHashSet();

                    // Prepare new projects
                    var newProjects = projects
                        .Where(p => !existingIds.Contains(p.Id))
                        .Select(p => new Projects
                        {
                            ProjectId = p.Id,
                            Projectname = p.Name,
                            Organization = organization
                        })
                        .ToList();

                    // Insert into DB
                    if (newProjects.Any())
                    {
                        _db.Projects.AddRange(newProjects);
                        await _db.SaveChangesAsync();
                    }

                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    //Challenge here, inside the controller
                    return Challenge(
                        new AuthenticationProperties { RedirectUri = "/" },
                        OpenIdConnectDefaults.AuthenticationScheme);
                }

            }


            var query = _db.Projects.AsQueryable();

            // Filter by organization
            query = query.Where(p => p.Organization == organization && p.Projectname != project_to_exclude);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.Projectname.Contains(search));

            var list = await query
                .OrderBy(p => p.Projectname)
                .Select(p => p.Projectname)
                .ToListAsync();

            return Ok(list ?? []);
        }
    }
}
