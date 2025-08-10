using FluentValidation;

namespace KnowledgeSpace.ViewModels.Systems;

public class UserCreateRequestValidator :  AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").WithMessage("Email format is invalid.");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required.");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(50).WithMessage("FirstName must not exceed 50 characters.");
    
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(50).WithMessage("LastName must not exceed 50 characters.");
    }
    
}