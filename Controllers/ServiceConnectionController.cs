using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orchestrator_portal.Db;
using orchestrator_portal.Dto;

namespace orchestrator_portal.Controllers
{
    [Authorize]
    public class ServiceConnectionController : Controller
    {
        private readonly ILogger<ServiceConnectionController> _logger;
        private readonly AppDbContext _context;

        public ServiceConnectionController(ILogger<ServiceConnectionController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View(new ServiceConnectionVm
            {
                ServiceConnection = _context.ServiceConnection.ToList(),
                RepoType =
                [
                    RepoType.Infra.ToString(),
                    RepoType.Code.ToString()
                ]
            });
        }

        [HttpPost("/ServiceConnection/Save")]
        public async Task<IActionResult> Save(ServiceConnection model)
        {
            if (model.Id == 0)
            {
                // CREATE
                var newServiceConnection = new ServiceConnection
                {
                    Name = model.Name,
                    RepoType = model.RepoType,
                    Description = model.Description,
                    Scope = model.Scope
                };

                _context.ServiceConnection.Add(newServiceConnection);
                _logger.LogInformation("create mode");
            }
            else
            {
                _logger.LogInformation("edit mode");

                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Index));

                var existing = await _context.ServiceConnection.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = model.Name;
                _logger.LogInformation($"existing repotype: {existing.RepoType}, new value: {model.RepoType}");

                if (existing.RepoType != model.RepoType)
                {

                    var removeinresourceassociation = await _context.ResourceAssociation.Where(s => s.ServiceConnectionId == model.Id).ToListAsync();
                    if (removeinresourceassociation.Any())
                    {
                        _logger.LogInformation($"removing relationship from service connection with id {model.Id} from ResourceAssociation db");
                        _context.ResourceAssociation.RemoveRange(removeinresourceassociation);
                        await _context.SaveChangesAsync();
                    }
                }
                existing.RepoType = model.RepoType;
                existing.Scope = model.Scope;
                existing.Description = model.Description;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/ServiceConnection/Delete")]
        public async Task<IActionResult> Delete(ServiceConnection model)
        {
            if (model.Id == 0)
                return BadRequest();

            var existing = await _context.ServiceConnection.FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            _context.ServiceConnection.Remove(existing);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted service connection with id {model.Id}");

            return RedirectToAction(nameof(Index));
        }

        //special dto only for web request binding since the database model use custom enum type  called repotype 
        //string is passed from body and is parse into enum type on insert
        [HttpPost("/ServiceConnection/UpdateAll")]
        public async Task<IActionResult> UpdateAll([FromBody] List<ServiceConnectionUpdateDto> ServiceConnection)
        {
            foreach (var dto in ServiceConnection)
            {
                var entity = await _context.ServiceConnection.FindAsync(dto.Id);
                if (entity == null) continue;

                if (!Enum.TryParse<RepoType>(dto.RepoType, true, out var repoType))
                {
                    return BadRequest($"Invalid RepoType: {dto.RepoType}");
                }

                entity.Name = dto.Name;
                entity.Description = dto.Description;

                if (entity.RepoType != repoType)
                {

                    var removeinresourceassociation = await _context.ResourceAssociation.Where(s => s.ServiceConnectionId == dto.Id).ToListAsync();
                    if (removeinresourceassociation.Any())
                    {
                        _logger.LogInformation($"removing relationship from service connection with id {dto.Id} from ResourceAssociation db");
                        _context.ResourceAssociation.RemoveRange(removeinresourceassociation);
                        await _context.SaveChangesAsync();
                    }
                }
                entity.RepoType = repoType;
                entity.Scope = dto.Scope;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
