using FluentValidation;
using Streetcode.BLL.MediatR.Payment;
using Streetcode.BLL.Validator;

namespace Streetcode.BLL.Validator.Payment.Create;

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(c => c.Payment.Amount).GreaterThan(0);
    }
}