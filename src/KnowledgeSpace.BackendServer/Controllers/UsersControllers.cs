using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers;


public class UsersController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<User> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _context = context;
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<IActionResult> PostUser(UserCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Dob = request.Dob,
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users;

        var uservms = await users.Select(u => new UserVm()
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Dob = u.Dob,
        }).ToListAsync();
        
        return Ok(uservms);
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetAllUsersPaging(string filter, int pageIndex, int pageSize)
    {
        var query =  _userManager.Users;
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x => x.Id.Contains(filter) || 
                                     x.FirstName!.Contains(filter) || 
                                     x.LastName!.Contains(filter) || 
                                     x.PhoneNumber!.Contains(filter));
        }

        var totalRecords = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u=>new UserVm() {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Dob = u.Dob,
            }).ToListAsync();
        var pagination = new Pagination<UserVm>
        {
            Items = items,
            TotalRecords = totalRecords,
        };
        return Ok(pagination);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        
        var userVm = new UserVm()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Dob = user.Dob,
        };
        return Ok(userVm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(string id, [FromBody] UserCreateRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Dob = request.Dob;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
    
    [HttpPut("{id}/changePassword")]
    public async Task<IActionResult> PutUserPassword(string id, [FromBody] UserPasswordChangeRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user,  request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            var userVm = new UserVm()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Dob = user.Dob,
            };
            return Ok(userVm);
        }

        return BadRequest(result.Errors);
    }
    
    [HttpGet("{userId}/menu")]
    public async Task<IActionResult> GetMenuByUserPermission(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var roles = await _userManager.GetRolesAsync(user);
        var query = from f in _context.Functions
            join p in _context.Permissions
                on f.Id equals p.FunctionId
            join r in _roleManager.Roles on p.RoleId equals r.Id
            join a in _context.Commands
                on p.CommandId equals a.Id
            where roles.Contains(r.Name) && a.Id == "VIEW"
            select new FunctionsVm
            {
                Id = f.Id,
                Name = f.Name,
                Url = f.Url,
                ParentId = f.ParentId,
                SortOrder = f.SortOrder,
                Icon = f.Icon
            };
        var data = await query.Distinct()
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.SortOrder)
            .ToListAsync();
        return Ok(data);
    }

}