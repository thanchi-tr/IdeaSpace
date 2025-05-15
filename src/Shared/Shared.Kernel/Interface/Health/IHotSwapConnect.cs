namespace Shared.Kernel.Interface.Health
{
    /// <summary>
    /// Interface that enable hot swap the connection path (port)
    /// since modern application they will likely have a dynamic port mapping
    /// </summary>
    public interface IHotSwapConnectionPath
    {
        void HotSwapConnection(string newConnectionPath);
    }
}
