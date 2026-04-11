// Throwaway diagnostic — compares the PlayerArchaeologist mesh against PlayerA
// to find why the body is invisible at runtime. DELETE after running once.
using UnityEditor;
using UnityEngine;

public static class PlayerArchaeologistMeshDiagnostic
{
    [MenuItem("Tools/Probe/Diagnose Archaeologist Mesh")]
    public static void Run()
    {
        Dump("Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Models/Units/PlayerA.fbx");
        Dump("Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Models/Units/PlayerArchaeologist.fbx");

        // Also dump the prefab's SkinnedMeshRenderer bones[] array length
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Units/Player.prefab");
        if (prefab != null)
        {
            var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr != null)
            {
                Debug.Log("[Diag] Player.prefab SMR bones[] length = " + smr.bones.Length);
                for (int i = 0; i < smr.bones.Length; i++)
                {
                    var b = smr.bones[i];
                    Debug.Log("[Diag] prefab bone " + i + " = " + (b == null ? "<null>" : b.name));
                }
                Debug.Log("[Diag] prefab SMR sharedMesh = " + (smr.sharedMesh == null ? "<null>" : smr.sharedMesh.name) + " materials=" + smr.sharedMaterials.Length);
            }
        }
    }

    static void Dump(string path)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        Mesh mesh = null;
        foreach (var a in assets)
        {
            if (a is Mesh m) { mesh = m; break; }
        }
        if (mesh == null) { Debug.Log("[Diag] no Mesh in " + path); return; }

        // Print mesh sub-asset fileID so we can wire it into PlayerConfig YAML if needed
        long localId; string guidStr;
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out guidStr, out localId))
            Debug.Log("[Diag] mesh fileID=" + localId + " guid=" + guidStr);

        Debug.Log("[Diag] === " + path + " ===");
        var bnd = mesh.bounds;
        Debug.Log("[Diag] mesh.name=" + mesh.name + " vertexCount=" + mesh.vertexCount + " subMeshCount=" + mesh.subMeshCount + " bindposes=" + mesh.bindposes.Length);
        Debug.Log("[Diag] bounds.center=(" + bnd.center.x.ToString("F4") + "," + bnd.center.y.ToString("F4") + "," + bnd.center.z.ToString("F4") + ") extents=(" + bnd.extents.x.ToString("F4") + "," + bnd.extents.y.ToString("F4") + "," + bnd.extents.z.ToString("F4") + ")");
        var verts = mesh.vertices;
        if (verts.Length > 0)
        {
            float minX=float.MaxValue,minY=float.MaxValue,minZ=float.MaxValue,maxX=float.MinValue,maxY=float.MinValue,maxZ=float.MinValue;
            for (int i = 0; i < verts.Length; i++)
            {
                var v = verts[i];
                if (v.x < minX) minX = v.x; if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y; if (v.y > maxY) maxY = v.y;
                if (v.z < minZ) minZ = v.z; if (v.z > maxZ) maxZ = v.z;
            }
            Debug.Log("[Diag] vertex range x=[" + minX.ToString("F3") + "," + maxX.ToString("F3") + "] y=[" + minY.ToString("F3") + "," + maxY.ToString("F3") + "] z=[" + minZ.ToString("F3") + "," + maxZ.ToString("F3") + "]");
        }

        // Find max bone index used by any vertex
        var weights = mesh.boneWeights;
        int maxIdx = -1;
        int verticesWithBone22Plus = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            var w = weights[i];
            if (w.weight0 > 0 && w.boneIndex0 > maxIdx) maxIdx = w.boneIndex0;
            if (w.weight1 > 0 && w.boneIndex1 > maxIdx) maxIdx = w.boneIndex1;
            if (w.weight2 > 0 && w.boneIndex2 > maxIdx) maxIdx = w.boneIndex2;
            if (w.weight3 > 0 && w.boneIndex3 > maxIdx) maxIdx = w.boneIndex3;

            if ((w.weight0 > 0 && w.boneIndex0 >= 22) ||
                (w.weight1 > 0 && w.boneIndex1 >= 22) ||
                (w.weight2 > 0 && w.boneIndex2 >= 22) ||
                (w.weight3 > 0 && w.boneIndex3 >= 22))
                verticesWithBone22Plus++;
        }
        Debug.Log("[Diag] max bone index used = " + maxIdx + " ; vertices touching bone>=22 = " + verticesWithBone22Plus);

        // Submesh sizes
        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            var sm = mesh.GetSubMesh(s);
            Debug.Log("[Diag] submesh " + s + " indexStart=" + sm.indexStart + " indexCount=" + sm.indexCount + " topology=" + sm.topology);
        }

        // Try to enumerate the bind bone names by walking the GameObject's SMR
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go != null)
        {
            var smr = go.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr != null && smr.bones != null)
            {
                Debug.Log("[Diag] FBX SMR bones[] length = " + smr.bones.Length);
                for (int i = 0; i < smr.bones.Length; i++)
                {
                    var b = smr.bones[i];
                    Debug.Log("[Diag] fbx bone " + i + " = " + (b == null ? "<null>" : b.name));
                }
            }
        }
    }
}
