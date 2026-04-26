using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Game.Level.Player;

// One-shot setup for the Mixamo-based player rig.
// Creates:
//   - PlayerMixamoAC.controller — AnimatorController with Walk + Idle states (placeholder Idle)
//   - Player_Adventuress.prefab — full per-character prefab with Animator/NavMeshAgent/Rigidbody/BoxCollider/PlayerView
//
// Run via: Tools > Setup PlayerMixamo (Adventuress prefab)
// Re-runnable; deletes existing assets at target paths and recreates them.
public static class PlayerMixamoSetup
{
    private const string FbxPath = "Assets/_Tests/PlayerAdventuressMixamo/PlayerAdventuressMixamo.fbx";
    private const string IdleFbxPath = "Assets/_Tests/PlayerAdventuressMixamo/AdvanturessIdle.fbx";
    private const string CarryIdleFbxPath = "Assets/_Tests/PlayerAdventuressMixamo/Box Idle.fbx";
    private const string CarryWalkFbxPath = "Assets/_Tests/PlayerAdventuressMixamo/Box Walk.fbx";
    private const string ControllerPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Animation/PlayerMixamoAC.controller";
    private const string CarryMaskPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Animation/PlayerMixamoCarryMask.mask";
    private const string PrefabPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player_Adventuress.prefab";
    private const string MaterialPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Materials/PlayerAdventuressMaterial.mat";
    private const string PlayerConfigPath = "Assets/GorodiskiGames/PerfectHotel/Resources/PlayerConfigs/7Adventuress.asset";
    private const string LegacyPlayerPrefabPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player.prefab";

    [MenuItem("Tools/Setup PlayerMixamo (Adventuress prefab)")]
    public static void Run()
    {
        // 1. Resolve dependencies
        var fbxRoot = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxRoot == null) { Debug.LogError($"Setup: missing FBX at {FbxPath}"); return; }

        AnimationClip walkClip = null;
        Avatar avatar = null;
        var subs = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
        foreach (var a in subs)
        {
            if (a is AnimationClip c && c.name == "Walk") walkClip = c;
            if (a is Avatar av && av.isHuman) avatar = av;
        }
        if (walkClip == null) { Debug.LogError($"Setup: 'Walk' clip not found in FBX"); return; }
        if (avatar == null)   { Debug.LogError($"Setup: humanoid avatar not found — re-import FBX as Humanoid"); return; }

        // Optional: idle clip from a separate Mixamo "Without Skin" FBX. Falls back to walk clip
        // if the idle FBX isn't present yet — that way the controller still works.
        AnimationClip idleClip = walkClip;
        if (System.IO.File.Exists(IdleFbxPath))
        {
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(IdleFbxPath))
                if (a is AnimationClip c && c.name == "Idle") { idleClip = c; break; }
        }

        // Optional: carry-overlay clips. When the player is carrying inventory, layer 1 weight
        // is set to 1.0 (see UnitView.SetLayerWeight) and these clips take over.
        AnimationClip carryIdleClip = null;
        AnimationClip carryWalkClip = null;
        if (System.IO.File.Exists(CarryIdleFbxPath))
        {
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(CarryIdleFbxPath))
                if (a is AnimationClip c && c.name == "BoxIdle") { carryIdleClip = c; break; }
        }
        if (System.IO.File.Exists(CarryWalkFbxPath))
        {
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(CarryWalkFbxPath))
                if (a is AnimationClip c && c.name == "BoxWalk") { carryWalkClip = c; break; }
        }
        Debug.Log($"Setup: clips idle='{idleClip.name}' walk='{walkClip.name}' carryIdle='{(carryIdleClip != null ? carryIdleClip.name : "<missing>")}' carryWalk='{(carryWalkClip != null ? carryWalkClip.name : "<missing>")}'");

        // 2. Build the AnimatorController.
        // The game uses Animator.PlayInFixedTime(StringToHash(AnimationType.ToString()), 0, t)
        // to drive animation directly by state name (see UnitView.PlayAnimation). Female
        // characters call IdleFemale (not Idle), so we MUST provide both 'Idle' and
        // 'IdleFemale' state names. Walk is gender-neutral so 'Walk' alone is enough.
        if (File.Exists(ControllerPath)) AssetDatabase.DeleteAsset(ControllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        var rsm = controller.layers[0].stateMachine;

        // Idle states (both genders) — use the Idle clip if available, otherwise fall back to Walk
        var idleState = rsm.AddState("Idle");
        idleState.motion = idleClip;
        idleState.speed = 1f;

        var idleFemaleState = rsm.AddState("IdleFemale");
        idleFemaleState.motion = idleClip;
        idleFemaleState.speed = 1f;

        // Walk state — used by both genders (PlayerView.Walk hardcodes AnimationType.Walk).
        var walkState = rsm.AddState("Walk");
        walkState.motion = walkClip;
        walkState.speed = 1f;

        // WalkFemale — defensively present so any external code that hashes "WalkFemale" finds a state.
        var walkFemaleState = rsm.AddState("WalkFemale");
        walkFemaleState.motion = walkClip;
        walkFemaleState.speed = 1f;

        rsm.defaultState = idleFemaleState; // Female default; harmless for males since PlayInFixedTime overrides

        // Add a synced "Carry" layer (layer 1). The legacy game code (UnitView.SetLayerWeight)
        // sets layer 1 weight to 1 when carrying inventory and 0 when not. With syncedLayerIndex=0,
        // layer 1 mirrors layer 0's state machine (same state names, same transitions); we only
        // override the *motion* per state. So when layer 0 plays "Walk", layer 1 also plays its
        // "Walk" state — but with the BoxWalk clip — and at weight 1.0 it fully overrides layer 0.
        if (carryIdleClip != null && carryWalkClip != null)
        {
            // Build an upper-body-only AvatarMask. Mixamo's Box Idle/Walk are full-body clips
            // authored on Mixamo's default character; their lower-body animation retargets badly
            // onto the Adventuress avatar (we saw a twisted left leg in test). Masking out the
            // legs and root means the carry layer only drives the upper body — arms/torso hold
            // the box pose while the legs continue the base layer's walk/idle. This is the
            // standard pattern (also how the legacy NPC carry setup works on UnitAC).
            if (System.IO.File.Exists(CarryMaskPath)) AssetDatabase.DeleteAsset(CarryMaskPath);
            var mask = new AvatarMask();
            // Body parts: 0=Root, 1=Body, 2=Head, 3=LeftLeg, 4=RightLeg, 5=LeftArm,
            //             6=RightArm, 7=LeftFingers, 8=RightFingers, 9=LeftFootIK,
            //             10=RightFootIK, 11=LeftHandIK, 12=RightHandIK
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Root, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFootIK, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFootIK, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);
            AssetDatabase.CreateAsset(mask, CarryMaskPath);
            AssetDatabase.SaveAssets();

            controller.AddLayer("Carry");
            var layers = controller.layers;
            layers[1].syncedLayerIndex = 0;
            layers[1].defaultWeight = 0f; // Off by default; UnitView.SetLayerWeight flips it at runtime.
            layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
            layers[1].iKPass = false;
            layers[1].avatarMask = mask;
            controller.layers = layers;

            // SetOverrideMotion(state, motion) is the synced-layer API for replacing motions
            // without redefining the state machine. Available via AnimatorController extension.
            controller.SetStateEffectiveMotion(idleState,        carryIdleClip, 1);
            controller.SetStateEffectiveMotion(idleFemaleState,  carryIdleClip, 1);
            controller.SetStateEffectiveMotion(walkState,        carryWalkClip, 1);
            controller.SetStateEffectiveMotion(walkFemaleState,  carryWalkClip, 1);
            Debug.Log($"Setup: added synced Carry layer (Idle/IdleFemale -> {carryIdleClip.name}, Walk/WalkFemale -> {carryWalkClip.name})");
        }
        else
        {
            Debug.LogWarning("Setup: carry clips missing — skipping Carry layer (carry overlay won't activate when picking up items).");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log($"Setup: created controller at {ControllerPath} (Idle/IdleFemale -> {idleClip.name}, Walk/WalkFemale -> {walkClip.name})");

        // 3. Build the per-character prefab from the FBX
        // Instantiate FBX in scene, layer in Animator/NavMeshAgent/Rigidbody/BoxCollider/PlayerView, save as prefab
        var go = (GameObject)PrefabUtility.InstantiatePrefab(fbxRoot);
        go.name = "Player_Adventuress";
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        // SkinnedMeshRenderer: assign the per-character material + defensive culling settings
        var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (smr != null)
        {
            if (mat != null) smr.sharedMaterial = mat;
            // Force bounds recomputation per frame — Humanoid retargeting can move bones
            // outside the bind-pose bounds, causing the mesh to be wrongly culled.
            smr.updateWhenOffscreen = true;
        }

        // Animator (FBX root already has one from import; configure controller + avatar)
        var animator = go.GetComponent<Animator>();
        if (animator == null) animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.avatar = avatar;
        animator.applyRootMotion = false;
        // Always animate, even when off-screen — defensive against the SMR culling issue.
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // NavMeshAgent (match Player.prefab values)
        var nav = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav == null) nav = go.AddComponent<UnityEngine.AI.NavMeshAgent>();
        nav.speed = 3.5f;
        nav.radius = 0.5f;
        nav.height = 2f;

        // Rigidbody (match Player.prefab: kinematic, gravity on, mass 1)
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // BoxCollider (rough body capsule equivalent)
        var col = go.GetComponent<BoxCollider>();
        if (col == null) col = go.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 1.0f, 0f);
        col.size = new Vector3(0.6f, 2f, 0.6f);

        // PlayerView component (the gameplay's expected MonoBehaviour)
        var view = go.GetComponent<PlayerView>();
        if (view == null) view = go.AddComponent<PlayerView>();

        // PlayerView/UnitView/UnitStaffView fields are private+SerializeField — set via SerializedObject.
        // The Inspector exposes these on the PlayerView component (since it inherits from UnitView).
        var so = new SerializedObject(view);

        // PlayerView._body
        var bodyProp = so.FindProperty("_body");
        if (bodyProp != null && smr != null) bodyProp.objectReferenceValue = smr;

        // PlayerView._aimTransform — head bone is a sensible aim point
        var aimProp = so.FindProperty("_aimTransform");
        if (aimProp != null)
        {
            var aimT = FindChildRecursive(go.transform, "mixamorig:Head") ?? go.transform;
            aimProp.objectReferenceValue = aimT;
        }

        // UnitView._animator / _navMeshAgent / _sex / _localTransform
        var animProp = so.FindProperty("_animator");
        if (animProp != null) animProp.objectReferenceValue = animator;
        var navProp = so.FindProperty("_navMeshAgent");
        if (navProp != null) navProp.objectReferenceValue = nav;
        var sexProp = so.FindProperty("_sex");
        if (sexProp != null) sexProp.intValue = 1; // 1 = Female (UnitSexType.Female)
        var localTProp = so.FindProperty("_localTransform");
        if (localTProp != null) localTProp.objectReferenceValue = go.transform;

        // UnitStaffView._inventoryHolder — placeholder at chest height; Fabian can refine in Inspector
        var invHolderProp = so.FindProperty("_inventoryHolder");
        if (invHolderProp != null)
        {
            var holder = FindChildRecursive(go.transform, "InventoryHolder");
            if (holder == null)
            {
                var holderGO = new GameObject("InventoryHolder");
                holderGO.transform.SetParent(go.transform, false);
                holderGO.transform.localPosition = new Vector3(0f, 1.4f, 0.3f);
                holder = holderGO.transform;
            }
            invHolderProp.objectReferenceValue = holder;
        }

        so.ApplyModifiedProperties();

        // 4. Save as prefab
        if (File.Exists(PrefabPath)) AssetDatabase.DeleteAsset(PrefabPath);
        var dir = Path.GetDirectoryName(PrefabPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath, out bool success);
        Object.DestroyImmediate(go);

        if (success)
            Debug.Log($"Setup: created prefab at {PrefabPath}");
        else
            Debug.LogError("Setup: SaveAsPrefabAsset failed");

        // 5. Re-wire 7Adventuress.asset to point at the freshly-created prefab.
        // CRITICAL: re-creating the prefab gives it a NEW internal fileID for the root
        // GameObject. Any existing reference to the old fileID resolves to <null> at
        // runtime even though the YAML "looks fine" (guid+fileID combo is stale).
        // Re-assigning the reference here repoints to the current fileID.
        var configAsset = AssetDatabase.LoadAssetAtPath<Game.Config.PlayerConfig>(PlayerConfigPath);
        var newPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (configAsset != null && newPrefabAsset != null)
        {
            var soCfg = new SerializedObject(configAsset);
            var prefabProp = soCfg.FindProperty("Prefab");
            if (prefabProp != null)
            {
                prefabProp.objectReferenceValue = newPrefabAsset;
                soCfg.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log($"Setup: re-wired {PlayerConfigPath} Prefab -> {newPrefabAsset.name}");
            }
        }

        Debug.Log("Setup: DONE.");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindChildRecursive(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
