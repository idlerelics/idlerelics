using System.IO;
using UnityEditor;
using UnityEngine;

namespace IdleRelics.EditorTools
{
    public static class RepackSheetWithPadding
    {
        private const string SheetUnityPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Sprites/Characters/Player_Adventuress_Walk.png";
        private const string BackupSuffix = ".original.png";
        private const int Cols = 6;
        private const int Rows = 8;
        private const int Padding = 8; // px of empty padding around each cell

        [MenuItem("Tools/IdleRelics/Repack -> Slice -> Build (Padded Sheet)")]
        public static void RepackSliceBuild()
        {
            if (!Repack()) return;
            // Defer slice + build so AssetDatabase finishes importing the new PNG first.
            EditorApplication.delayCall += () =>
            {
                SliceAdventuressWalk.Slice();
                BuildPlayer2DPrefab.Build();
            };
        }

        [MenuItem("Tools/IdleRelics/Repack Sheet With Padding")]
        public static bool Repack()
        {
            string absSheetPath = Path.GetFullPath(SheetUnityPath);
            if (!File.Exists(absSheetPath))
            {
                Debug.LogError("Sheet not found at " + absSheetPath);
                return false;
            }

            // Back up original once (don't overwrite an existing backup).
            string backupPath = absSheetPath + BackupSuffix;
            if (!File.Exists(backupPath))
            {
                File.Copy(absSheetPath, backupPath);
                Debug.Log("Backed up original sheet to " + Path.GetFileName(backupPath));
            }

            var srcBytes = File.ReadAllBytes(absSheetPath);
            var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!src.LoadImage(srcBytes))
            {
                Debug.LogError("Failed to decode sheet PNG.");
                return false;
            }

            int srcCellW = src.width / Cols;
            int srcCellH = src.height / Rows;
            int dstCellW = srcCellW + 2 * Padding;
            int dstCellH = srcCellH + 2 * Padding;
            int dstW = Cols * dstCellW;
            int dstH = Rows * dstCellH;

            var dst = new Texture2D(dstW, dstH, TextureFormat.RGBA32, false);
            // Fill transparent
            var clear = new Color[dstW * dstH]; // default-initialized = (0,0,0,0)
            dst.SetPixels(0, 0, dstW, dstH, clear);

            // Texture origin is bottom-left in Unity. Row 0 in our scheme is the TOP visual row.
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    int srcX = col * srcCellW;
                    int srcYBottom = src.height - (row + 1) * srcCellH;
                    var pixels = src.GetPixels(srcX, srcYBottom, srcCellW, srcCellH);

                    int dstX = col * dstCellW + Padding;
                    int dstYBottom = dst.height - (row + 1) * dstCellH + Padding;
                    dst.SetPixels(dstX, dstYBottom, srcCellW, srcCellH, pixels);
                }
            }
            dst.Apply();

            var pngBytes = dst.EncodeToPNG();
            File.WriteAllBytes(absSheetPath, pngBytes);
            AssetDatabase.ImportAsset(SheetUnityPath, ImportAssetOptions.ForceUpdate);

            Object.DestroyImmediate(src);
            Object.DestroyImmediate(dst);

            Debug.Log(string.Format(
                "Repacked sheet: src cell {0}x{1} -> dst cell {2}x{3} (+{4}px padding), new sheet {5}x{6}.",
                srcCellW, srcCellH, dstCellW, dstCellH, Padding, dstW, dstH));
            return true;
        }

        [MenuItem("Tools/IdleRelics/Restore Original Sheet (from .original.png)")]
        public static void RestoreOriginal()
        {
            string absSheetPath = Path.GetFullPath(SheetUnityPath);
            string backupPath = absSheetPath + BackupSuffix;
            if (!File.Exists(backupPath))
            {
                Debug.LogWarning("No backup found at " + backupPath);
                return;
            }
            File.Copy(backupPath, absSheetPath, overwrite: true);
            AssetDatabase.ImportAsset(SheetUnityPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Restored original (unpadded) sheet. Run Slice + Build to refresh sub-sprites.");
        }
    }
}
