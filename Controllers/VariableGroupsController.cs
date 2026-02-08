using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orchestrator_portal.Db;
using orchestrator_portal.Dto;

namespace orchestrator_portal.Controllers
{
    public class VariableGroupsController : Controller
    {
        private readonly ILogger<VariableGroupsController> _logger;
        private readonly AppDbContext _context;

        public VariableGroupsController(ILogger<VariableGroupsController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }
        public ActionResult Index()
        {
            return View(new VariableGroupVm
            {
                VariableGroup = _context.VariableGroup.ToList(),
                VariableGroupsAssociation = _context.VariableGroupAssociation.ToList()
            });
        }

        [HttpPost("/variablegroup/association/Save")]
        public async Task<IActionResult> Save(VariableGroupAssociation model)
        {
            if (model.Id == 0)
            {
                // CREATE new resource
                var newVariablegroupassociation = new VariableGroupAssociation
                {
                    VariableGroupId = model.VariableGroupId, 
                    Key = model.Key,
                    Value = model.Value
                };

                _context.VariableGroupAssociation.Add(newVariablegroupassociation);
                _logger.LogInformation("create mode");
            }
            else
            {
                _logger.LogInformation("edit mode");

                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Index));

                var existing = await _context.VariableGroupAssociation.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Key = model.Key;
                existing.Value = model.Value;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/variablegroup/association/Delete")]
        public async Task<IActionResult> Delete(VariableGroupAssociation model)
        {
            if (model.Id == 0)
                return BadRequest();

            var existing = await _context.VariableGroupAssociation.FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            _context.VariableGroupAssociation.Remove(existing);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted resource with id {model.Id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/variablegroup/Delete")]
        public async Task<IActionResult> DeleteVarGroup(VariableGroup model)
        {
            if (model.Id == 0)
                return BadRequest();

            var existing = await _context.VariableGroup.FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            _context.VariableGroup.Remove(existing);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted VariableGroup with id {model.Id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/variablegroup/Save")]
        public async Task<IActionResult> SaveVarGroup(VariableGroup model)
        {
            if (model.Id == 0)
            {
                // CREATE new resource
                var newVariableGroup = new VariableGroup
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description
                };

                _context.VariableGroup.Add(newVariableGroup);
                _logger.LogInformation("create mode");
            }
            else
            {
                _logger.LogInformation("edit mode");

                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Index));

                var existing = await _context.VariableGroup.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = model.Name;
                existing.Description = model.Description;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //usage of model complicate, just need the id
        [HttpPost("/variablegroup/Clone")]
        public async Task<IActionResult> CloneVarGroup(int Id)
        {
            if (Id == 0)
            {
                //never clone a model with id 0
                _logger.LogInformation("id 0 doesnt apply");
                return RedirectToAction(nameof(Index));

            }

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            _logger.LogInformation("clone mode");

            var existing = await _context.VariableGroup.FindAsync(Id);

            if (existing == null)
                return NotFound();

            var newVariableGroup = new VariableGroup
            {
                Name = existing.Name+"-clone",
                Description = existing.Description
            };

            _context.VariableGroup.Add(newVariableGroup);
            // ToString get the new var group id
            await _context.SaveChangesAsync();

            var existingassociation = await _context.VariableGroupAssociation.Where(s => s.VariableGroupId == Id).ToListAsync();

            //clone all resource association that have id model.Id to another with the idea obtained from the inserted above 
            foreach (var assoc in existingassociation)
            {
                var clonedAssoc = new VariableGroupAssociation
                {
                    VariableGroupId = newVariableGroup.Id, // 👈 NEW FK
                    Key = assoc.Key,
                    Value = assoc.Value
                };

                _context.VariableGroupAssociation.Add(clonedAssoc);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}