using FluentValidation;

namespace Application.Dtos.Requests.Shared;

public record PageSettings(ushort PageNumber, ushort PageSize);

public class PageSettingsValidator : AbstractValidator<PageSettings>
{
    public PageSettingsValidator()
    {
        RuleFor(x => x.PageNumber).Must(n => n > 0);
        RuleFor(x => x.PageSize).Must(s => s > 0);
    }
}