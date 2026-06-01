using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IdleRelics.EditorTools
{
    public static class SliceAdventuressIdleFront
    {
        public const string AssetPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Sprites/Characters/Player_Adventuress_Idle_Front.png";
        public const int Cols = 4;
        public const int Rows = 11;

        [MenuItem("Tools/IdleRelics/Slice Adventuress Idle Front Sheet")]
        public static void Slice()
        {
            var importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("Idle sheet importer not found at " + AssetPath);
                return;
            }

            // Read the PNG header directly so we get the SOURCE resolution, not Unity's
            // post-import (potentially downscaled) texture dimensions.
            if (!TryReadPngSize(AssetPath, out int sheetW, out int sheetH))
            {
                Debug.LogError("Failed to read PNG header at " + AssetPath);
                return;
            }
            int cellW = sheetW / Cols;
            int cellH = sheetH / Rows;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            // Sheet is 2048x5632 — default maxTextureSize (2048) would downscale it. Bump to
            // 8192 so cells stay at their native pixel size for clean slicing.
            importer.maxTextureSize = 8192;
            // Match walk-sheet PPU so the idle sprite renders at the same world scale.
            // Walk cell is ~180-196px; idle cell is 512px. Use idle cell directly as PPU
            // and the prefab's Sprite scale stays consistent for both.
            importer.spritePixelsPerUnit = cellH;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;

            var metas = new List<SpriteMetaData>(Cols * Rows);
            int frameIndex = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var m = new SpriteMetaData();
                    int xPixel = col * cellW;
                    int yPixelTop = row * cellH;
                    int yPixelBottom = sheetH - yPixelTop - cellH;
                    m.rect = new Rect(xPixel, yPixelBottom, cellW, cellH);
                    m.alignment = (int)SpriteAlignment.BottomCenter;
                    m.pivot = new Vector2(0.5f, 0f);
                    m.name = string.Format("Adventuress_IdleFront_{0:D2}", frameIndex);
                    metas.Add(m);
                    frameIndex++;
                }
            }

            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();
            Debug.Log(string.Format("Sliced {0} idle-front sprites at {1} (source PNG {2}x{3}, cell {4}x{5})",
                metas.Count, AssetPath, sheetW, sheetH, cellW, cellH));
        }

        private static bool TryReadPngSize(string unityPath, out int width, out int height)
        {
            width = 0; height = 0;
            string abs = Path.GetFullPath(unityPath);
            if (!File.Exists(abs)) return false;
            byte[] header = new byte[24];
            using (var fs = File.OpenRead(abs))
            {
                if (fs.Read(header, 0, 24) < 24) return false;
            }
            // PNG IHDR width/height: big-endian 32-bit ints at offsets 16 and 20.
            width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
            height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
            return width > 0 && height > 0;
        }
    }
}
