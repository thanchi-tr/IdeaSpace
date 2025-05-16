using Crud.Application.Util;
using FluentValidation;

namespace Crud.Application.Data.DTO
{
    public class TagRestfulDTO
    {
        public Guid TagId { get; init; } = Guid.NewGuid();
        public string Description { get; set; }

        public class Validator: AbstractValidator<TagRestfulDTO>
        {
            public Validator()
            {
                RuleFor(cr => cr.Description)
                    .Must(input => !string.IsNullOrWhiteSpace(input) &&
                                    !input.IsContainXss())
                        .WithMessage("Attempt to perform XSS injection")
                    .MaximumLength(300)
                        .WithMessage("Description is too long (max 300 chars)");
            }
        }
    }
}
