using FluentValidation;

namespace Streetcode.BLL.Validator;

public static class CommonRules
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
        .MaximumLength(100)
        .WithMessage("Title cannot be more than 100 characters");

    public static IRuleBuilderOptions<T, string> ValidText<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty();

    public static IRuleBuilderOptions<T, string> ValidUrl<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("Url must be absolute");

    public static IRuleBuilderOptions<T, string?> ValidUrlOptional<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(u => u is null || Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("URL must be absolute");

    public static IRuleBuilderOptions<T, DateTime> NotInFuture<T>(this IRuleBuilder<T, DateTime> rule) =>
        rule.LessThanOrEqualTo(DateTime.UtcNow);

    public static IRuleBuilderOptions<T, int> ValidId<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0);

    public static IRuleBuilderOptions<T, string?> ValidImageDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(200)
            .WithMessage("Image description cannot be more than 200 characters");
}