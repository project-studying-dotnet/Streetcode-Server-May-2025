using FluentValidation;

namespace Streetcode.BLL.Validator.News.Rules;

public static class NewsRules
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty();

    public static IRuleBuilderOptions<T, string> ValidText<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty();

    public static IRuleBuilderOptions<T, string> ValidUrl<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("Url must be absolute");

    public static IRuleBuilderOptions<T, DateTime> NotInFuture<T>(this IRuleBuilder<T, DateTime> rule) =>
        rule.LessThanOrEqualTo(DateTime.UtcNow);
}