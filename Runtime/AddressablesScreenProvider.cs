#if ADDRESSABLES
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace ScreenSystem.Runtime
{
    public class AddressablesScreenProvider : IScreenProvider
    {
        public bool Initialized { get; private set;}

        public AddressablesScreenProvider()
        {
            Addressables.InitializeAsync();
            
        }
        
        public async Task<T> GetScreen<T>(string id) where T : GameScreen
        {
            return await Addressables.LoadAssetAsync<T>(id).Task;
        }

        public async Task<T> GetPopup<T>(string id) where T : Popup
        {
            return await Addressables.LoadAssetAsync<T>(id).Task;
        }
    }
}
#endif