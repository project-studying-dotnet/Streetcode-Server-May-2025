using FluentValidation;

namespace Streetcode.BLL.Validator.Streetcode.RelatedTerm.Rules;

public static class RelatedTermRules
{
    public static IRuleBuilderOptions<T, string> ValidWord<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty();

    public static IRuleBuilderOptions<T, int> ValidTermId<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0);
}