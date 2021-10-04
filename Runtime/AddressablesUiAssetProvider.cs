using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace ScreenSystem.Runtime
{
    public class AddressablesUiAssetProvider : IScreenProvider
    {
        public bool Initialized { get; private set;}

        public AddressablesUiAssetProvider()
        {
            Initialized = true;
        }
        
        public async Task<T> GetScreen<T>(string id) where T : GameScreen
        {
            var result = await Addressables.InstantiateAsync(id).Task;
            return result.GetComponent<T>();
        }

        public async Task<T> GetPopup<T>(string id) where T : Popup
        {
            var result = await Addressables.InstantiateAsync(id).Task;
            return result.GetComponent<T>();
        }
    }
}