using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers;


public class RolesController : BaseController
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> PostRole(RoleCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var role = new IdentityRole()
        {
            Id = request.Id,
            Name = request.Name,
            NormalizedName = request.Name.ToUpper(),
        };
        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, request);
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
    public async Task<IActionResult> PutRole(string id, [FromBody] RoleCreateRequest request)
    {
        if (id != request.Id)
            return BadRequest();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        role.Name = request.Name;
        role.NormalizedName = request.Name.ToUpper();

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
    
    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetPermissionByRoleId(string roleId)
    {
        var permissions = from p in _context.Permissions

                    join a in _context.Commands 
                        on p.CommandId equals a.Id
                    where p.RoleId == roleId
                    select new PermissionVm()
                    {
                        FunctionId = p.FunctionId,
                        CommandId = p.CommandId,
                        RoleId = p.RoleId
                    };

        return Ok(await permissions.ToListAsync());
    }
    
    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> PutPermissionByRoleId(string roleId, [FromBody] UpdatePermissionRequest request)
    {
        //create new permission list from user changed
        var newPermissions = new List<Permission>();
        foreach (var p in request.Permissions)
        {
            newPermissions.Add(new Permission(p.FunctionId, roleId, p.CommandId));
        }
        var existingPermissions = _context.Permissions.Where(x => x.RoleId == roleId);

        _context.Permissions.RemoveRange(existingPermissions);
        _context.Permissions.AddRange(newPermissions);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return NoContent();
        }

        return BadRequest("Save permission failed");
    }
}