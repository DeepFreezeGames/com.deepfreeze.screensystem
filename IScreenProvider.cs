using System.Threading.Tasks;

namespace DeepFreeze.ScreenSystem
{
    public interface IScreenProvider
    {
        bool Initialized { get; }
        
        Task<T> GetScreen<T>(string id) where T : GameScreen;
        
        Task<T> GetPopup<T>(string id) where T : Popup;
    }
}