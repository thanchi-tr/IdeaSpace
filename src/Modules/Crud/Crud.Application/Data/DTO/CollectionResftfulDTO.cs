using AutoMapper;
using FluentValidation;
using Shared.Domain.Models;
namespace Crud.Application.Data.DTO
{
    public class CollectionCreationResftfulDTO
    {
        public Guid? ParentCollectionId { get; set; }
        public string Description { get; set; }
        public Guid LabelId { get; set; }


        public class MappingProfile: Profile
        {
            public MappingProfile(HttpClient http)
            {
                //CreateMap<CollectionCreationResftfulDTO, Collection>()
                //    .ForMember(dest => dest.AuthorId, opts => opts.MapFrom(_ => http.GetUserId()));
            }
        }

        public class Validator: AbstractValidator<CollectionCreationResftfulDTO>
        {
            public Validator()
            {
                RuleFor(cr => cr.Description)
                    .NotEmpty().WithMessage("Please provide a short collection description!");
            }
        }
    }
}
