using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers;

public class FunctionsController: BaseController
{
    private readonly ApplicationDbContext _context;

    public FunctionsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> PostFunctions([FromBody]FunctionsCreateRequest request)
    {
        var functions = new Function()
        {
            Id = request.Id,
            Name = request.Name,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            Url = request.Url,
            Icon = request.Icon,
        };
        _context.Functions.AddAsync(functions);
        var result = await _context.SaveChangesAsync();
        
        if (result > 0)
        {
            return CreatedAtAction(nameof(GetById), new { id = functions.Id }, request);
        }
        else
        {
            return BadRequest();
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetFunctions()
    {
        var functions = _context.Functions;

        var functionsVms = await functions.Select(f => new FunctionsVm()
        { 
            Id = f.Id,
            Name = f.Name,
            ParentId = f.ParentId,
            SortOrder = f.SortOrder,
            Url = f.Url,
            Icon = f.Icon,
        }).ToListAsync();
        
        return Ok(functionsVms);
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetAllFunctionsPaging(string filter, int pageIndex, int pageSize)
    {
        var query =  _context.Functions.AsQueryable();
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x => x.Id.Contains(filter) || 
                                     x.Name!.Contains(filter) || 
                                     x.Url!.Contains(filter));
        }

        var totalRecords = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(f=>new FunctionsVm() {
                Id = f.Id,
                Name = f.Name,
                ParentId = f.ParentId,
                SortOrder = f.SortOrder,
                Url = f.Url,
                Icon = f.Icon,
            }).ToListAsync();
        var pagination = new Pagination<FunctionsVm>
        {
            Items = items,
            TotalRecords = totalRecords,
        };
        return Ok(pagination);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var functions = await _context.Functions.FindAsync(id);
        if (functions == null)
            return NotFound();
        
        var functionsVm = new FunctionsVm()
        {
            Id = functions.Id,
            Name = functions.Name,
            ParentId = functions.ParentId,
            SortOrder = functions.SortOrder,
            Url = functions.Url,
            Icon = functions.Icon,
        };
        return Ok(functionsVm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutFunctions(string id, [FromBody] FunctionsCreateRequest request)
    {
        var functions = await _context.Functions.FindAsync(id);
        if (functions == null)
            return NotFound();

        functions.Name = request.Name;
        functions.ParentId = request.ParentId;
        functions.SortOrder = request.SortOrder;
        functions.Url = request.Url;
        functions.Icon = request.Icon;

        _context.Functions.Update(functions);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return NoContent();
        }

        return BadRequest();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFunctions(string id)
    {
        var functions = await _context.Functions.FindAsync(id);
        if (functions == null)
            return NotFound();

        _context.Functions.Remove(functions);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            var functionsVm = new FunctionsVm()
            {
                Id = functions.Id,
                Name = functions.Name,
                ParentId = functions.ParentId,
                SortOrder = functions.SortOrder,
                Url = functions.Url,
                Icon = functions.Icon,
            };
            return Ok(functions);
        }
        return BadRequest();
    }
    
    [HttpGet("{functionId}/commands")]
    public async Task<IActionResult> GetCommandsInFunction(string functionId)
    {
        var query = from a in _context.Commands
            join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
            from commandInFunction in result1.DefaultIfEmpty()
            join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
            from function in result2.DefaultIfEmpty()
            select new
            {
                a.Id,
                a.Name,
                commandInFunction.FunctionId
            };

        query = query.Where(x => x.FunctionId == functionId);

        var data = await query.Select(x => new CommandVm()
        {
            Id = x.Id,
            Name = x.Name
        }).ToListAsync();

        return Ok(data);
    }
    
    [HttpPost("{functionId}/commands")]
    public async Task<IActionResult> PostCommandToFunction(string functionId, [FromBody]AddCommandToFunctionRequest request)
    {
        var commandInFunction = await _context.CommandInFunctions.FindAsync(request.CommandId, request.FunctionId);
        if (commandInFunction != null)
        {
            return BadRequest("This command has been already added to this function");
        }
        
        var entity = new CommandInFunction()
        {
            CommandId = request.CommandId,
            FunctionId = request.FunctionId,
        };
        _context.CommandInFunctions.Add(entity);
        var result = await _context.SaveChangesAsync();
        
        if (result > 0)
        {
            return CreatedAtAction(nameof(GetById), new { commandId = request.CommandId, functionId = request.FunctionId }, request);
        }
        else
        {
            return BadRequest();
        }
    }
    
    [HttpDelete("{functionId}/commands/{commandId}")]
    public async Task<IActionResult> DeleteCommandToFunction(string functionId, string commandId)
    {
        var commandInFunction = await _context.CommandInFunctions.FindAsync(functionId, commandId);
        if (commandInFunction == null)
        {
            return BadRequest("This command is not existed in function");
        }
        
        var entity = new CommandInFunction()
        {
            CommandId = commandId,
            FunctionId = functionId,
        };
        _context.CommandInFunctions.Remove(entity);
        var result = await _context.SaveChangesAsync();

        if (result > 0)
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    
}