using System.Threading.Tasks;

namespace ScreenSystem.Runtime
{
    public interface IScreenProvider
    {
        bool Initialized { get; }
        
        Task<T> GetScreen<T>(string id) where T : GameScreen;
        
        Task<T> GetPopup<T>(string id) where T : Popup;
    }
}