using Game.Config;
using UnityEditor;
using UnityEngine;

namespace IdleRelics.EditorTools
{
    public static class WireAdventuressToPlayer2D
    {
        private const string AdventuressConfigPath = "Assets/GorodiskiGames/PerfectHotel/Resources/PlayerConfigs/7Adventuress.asset";
        private const string Player2DPrefabPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player_2D.prefab";
        private const string GameModelPrefsKey = "model";
        private const int AdventuressPlayerIndex = 7;

        private const string PlayerAdventuressPrefabPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player_Adventuress.prefab";

        [MenuItem("Tools/IdleRelics/Revert Adventuress -> Player_Adventuress (3D)")]
        public static void RevertAdventuressToOriginal()
        {
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(AdventuressConfigPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerAdventuressPrefabPath);
            if (config == null) { Debug.LogError("Adventuress config not found"); return; }
            if (prefab == null) { Debug.LogError("Player_Adventuress.prefab not found"); return; }
            var so = new SerializedObject(config);
            so.FindProperty("Prefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("Adventuress config reverted to Player_Adventuress.prefab (3D).");
        }

        [MenuItem("Tools/IdleRelics/Wire Adventuress -> Player_2D")]
        public static void WireConfigOnly()
        {
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(AdventuressConfigPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Player2DPrefabPath);

            if (config == null) { Debug.LogError("Adventuress config not found at " + AdventuressConfigPath); return; }
            if (prefab == null) { Debug.LogError("Player_2D prefab not found at " + Player2DPrefabPath); return; }

            var so = new SerializedObject(config);
            so.FindProperty("Prefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("Adventuress config now points at Player_2D.prefab.");
        }

        [MenuItem("Tools/IdleRelics/Force Player7 (Adventuress) Active")]
        public static void ForcePlayer7Active()
        {
            string json = PlayerPrefs.GetString(GameModelPrefsKey, "");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("No saved 'model' in PlayerPrefs yet. The game will create one on first launch — re-run after entering Play mode once.");
                return;
            }

            var parsed = JsonUtility.FromJson<MinimalModel>(json);
            if (parsed == null)
            {
                Debug.LogError("Failed to parse saved model JSON.");
                return;
            }

            int prev = parsed.Player;
            parsed.Player = AdventuressPlayerIndex;
            string updated = JsonUtility.ToJson(parsed);

            // Reload the full original to preserve any fields MinimalModel doesn't know about.
            var raw = SimpleJsonReplace(json, "\"Player\":" + prev, "\"Player\":" + AdventuressPlayerIndex);
            PlayerPrefs.SetString(GameModelPrefsKey, raw);
            PlayerPrefs.Save();
            Debug.Log(string.Format("PlayerPrefs 'model.Player' set: {0} -> {1}.", prev, AdventuressPlayerIndex));
        }

        // Robust enough for an int field; falls back to the JsonUtility-roundtripped version
        // if the literal substring isn't found.
        private static string SimpleJsonReplace(string json, string oldChunk, string newChunk)
        {
            if (json.Contains(oldChunk)) return json.Replace(oldChunk, newChunk);
            var minimal = JsonUtility.FromJson<MinimalModel>(json);
            minimal.Player = AdventuressPlayerIndex;
            return JsonUtility.ToJson(minimal);
        }

        [System.Serializable]
        private class MinimalModel
        {
            public int Player;
        }
    }
}
