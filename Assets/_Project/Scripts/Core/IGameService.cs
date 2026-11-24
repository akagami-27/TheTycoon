namespace TheTycoon.Core
{
    /// <summary>
    /// Marker interface for services registered with the simulation lifecycle.
    /// </summary>
    public interface IGameService
    {
        void Initialize();
        void Shutdown();
    }
}
