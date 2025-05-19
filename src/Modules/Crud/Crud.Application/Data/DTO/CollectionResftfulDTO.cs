using AutoMapper;
using Crud.Application.Util;
using FluentValidation;
namespace Crud.Application.Data.DTO
{
    public class CollectionCreationResftfulDTO
    {
        public Guid? ParentCollectionId { get; set; }
        public string? Description { get; set; }
        public Guid LabelId { get; set; }


        public class MappingProfile : Profile // this is predecate due to that auto mapper is no longer free to use
        {
            public MappingProfile(HttpClient http)
            {
                //CreateMap<CollectionCreationResftfulDTO, Collection>()
                //    .ForMember(dest => dest.AuthorId, opts => opts.MapFrom(_ => http.GetUserId()));
            }
        }

        public class Validator : AbstractValidator<CollectionCreationResftfulDTO>
        {
            public Validator()
            {
                RuleFor(cr => cr.Description)
                    .Must(input => !string.IsNullOrWhiteSpace(input) &&
                                    !input.IsContainXss())
                        .WithMessage("Attempt to perform XSS injection")
                    .MaximumLength(300)
                        .WithMessage("Description is too long (max 300 chars)");
                RuleFor(cr => cr.LabelId)
                    .NotEmpty().WithMessage("Missing key: LabelId");
            }
        }
    }
}
