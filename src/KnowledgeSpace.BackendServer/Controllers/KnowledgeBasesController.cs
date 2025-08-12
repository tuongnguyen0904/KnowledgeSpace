using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Contents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers;

public class KnowledgeBasesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public KnowledgeBasesController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> PostFunctions([FromBody]KnowledgeBaseCreateRequest request)
    {
        var knowledgeBase = new KnowledgeBase
        {
            CategoryId = request.CategoryId,
            Title  = request.Title,
            SeoAlias = request.SeoAlias,
            Description = request.Description,
            Environment = request.Environment,
            Problem = request.Problem,
            StepToReproduce = request.StepToReproduce,
            ErrorMessage = request.ErrorMessage,
            Workaround = request.Workaround,
            Note = request.Note,
            Labels  = request.Labels,
        };
        _context.KnowledgeBases.AddAsync(knowledgeBase);
        var result = await _context.SaveChangesAsync();
        
        if (result > 0)
        {
            return CreatedAtAction(nameof(GetById), new { id = knowledgeBase.Id }, request);
        }
        else
        {
            return BadRequest();
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetKnowledgeBases()
    {
        var knowledgeBases = _context.KnowledgeBases;

        var knowledgeBasesVm = await knowledgeBases.Select(k => new KnowledgeBaseQuickVm()
        { 
            Id = k.Id,
            CategoryId = k.CategoryId,
            Title  = k.Title,
            SeoAlias = k.SeoAlias,
            Description = k.Description,
        }).ToListAsync();
        
        return Ok(knowledgeBasesVm);
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetAllFunctionsPaging(string filter, int pageIndex, int pageSize)
    {
        var query =  _context.KnowledgeBases.AsQueryable();
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x => x.Title.Contains(filter));
        }

        var totalRecords = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(k=>new KnowledgeBaseQuickVm() {
                Id = k.Id,
                CategoryId = k.CategoryId,
                Title  = k.Title,
                SeoAlias = k.SeoAlias,
                Description = k.Description,
            }).ToListAsync();
        var pagination = new Pagination<KnowledgeBaseQuickVm>()
        {
            Items = items,
            TotalRecords = totalRecords,
        };
        return Ok(pagination);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
        if (knowledgeBase == null)
            return NotFound();
        
        var functionsVm = new KnowledgeBaseVm()
        {
            Id = knowledgeBase.Id,
            CategoryId = knowledgeBase.CategoryId,
            Title = knowledgeBase.Title,
            SeoAlias = knowledgeBase.SeoAlias,
            Description = knowledgeBase.Description,
            Environment = knowledgeBase.Environment,
            Problem = knowledgeBase.Problem,
            StepToReproduce = knowledgeBase.StepToReproduce,
            ErrorMessage = knowledgeBase.ErrorMessage,
            Workaround = knowledgeBase.Workaround,
            Note = knowledgeBase.Note,
            CreateDate  = knowledgeBase.CreateDate,
            LastModifiedDate = knowledgeBase.LastModifiedDate,
            NumberOfComments = knowledgeBase.NumberOfComments,
            NumberOfVotes = knowledgeBase.NumberOfVotes,
            NumberOfReports = knowledgeBase.NumberOfReports,
            
        };
        return Ok(functionsVm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutKnowledgeBase(int id, [FromBody] KnowledgeBaseCreateRequest request)
    {
        var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
        if (knowledgeBase == null)
            return NotFound();

        knowledgeBase.CategoryId = request.CategoryId;
        knowledgeBase.Title = request.Title;
        knowledgeBase.SeoAlias = request.SeoAlias;
        knowledgeBase.Description = request.Description;
        knowledgeBase.Environment = request.Environment;
        knowledgeBase.Problem = request.Problem;
        knowledgeBase.StepToReproduce = request.StepToReproduce;
        knowledgeBase.ErrorMessage = request.ErrorMessage;
        knowledgeBase.Workaround = request.Workaround;
        knowledgeBase.Note = request.Note;
        knowledgeBase.Labels = request.Labels;

        _context.KnowledgeBases.Update(knowledgeBase);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return NoContent();
        }

        return BadRequest();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFunctions(int id)
    {
        var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
        if (knowledgeBase == null)
            return NotFound();

        _context.KnowledgeBases.Remove(knowledgeBase);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            var knowledgeBaseVm = new KnowledgeBaseVm()
            {
                Id = knowledgeBase.Id,
                CategoryId = knowledgeBase.CategoryId,
                Title = knowledgeBase.Title,
                SeoAlias = knowledgeBase.SeoAlias,
                Description = knowledgeBase.Description,
                Environment = knowledgeBase.Environment,
                Problem = knowledgeBase.Problem,
                StepToReproduce = knowledgeBase.StepToReproduce,
                ErrorMessage = knowledgeBase.ErrorMessage,
                Workaround = knowledgeBase.Workaround,
                Note = knowledgeBase.Note,
                CreateDate  = knowledgeBase.CreateDate,
                LastModifiedDate = knowledgeBase.LastModifiedDate,
                NumberOfComments = knowledgeBase.NumberOfComments,
                NumberOfVotes = knowledgeBase.NumberOfVotes,
                NumberOfReports = knowledgeBase.NumberOfReports,
            };
            return Ok(knowledgeBaseVm);
        }
        return BadRequest();
    }
    
}