using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<IActionResult> PostRole(RoleVm roleVm)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var role = new IdentityRole()
        {
            Id = roleVm.Id,
            Name = roleVm.Name,
            NormalizedName = roleVm.Name.ToUpper(),
        }; 
        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, roleVm);
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = _roleManager.Roles;

        var rolevms = await roles.Select(r => new RoleVm()
        {
            Id = r.Id,
            Name = r.Name,
        }).ToListAsync();
        
        return Ok(rolevms);
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetAllRolesPaging(string filter, int pageIndex, int pageSize)
    {
        var query =  _roleManager.Roles;
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x => x.Id.Contains(filter) || x.Name!.Contains(filter));
        }

        var totalRecords = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(r=>new RoleVm() {
            Id = r.Id,
            Name = r.Name})
            .ToListAsync();
        var pagination = new Pagination<RoleVm>
        {
            Items = items,
            TotalRecords = totalRecords,
        };
        return Ok(pagination);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();
        
        var roleVm = new RoleVm()
        {
            Id = role.Id,
            Name = role.Name,
        };
        return Ok(roleVm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutRole(string id, [FromBody] RoleVm roleVm)
    {
        if (id != roleVm.Id)
            return BadRequest();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        role.Name = roleVm.Name;
        role.NormalizedName = roleVm.Name.ToUpper();

        var result = await _roleManager.UpdateAsync(role);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            var rolevm = new RoleVm
            {
                Id = role.Id,
                Name = role.Name,
            };
            return Ok(rolevm);
        }

        return BadRequest(result.Errors);
    }

}