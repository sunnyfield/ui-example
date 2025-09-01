using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Game.Scripts.UISystem
{
    public struct AssetHandle
    {
        public uint ID;
        
        public static readonly AssetHandle s_invalid = new() { ID = 0 };
        
        public AssetHandle(uint id)
        {
            ID = id;
        }
        
        public bool IsValid => ID != 0;
        
        public static implicit operator uint(AssetHandle handle) => handle.ID;
        public static implicit operator AssetHandle(uint id) => new(id);
    }
    
    public static class AddressableAssetManager
    {
        public static readonly AssetHandle s_coinIcon = new(1);
        public static readonly AssetHandle s_adsIcon = new(2);
        
        private static readonly string[] s_addressableKeys = { "coin", "ads-icon" };
        private static Texture2D[] s_loadedAssets = new Texture2D[2];
        private static AsyncOperationHandle<Texture2D>[] s_loadHandles = new AsyncOperationHandle<Texture2D>[2];
        
        public static async Task PreloadAssets()
        {
            var tasks = new Task<Texture2D>[s_addressableKeys.Length];
            
            for (int i = 0; i < s_addressableKeys.Length; i++)
            {
                s_loadHandles[i] = Addressables.LoadAssetAsync<Texture2D>(s_addressableKeys[i]);
                tasks[i] = s_loadHandles[i].Task;
            }
            
            var results = await Task.WhenAll(tasks);
            
            for (int i = 0; i < results.Length; i++)
            {
                s_loadedAssets[i] = results[i];
            }
        }
        
        public static Texture2D GetAsset(AssetHandle handle)
        {
            return s_loadedAssets[handle.ID - 1];
        }
        
        public static void Cleanup()
        {
            for (int i = 0; i < s_loadHandles.Length; i++)
            {
                if (s_loadHandles[i].IsValid())
                {
                    Addressables.Release(s_loadHandles[i]);
                }
            }
        }
    }
}