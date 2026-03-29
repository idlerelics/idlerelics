using UnityEngine;

namespace Game.Managers
{
    public sealed class ResourcesManager
    {
        private static string _rawCameraPrefabPath = "Prefabs/RawCameraPrefab";
        private static string _playerSlotPath = "Prefabs/PlayerSlot";
        private static string _attributeSlotPath = "Prefabs/AttributeSlot";

        public GameObject LoadPlayerSlot()
        {
            return Resources.Load<GameObject>(_playerSlotPath);
        }

        public GameObject LoadAttributeSlot()
        {
            return Resources.Load<GameObject>(_attributeSlotPath);
        }

        public GameObject LoadRawCameraPrefab()
        {
            return Resources.Load<GameObject>(_rawCameraPrefabPath);
        }

        public void ReleaseAsset(Object asset)
        {
            Resources.UnloadAsset(asset);
        }

        public void Dispose()
        {
            
        }
    }
}