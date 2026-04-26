using UnityEditor;
using UnityEngine;

// Re-runnable: configures any Mixamo "Without Skin" FBX in the Adventuress test folder
// as Humanoid + CreateFromThisModel + clip looping. Idempotent.
public static class ConfigureMixamoClips
{
    private const string Folder = "Assets/_Tests/PlayerAdventuressMixamo";

    private static readonly (string fileName, string clipName)[] Targets =
    {
        ("AdvanturessIdle.fbx", "Idle"),
        ("Box Idle.fbx",        "BoxIdle"),
        ("Box Walk.fbx",        "BoxWalk"),
    };

    [MenuItem("Tools/Configure Mixamo Clips")]
    public static void Run()
    {
        foreach (var (fileName, clipName) in Targets)
        {
            var path = $"{Folder}/{fileName}";
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"ConfigureMixamoClips: skipping (importer not found) {path}");
                continue;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            var defaults = importer.defaultClipAnimations;
            if (defaults != null && defaults.Length > 0)
            {
                for (int i = 0; i < defaults.Length; i++)
                {
                    defaults[i].name = clipName;
                    defaults[i].loopTime = true;
                    defaults[i].loop = true;
                }
                importer.clipAnimations = defaults;
            }

            importer.SaveAndReimport();

            // Verify
            var subs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in subs)
            {
                if (a is AnimationClip clip)
                    Debug.Log($"[ConfigureMixamoClips] {fileName} -> '{clip.name}' length={clip.length:F2}s loop={clip.isLooping}");
            }
        }
        Debug.Log("ConfigureMixamoClips: DONE");
    }
}
