using System.Threading.Tasks;

namespace DeepFreeze.Packages.ScreenSystem.Runtime
{
    public interface IScreenProvider
    {
        bool Initialized { get; }
        
        Task<T> GetScreen<T>(string id) where T : GameScreen;
        
        Task<T> GetPopup<T>(string id) where T : Popup;
    }
}