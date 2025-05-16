using Crud.Application.Util;
using FluentValidation;
using Shared.Domain.Models;

namespace Crud.Application.Data.DTO
{
    public class IdeaRestfulDTO
    {
        public Guid IdeaId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SerialisedQuestion { get; set; }
        public bool PrevRevisedResult { get; set; }
        public string SerialisedSampleAnswer { get; set; }
        public Level Level { get; set; }
        public QuestionType QuestionType { get; set; }

        public class Validator : AbstractValidator<IdeaRestfulDTO>
        {
            public Validator() {
                RuleFor(i => i.SerialisedSampleAnswer)
                    .Must(answer => !answer.IsContainXss())
                    .WithMessage("In-valid message.");
                RuleFor(i => i.SerialisedQuestion)
                    .Must(answer => !answer.IsContainXss())
                    .WithMessage("In-valid message.");
            }
        }
    }
}
