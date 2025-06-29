using FluentValidation;

namespace Application.Dtos.Requests.Shared;

public record PageSettings(int Skip, int Take);

public class PageSettingsValidator : AbstractValidator<PageSettings>
{
    public PageSettingsValidator()
    {
        RuleFor(x => x.Skip).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Take).GreaterThan(0);
    }
}