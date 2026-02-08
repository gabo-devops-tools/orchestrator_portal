using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orchestrator_portal.Db;
using orchestrator_portal.Dto;
using System.Security.Principal;


namespace orchestrator_portal.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private readonly ILogger<ResourceController> _logger;
        private readonly AppDbContext _context;

        public ResourceController(ILogger<ResourceController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(new ResourceListVm
            {
                Resources = _context.Resources.ToList(),
                RbacOptions =
                [
                    "AcrPull",
                    "AcrPush",
                    "Azure Kubernetes Service Cluster Admin Role",
                    "Azure Kubernetes Service RBAC Cluster Admin",
                    "Contributor",
                    "Key Vault Secrets User",
                    "Key Vault Secrets Officer",
                    "Reader",
                    "Role Based Access Control Administrator",
                    "Storage Blob Data Contributor",
                    "Storage Blob Data Reader",
                    "User Access Administrator"
                ],
                typeOptions = [
                    "StorageAccount",
                    "KeyVault",
                    "ResourceGroups",
                    "Subscriptions",
                    "KubernetesService",
                    "ManagedIdentity",

                ]
            });
        }


        [HttpPost("/Resource/Save")]
        public async Task<IActionResult> Save(Resources model)
        {
            if (model.Id == 0)
            {
                // CREATE new resource
                var newResource = new Resources
                {
                    Name = model.Name,
                    Rbac = model.Rbac,
                    type = model.type,               // include Type if in model
                    Description = model.Description,
                    applyforinfra = model.applyforinfra,
                    applyforcode = model.applyforcode
                };

                _context.Resources.Add(newResource);
                _logger.LogInformation("create mode");
            }
            else
            {
                _logger.LogInformation("edit mode");

                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Index));

                var existing = await _context.Resources.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = model.Name;
                existing.Rbac = model.Rbac;
                existing.type = model.type;
                existing.Description = model.Description;

                if (existing.applyforinfra != model.applyforinfra)
                {

                    var removeinresourceassociation = await _context.ResourceAssociation.Where(s => s.ResourceId == model.Id).ToListAsync();
                    if (removeinresourceassociation.Any())
                    {
                        _logger.LogInformation($"removing relationship from resources with id {model.Id} from ResourceAssociation db");
                        _context.ResourceAssociation.RemoveRange(removeinresourceassociation);
                        await _context.SaveChangesAsync();
                    }
                }
                existing.applyforinfra = model.applyforinfra;
                if (existing.applyforcode != model.applyforcode)
                {

                    var removeinresourceassociation = await _context.ResourceAssociation.Where(s => s.ResourceId == model.Id).ToListAsync();
                    if (removeinresourceassociation.Any())
                    {
                        _logger.LogInformation($"removing relationship from resources with id {model.Id} from ResourceAssociation db");
                        _context.ResourceAssociation.RemoveRange(removeinresourceassociation);
                        await _context.SaveChangesAsync();
                    }
                }
                existing.applyforcode = model.applyforcode;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Resource/Delete")]
        public async Task<IActionResult> Delete(Resources model)
        {
            if (model.Id == 0)
                return BadRequest();

            var existing = await _context.Resources.FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            _context.Resources.Remove(existing);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted resource with id {model.Id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Resource/UpdateAll")]
        public async Task<IActionResult> UpdateAll([FromBody] List<Resources> resources)
        {
            foreach (var dto in resources)
            {
                var entity = await _context.Resources.FindAsync(dto.Id);
                if (entity == null) continue;

                entity.Name = dto.Name;
                entity.Description = dto.Description;
                entity.Rbac = dto.Rbac;
                entity.type = dto.type;
                entity.applyforinfra = dto.applyforinfra;
                entity.applyforcode = dto.applyforcode;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
