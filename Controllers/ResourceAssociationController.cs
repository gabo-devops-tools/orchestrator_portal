using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using orchestrator_portal.Db;
using orchestrator_portal.Dto;

namespace orchestrator_portal.Controllers
{
    [Authorize]
    public class ResourceAssociationController : Controller
    {

        private readonly ILogger<ResourceAssociationController> _logger;
        private readonly AppDbContext _context;

        public ResourceAssociationController(ILogger<ResourceAssociationController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string mode = "infra")
        {
            mode = mode.ToLowerInvariant();

            var vmcode = new ResourceAssociationListVm
            {
                Mode = "Code",
                ResourceAssociation = _context.ResourceAssociation.ToList(),
                Rows = from ra in _context.ResourceAssociation
                       join r in _context.Resources.Where(r => r.applyforcode)
                           on ra.ResourceId equals r.Id
                       join sc in _context.ServiceConnection.Where(sc => sc.RepoType == RepoType.Code)
                           on ra.ServiceConnectionId equals sc.Id
                       select new ResourceAssociationRowVm
                       {
                           Id = ra.Id,

                           ResourceId = r.Id,
                           ResourceName = r.Name,
                           ResourceType = r.type,
                           ResourceRbac = r.Rbac,

                           ServiceConnectionId = sc.Id,
                           ServiceConnectionName = sc.Name,
                           ServiceConnectionDescription = sc.Description,
                       },
                ServiceConnection = _context.ServiceConnection.Where(s => s.RepoType == RepoType.Code).ToList(),
                Resources = _context.Resources.Where(s => s.applyforcode == true).ToList()
            };


            var vminfra = new ResourceAssociationListVm
            {
                Mode = "Infra",
                ResourceAssociation = _context.ResourceAssociation.ToList(),
                Rows = from ra in _context.ResourceAssociation
                       join r in _context.Resources.Where(r => r.applyforinfra)
                           on ra.ResourceId equals r.Id
                       join sc in _context.ServiceConnection.Where(sc => sc.RepoType == RepoType.Infra)
                           on ra.ServiceConnectionId equals sc.Id
                       select new ResourceAssociationRowVm
                       {
                           Id = ra.Id,

                           ResourceId = r.Id,
                           ResourceName = r.Name,
                           ResourceType = r.type,
                           ResourceRbac = r.Rbac,

                           ServiceConnectionId = sc.Id,
                           ServiceConnectionName = sc.Name,
                           ServiceConnectionDescription = sc.Description,
                       },
                ServiceConnection = _context.ServiceConnection.Where(s => s.RepoType == RepoType.Infra).ToList(),
                Resources = _context.Resources.Where(s => s.applyforinfra == true).ToList()
            };

            return mode == "code" ? View(vmcode): View(vminfra);
        }

        [HttpPost("/ResourceAssociation/Save")]
        public async Task<IActionResult> Save(ResourceAssociation model)
        {
            if (model.Id == 0)
            {
                // CREATE new resource
                _logger.LogInformation("resource id: {ResourceId}", model.ResourceId);

                _logger.LogInformation("service connection id: {ServiceConnectionId}",model.ServiceConnectionId);

                var newResourceAssociation = new ResourceAssociation
                {
                    ResourceId = model.ResourceId,
                    ServiceConnectionId = model.ServiceConnectionId,
                    AzureResourceId= model.AzureResourceId// include Type if in model
                };

                _context.ResourceAssociation.Add(newResourceAssociation);
                _logger.LogInformation("create mode");
            }
            else
            {
                _logger.LogInformation("edit mode");

                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Index));

                var existing = await _context.ResourceAssociation.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.ResourceId = model.ResourceId;
                existing.ServiceConnectionId = model.ServiceConnectionId;

            }

            await _context.SaveChangesAsync();
            return Ok();
            //return RedirectToAction(nameof(Index));

        }

        [HttpPost("/ResourceAssociation/Delete")]
        public async Task<IActionResult> Delete(ResourceAssociation model)
        {
            if (model.Id == 0)
                return BadRequest();

            var existing = await _context.ResourceAssociation.FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            _context.ResourceAssociation.Remove(existing);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted ResourceAssociation with id {model.Id}");
            return Ok();
            //return RedirectToAction(nameof(Index));
        }

        [HttpPost("/ResourceAssociation/UpdateAll")]
        public async Task<IActionResult> UpdateAll([FromBody] List<ResourceAssociation> ResourceAssociation)
        {
            foreach (var dto in ResourceAssociation)
            {
                var entity = await _context.ResourceAssociation.FindAsync(dto.Id);
                if (entity == null) continue;
                _logger.LogInformation($"updating  ResourceAssociation with id {dto.Id}");

                _logger.LogInformation($"new resourceid {dto.ResourceId}");


                _logger.LogInformation($"new serviceconnectionid with id {dto.ServiceConnectionId}");


                entity.ResourceId = dto.ResourceId;
                entity.AzureResourceId = dto.AzureResourceId;
                entity.ServiceConnectionId = dto.ServiceConnectionId;
            }

            await _context.SaveChangesAsync();
            return Ok();
            //return RedirectToAction(nameof(Index));
        }
    }
}
