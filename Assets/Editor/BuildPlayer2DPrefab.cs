using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Level.Unit;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace IdleRelics.EditorTools
{
    public static class BuildPlayer2DPrefab
    {
        private const string SourcePrefabPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player.prefab";
        private const string TargetPrefabPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player_2D.prefab";
        private const string SpriteSheetPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Sprites/Characters/Player_Adventuress_Walk.png";
        private const string IdleFrontSheetPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Sprites/Characters/Player_Adventuress_Idle_Front.png";

        private static readonly string[] DirOrder = { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };
        private const int FramesPerDirection = 6;
        private const float SpriteScale = 2.25f;

        [MenuItem("Tools/IdleRelics/Build Player_2D Prefab")]
        public static void Build()
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
            if (sourcePrefab == null)
            {
                Debug.LogError("Source prefab not found at " + SourcePrefabPath);
                return;
            }

            var sprites = LoadOrderedSprites();
            if (sprites == null)
                return;

            // Idle-front sprites are optional. If the sheet isn't sliced yet, just skip
            // wiring them — runtime falls back to walk-frame-0 when facing forward.
            var idleFrontSprites = LoadIdleFrontSprites();

            // LoadPrefabContents gives us a hidden-scene COPY of the prefab — saving this
            // back via SaveAsPrefabAsset produces a flat, standalone prefab (no nested
            // PrefabInstance / variant relationship to the source). Avoids weird interactions
            // at runtime instantiation that hit on prefab-variant Player.prefab.
            var temp = PrefabUtility.LoadPrefabContents(SourcePrefabPath);
            try
            {
                temp.name = "Player_2D";

                DisableSkinnedMeshRenderers(temp);

                var spriteGO = new GameObject("Sprite");
                spriteGO.transform.SetParent(temp.transform, false);
                spriteGO.transform.localPosition = Vector3.zero;
                // Scale tuned so the sprite reads as similar height to chunky 3D NPCs in
                // the scene (which have ~50% head-to-body ratio and look visually large).
                // Iterate by tweaking SpriteScale below if she's too tall/short.
                spriteGO.transform.localScale = new Vector3(SpriteScale, SpriteScale, SpriteScale);

                var spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprites[0];

                var view = spriteGO.AddComponent<SpriteCharacterView>();
                var navAgent = temp.GetComponent<NavMeshAgent>();
                var rigidbody = temp.GetComponent<Rigidbody>();
                AssignSerializedReferences(view, spriteRenderer, navAgent, rigidbody, sprites, idleFrontSprites);

                EnsureFolder(Path.GetDirectoryName(TargetPrefabPath));
                PrefabUtility.SaveAsPrefabAsset(temp, TargetPrefabPath);
                Debug.Log("Built Player_2D prefab (flat, non-variant) at " + TargetPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(temp);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Sprite[] LoadOrderedSprites()
        {
            var all = AssetDatabase.LoadAllAssetRepresentationsAtPath(SpriteSheetPath);
            var sprites = all.OfType<Sprite>().ToList();
            if (sprites.Count != DirOrder.Length * FramesPerDirection)
            {
                Debug.LogError(string.Format(
                    "Expected {0} sub-sprites at {1}, found {2}. Re-slice via Tools/IdleRelics/Slice Adventuress Walk Sheet first.",
                    DirOrder.Length * FramesPerDirection, SpriteSheetPath, sprites.Count));
                return null;
            }

            var byName = sprites.ToDictionary(s => s.name);
            var ordered = new Sprite[DirOrder.Length * FramesPerDirection];
            for (int d = 0; d < DirOrder.Length; d++)
            {
                for (int f = 0; f < FramesPerDirection; f++)
                {
                    string key = string.Format("Adventuress_Walk_{0}_{1}", DirOrder[d], f);
                    if (!byName.TryGetValue(key, out var sprite))
                    {
                        Debug.LogError("Missing sliced sprite: " + key);
                        return null;
                    }
                    ordered[d * FramesPerDirection + f] = sprite;
                }
            }
            return ordered;
        }

        private static void DisableSkinnedMeshRenderers(GameObject root)
        {
            foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                smr.enabled = false;
        }

        private static void AssignSerializedReferences(
            SpriteCharacterView view,
            SpriteRenderer spriteRenderer,
            NavMeshAgent navAgent,
            Rigidbody rigidbody,
            Sprite[] sprites,
            Sprite[] idleFrontSprites)
        {
            var so = new SerializedObject(view);
            so.FindProperty("_spriteRenderer").objectReferenceValue = spriteRenderer;
            if (navAgent != null) so.FindProperty("_navMeshAgent").objectReferenceValue = navAgent;
            if (rigidbody != null) so.FindProperty("_rigidbody").objectReferenceValue = rigidbody;
            so.FindProperty("_framesPerDirection").intValue = FramesPerDirection;

            var framesProp = so.FindProperty("_walkFrames");
            framesProp.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                framesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

            var idleFrontProp = so.FindProperty("_idleFramesFront");
            int idleLen = idleFrontSprites != null ? idleFrontSprites.Length : 0;
            idleFrontProp.arraySize = idleLen;
            for (int i = 0; i < idleLen; i++)
                idleFrontProp.GetArrayElementAtIndex(i).objectReferenceValue = idleFrontSprites[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite[] LoadIdleFrontSprites()
        {
            var all = AssetDatabase.LoadAllAssetRepresentationsAtPath(IdleFrontSheetPath);
            if (all == null || all.Length == 0)
            {
                Debug.LogWarning("Idle-front sheet not sliced yet at " + IdleFrontSheetPath + " — skipping idle wiring.");
                return null;
            }
            // Sort by sprite name (Adventuress_IdleFront_00, _01, ...) so frame order matches the slicer.
            var sprites = all.OfType<Sprite>().OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray();
            return sprites;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
                return;

            string parent = Path.GetDirectoryName(folderPath);
            string leaf = Path.GetFileName(folderPath);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
