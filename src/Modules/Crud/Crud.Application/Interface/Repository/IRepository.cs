namespace Crud.Application.Interface.Repository
{
    public interface IRepository
    {
        Task<bool> IsActive { get; }
    }
}
