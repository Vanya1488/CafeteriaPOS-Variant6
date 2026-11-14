using FluentValidation;

internal sealed class PriceUpdateValidator : AbstractValidator<PriceUpdateRequest>
{
    public PriceUpdateValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0m).WithMessage("Цена должна быть положительной.")
            .LessThanOrEqualTo(100000m).WithMessage("Цена превышает допустимый предел.");
    }
}
