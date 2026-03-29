using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// Manages loading and unloading of game assets (prefabs) from the Resources folder.
    ///
    /// In Unity, "Resources" is a special folder. Any asset placed inside it can be loaded
    /// at runtime using Resources.Load(). This is one way to load assets without dragging
    /// them into Inspector fields -- useful for assets needed dynamically.
    ///
    /// A "prefab" is a reusable template for a GameObject. Think of it like a blueprint
    /// that you can create copies ("instances") of during gameplay.
    /// </summary>
    public sealed class ResourcesManager
    {
        // 'static' fields belong to the class itself, not to any particular instance.
        // These strings are file paths (relative to any "Resources" folder in your project).
        private static string _rawCameraPrefabPath = "Prefabs/RawCameraPrefab";
        private static string _playerSlotPath = "Prefabs/PlayerSlot";
        private static string _attributeSlotPath = "Prefabs/AttributeSlot";

        /// <summary>
        /// Loads the PlayerSlot prefab from the Resources folder.
        /// Resources.Load&lt;GameObject&gt; finds and returns a GameObject asset at the given path.
        /// Returns the prefab as a GameObject (not yet placed in the scene -- you must Instantiate it).
        /// </summary>
        public GameObject LoadPlayerSlot()
        {
            return Resources.Load<GameObject>(_playerSlotPath);
        }

        /// <summary>
        /// Loads the AttributeSlot prefab from the Resources folder.
        /// </summary>
        public GameObject LoadAttributeSlot()
        {
            return Resources.Load<GameObject>(_attributeSlotPath);
        }

        /// <summary>
        /// Loads the RawCamera prefab from the Resources folder.
        /// </summary>
        public GameObject LoadRawCameraPrefab()
        {
            return Resources.Load<GameObject>(_rawCameraPrefabPath);
        }

        /// <summary>
        /// Unloads an asset from memory to free up resources.
        /// 'Object' here is UnityEngine.Object (the base class for all Unity objects),
        /// not System.Object. This can unload textures, meshes, etc. that are no longer needed.
        /// </summary>
        public void ReleaseAsset(Object asset)
        {
            Resources.UnloadAsset(asset);
        }

        /// <summary>
        /// Cleanup method. Currently empty but exists as a placeholder for future cleanup logic.
        /// Following the "Dispose" pattern is good practice -- it gives a consistent way
        /// to clean up resources when a manager is no longer needed.
        /// </summary>
        public void Dispose()
        {

        }
    }
}