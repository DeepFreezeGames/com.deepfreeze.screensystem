using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    public class ResourceScreenProvider : IScreenProvider
    {
        public bool Initialized { get; private set; } = true;
        
        public async Task<T> GetScreen<T>(string id) where T : GameScreen
        {
            var asset = await Resources.LoadAsync<T>(id);
            return (T)asset;
        }

        public async Task<T> GetPopup<T>(string id) where T : Popup
        {
            var asset = await Resources.LoadAsync<T>(id);
            return (T)asset;
        }
    }
}