namespace Crud.Application.Interface.Util
{
    public interface IMap
    {
        DomainModel Map<ModelDTO, DomainModel>(ModelDTO dto)
            where ModelDTO : class
            where DomainModel : new();
    }
}
