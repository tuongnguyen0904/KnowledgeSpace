using FluentValidation;

namespace KnowledgeSpace.ViewModels.Systems;

public class FunctionsCreateRequestValidator : AbstractValidator<FunctionsCreateRequest>
{
    public FunctionsCreateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.")
            .MaximumLength(50).WithMessage("Id must not exceed 50 characters.");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.SortOrder)
            .NotEmpty().WithMessage("Sort order is required.");
        
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required.")
            .MaximumLength(200).WithMessage("Url must not exceed 200 characters.");
        
        
        RuleFor(x => x.ParentId)
            .MaximumLength(50)
            .When(x=>!string.IsNullOrEmpty(x.ParentId))
            .WithMessage("ParentId must not exceed 50 characters.");
    
        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("Icon is required.")
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters.");;
    }
    
}