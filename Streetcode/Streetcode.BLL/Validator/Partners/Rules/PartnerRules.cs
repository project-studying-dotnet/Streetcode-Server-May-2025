using FluentValidation;

namespace Streetcode.BLL.Validator.Partners.Rules;

public static class PartnerRules
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty();

    public static IRuleBuilderOptions<T, string?> ValidUrlOptional<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(u => u is null || Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("URL must be absolute");
}