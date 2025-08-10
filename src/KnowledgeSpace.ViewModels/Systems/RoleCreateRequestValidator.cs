using FluentValidation;

namespace KnowledgeSpace.ViewModels.Systems;

public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
{
    public RoleCreateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role id is required.")
            .MinimumLength(10).WithMessage("Role id must be at least 10 characters long.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MinimumLength(10).WithMessage("Role name must be at least 10 characters long.");
    }
    
}