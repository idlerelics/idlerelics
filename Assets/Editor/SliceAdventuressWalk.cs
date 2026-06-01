using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IdleRelics.EditorTools
{
    public static class SliceAdventuressWalk
    {
        private const string AssetPath = "Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Sprites/Characters/Player_Adventuress_Walk.png";
        private const int Cols = 6;
        private const int Rows = 8;

        private static readonly string[] DirNames = { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };

        [MenuItem("Tools/IdleRelics/Slice Adventuress Walk Sheet")]
        public static void Slice()
        {
            var importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("Sprite sheet importer not found at " + AssetPath);
                return;
            }

            // Read the sheet's actual dimensions so we adapt automatically when the source
            // is repacked with extra padding. Cell sizes are derived from a fixed 6x8 grid.
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath);
            if (tex == null)
            {
                Debug.LogError("Could not load texture at " + AssetPath);
                return;
            }
            int sheetW = tex.width;
            int sheetH = tex.height;
            int cellW = sheetW / Cols;
            int cellH = sheetH / Rows;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = cellH;
            // Point filtering: pixel-art rendering, prevents bleed from adjacent cells.
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;

            var metas = new List<SpriteMetaData>(Cols * Rows);
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
                    m.name = string.Format("Adventuress_Walk_{0}_{1}", DirNames[row], col);
                    metas.Add(m);
                }
            }

            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();
            Debug.Log(string.Format("Sliced {0} sprites at {1} (sheet {2}x{3}, cell {4}x{5})",
                metas.Count, AssetPath, sheetW, sheetH, cellW, cellH));
        }
    }
}
