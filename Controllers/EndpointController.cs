using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using orchestrator_portal.Db;
using orchestrator_portal.Dto;
using System.Text.Json;

namespace orchestrator_portal.Controllers
{
    [Authorize]
    [ApiController]
    public class EndpointController : ControllerBase
    {

        private readonly IAzureDevopsFactory _devopsFactory;
        private readonly ILogger<EndpointsController> _logger;
        private readonly AppDbContext _db;
        private readonly AzureArmService _azurearm;

        public EndpointController( ILogger<EndpointsController> logger,IAzureDevopsFactory devopsFactory, AppDbContext db, AzureArmService Azurearmservice)
        {
            _logger = logger;
            _devopsFactory = devopsFactory;
            _db = db;
            _azurearm = Azurearmservice;
        }

        [HttpGet("/api/subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var list = await _db.Subscriptions
                .OrderBy(s => s.displayName)
                .Select(s => new { 
                    s.displayName, 
                    s.subscriptionId
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("/api/serviceconnection/select")]
        public async Task<IActionResult> GetServiceConnection(string repoType)
        {
            if (!Enum.TryParse<RepoType>(repoType, true, out var parsedRepoType))
                return BadRequest("Invalid repoType");

            var list = await _db.ServiceConnection
                .Where(s => s.RepoType == parsedRepoType &&
                _db.ResourceAssociation.Any(ra => ra.ServiceConnectionId == s.Id)
                )
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Scope
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("/api/variablegroup/select")]
        public async Task<IActionResult> GetVariableGroup()
        {

            var list = await _db.VariableGroup
                .Where( s =>_db.VariableGroupAssociation.Any(ra => ra.VariableGroupId == s.Id)
                )
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("/api/serviceconnection/getbyid")]
        public async Task<IActionResult> GetServiceConnectionById(int Id)
        {

            var list = await (
                 from ra in _db.ResourceAssociation
                 join r in _db.Resources
                     on ra.ResourceId equals r.Id
                 where ra.ServiceConnectionId == Id
                 select new
                 {
                     // ResourceAssociation fields
                     ra.Id,
                     ra.ResourceId,
                     ra.AzureResourceId,
                     ra.ServiceConnectionId,

                     // Resources fields
                     ResourceName = r.Name,
                     r.Rbac,
                     r.type
                 }
            ).ToListAsync();

            return Ok(list);
        }

        [HttpGet("/api/vargroup/getbyid")]
        public async Task<IActionResult> GetvargroupById(int Id)
        {

            var list = await (
                 from vga in _db.VariableGroupAssociation
                 join v in _db.VariableGroup
                     on vga.VariableGroupId equals v.Id
                 where vga.VariableGroupId == Id
                 select new
                 {
                     // VariableGroupAssociation fields
                     vga.Id,
                     vga.VariableGroupId,
                     vga.Key,
                     vga.Value,

                     // VariableGroup fields
                     VargroupName = v.Name
                 }
            ).ToListAsync();

            return Ok(list);
        }

        [HttpPost("/api/serviceconnection/ResourceGroups")]
        public async Task<IActionResult> GetAzureResourceResourceGroups([FromBody] ResourceGroupsRequest request)
        {
            var list = new List<ResourceGroups>();
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                //Console.WriteLine(token);
                try
                {
                    list = await _azurearm.SyncUserResourceGroupsAsync(User, tenantId, request);

                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    //Challenge here, inside the controller
                    return Challenge(
                        new AuthenticationProperties { RedirectUri = "/" },
                        OpenIdConnectDefaults.AuthenticationScheme);
                }

            }
            return Ok(list);
        }

        [HttpPost("/api/serviceconnection/Subscriptions")]
        public async Task<IActionResult> GetAzureResourceSubscriptions([FromBody] AzureSubscriptionRequest request)
        {
            var list = new List<AzureSubscription>();
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                //Console.WriteLine(token);
                try
                {
                    list = await _azurearm.SyncUserAzureSubscriptionAsync(User, tenantId, request);

                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    //Challenge here, inside the controller
                    return Challenge(
                        new AuthenticationProperties { RedirectUri = "/" },
                        OpenIdConnectDefaults.AuthenticationScheme);
                }

            }
            return Ok(list);
        }

        [HttpPost("/api/serviceconnection/StorageAccount")]
        public async Task<IActionResult> GetAzureResourceStorageAccount([FromBody] StorageAccountRequest request)
        {
            var list = new List<StorageAccount>();
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                //Console.WriteLine(token);
                try
                {
                   list = await _azurearm.SyncUserStorageAccountAsync(User, tenantId, request);

                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    //Challenge here, inside the controller
                    return Challenge(
                        new AuthenticationProperties { RedirectUri = "/" },
                        OpenIdConnectDefaults.AuthenticationScheme);
                }

            }
            return Ok(list);
        }

        [HttpPost("/api/serviceconnection/KeyVault")]

        public async Task<IActionResult> GetAzureResourceKeyVault([FromBody] KeyVaultRequest request)
        {
            var list = new List<KeyVault>();
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                //Console.WriteLine(token);
                try
                {
                    list = await _azurearm.SyncUserKeyVaultAsync(User, tenantId, request);

                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    //Challenge here, inside the controller
                    return Challenge(
                        new AuthenticationProperties { RedirectUri = "/" },
                        OpenIdConnectDefaults.AuthenticationScheme);
                }

            }
            return Ok(list);
        }

        [HttpGet("/api/projects")]
        public async Task<IActionResult> GetProjects(string? search)
        {

            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                string token = await _devopsFactory.GetDevOpsTokenAsync(User, tenantId);
                //Console.WriteLine($"token : {token}");

                try
                {
                    var activeOrg = await _db.Organization.FirstOrDefaultAsync(o => o.IsActive);

                    if (activeOrg != null)
                    {
                        Console.WriteLine($"organization: {activeOrg.name}");
                        var projects = await _devopsFactory.SearchforProjects(token, activeOrg.name);

                        // Get existing project IDs to avoid duplicates
                        var existingIds = (await _db.Projects
                        .Where(p => p.Organization == activeOrg.name)
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
                                Organization = activeOrg.name
                            })
                            .ToList();

                        // Insert into DB
                        if (newProjects.Any())
                        {
                            _db.Projects.AddRange(newProjects);
                            await _db.SaveChangesAsync();
                        }
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


            var activeOrgquery = await _db.Organization
                                .Where(o => o.IsActive)
                                .Select(o => new
                                {
                                    o.name,
                                    o.AutomationProjectname,
                                    o.terraformProjectname
                                })
                                .FirstOrDefaultAsync();
            if (activeOrgquery == null)
            {
                return Ok(Array.Empty<string>());
            }

            var query = _db.Projects
                .Where(p =>
                    p.Organization == activeOrgquery.name &&
                    p.Projectname != activeOrgquery.AutomationProjectname &&
                    p.Projectname != activeOrgquery.terraformProjectname
                );

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Projectname.Contains(search));
            }

            var list = await query
                .OrderBy(p => p.Projectname)
                .Select(p => p.Projectname)
                .ToListAsync();

            return Ok(list);

        }

        [HttpPost("/api/data/selected")]
        public async Task<IActionResult> SaveData([FromBody] DataSelectionRequest request)
        {
            _logger.LogInformation("Project: {Project}", request.Options.Project_name);

            _logger.LogInformation("Recreate project: {Recreate}", request.Options.Recreate_project);


            _logger.LogInformation("Project: {Createrbac}", request.Options.Create_rbac);

            foreach (var sub in request.Subscriptions)
                _logger.LogInformation("Subscription: {Sub}", sub);

            foreach (var type in request.Requiredrepos)
                _logger.LogInformation("required repos: {Type}", type);


            string result="General Error , Data couldnt Load";
            int code=400;
            int pipelinedefinition = 686; //old was 25 ,this is for the new

            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (tenantId != null && tenantId != "9188040d-6c67-4c5b-b112-36a304b66dad")
            {
                string token = await _devopsFactory.GetDevOpsTokenAsync(User, tenantId);
                var firstOrg = _db.Organization
                                .Where(o => o.IsActive)
                                .FirstOrDefault();

                if (firstOrg != null)
                {
                    string orgname = firstOrg.name;
                    string automationproject = firstOrg.AutomationProjectname;

                    _logger.LogInformation($"org: {orgname} , automation project: {automationproject}");


                    bool isRunning = await _devopsFactory.IsPipelineRunningAsync(token,
                                    organization: orgname,
                                    coreproject: automationproject,
                                    projecttobecreated: request.Options.Project_name,
                                    pipelineId: pipelinedefinition
                    );

                    _logger.LogInformation($"isrunning: {isRunning}");

                    if (isRunning)
                    {
                        result = $"pipeline for {request.Options.Project_name} is already running";
                        code = 400;
                        _logger.LogInformation($"pipeline for {request.Options.Project_name} is already running");
                    }
                    else
                    {
                        result = $"pipeline for {request.Options.Project_name} is  starting";
                        code = 200;
                        _logger.LogInformation($" pipeline  for {request.Options.Project_name} is starting");

                        try
                        {
                            await _devopsFactory.TriggerPipelineRunAsync(token,
                            organization: orgname,
                            coreproject: automationproject,
                            request: request,
                            pipelineId: pipelinedefinition,
                            pool: "aks pool" //ignored for the moment
                            );
                        }
                        catch (Exception ex)
                        {
                            string friendlyMessage = ex.Message;

                            // Try to extract JSON part from the message
                            int jsonStart = ex.Message.IndexOf('{');
                            if (jsonStart >= 0)
                            {
                                try
                                {
                                    var jsonPart = ex.Message.Substring(jsonStart);
                                    using var doc = JsonDocument.Parse(jsonPart);
                                    if (doc.RootElement.TryGetProperty("message", out var messageProp))
                                    {
                                        friendlyMessage = messageProp.GetString() ?? friendlyMessage;
                                    }
                                }
                                catch
                                {
                                    // ignore parsing errors, fallback to original ex.Message
                                }
                            }
                            result = friendlyMessage;
                            code = 400;
                        }
                    }
                }
            }
            return Ok(new
            {
                message = result,
                htmlcode = code
            }); 
        }



        [HttpGet("/api/subscription/fordeploy")]
        public async Task<IActionResult> Get(string subscriptionid)
        {
            var list = await _db.Subscriptions
                    .Where(s => s.subscriptionId == subscriptionid)
                    .OrderBy(s => s.displayName)
                    .Select(s => s.fordeploy)
                    .FirstOrDefaultAsync();
            return Ok(list);
        }


        [HttpPost("/api/subscription/fordeploy")]
        public async Task<IActionResult> UpdateForDeploy([FromForm]  string subscriptionid, [FromForm] bool fordeploy)
        {
            _logger.LogInformation("subscription: {subscription}", subscriptionid);

            _logger.LogInformation("For deploy : {fordeploy}", fordeploy);

            var entity = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.subscriptionId == subscriptionid);

            if (entity == null)
                return NotFound();

            entity.fordeploy = fordeploy;
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("/api/serviceconnection/saveresources")]
        public async Task<IActionResult> SaveResourceInResourceAssociation([FromBody] List<SaveResourceAssociationDto> ResourceAssociation)
        {
            foreach (var dto in ResourceAssociation)
            {
                var entity = await _db.ResourceAssociation.FindAsync(dto.ResourceAssociationId);
                if (entity == null) continue;
                _logger.LogInformation($"updating  ResourceAssociation with id {dto.ResourceAssociationId}");

                _logger.LogInformation($"new azureresourceid {dto.AzureResourceId}");


                entity.AzureResourceId = dto.AzureResourceId;
            }

            await _db.SaveChangesAsync();
            return Ok();
            //return RedirectToAction(nameof(Index));
        }

        [HttpPost("/api/vargroup/saveresources")]
        public async Task<IActionResult> SaveValuesinVargroupAssociation([FromBody] List<SaveVargroupAssociationDto> VargroupAssociation)
        {
            foreach (var dto in VargroupAssociation)
            {
                var entity = await _db.VariableGroupAssociation.FindAsync(dto.Id);
                if (entity == null) continue;
                _logger.LogInformation($"updating  VariableGroupAssociation with id {dto.Id}");

                _logger.LogInformation($"new value {dto.Value}");


                entity.Value = dto.Value;
            }

            await _db.SaveChangesAsync();
            return Ok();
            //return RedirectToAction(nameof(Index));
        }
    }
}
