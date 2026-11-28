#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace Signal.Game.Editor
{
    /// <summary>
    /// Builds runtime sprite sheets from the supplied layered-character export.
    /// Only the composed Frames images are imported; NPC behaviour remains unchanged.
    /// </summary>
    public static class NpcSpriteSheetImporter
    {
        private const string DestinationRoot = "Assets/_Project/Resources/NpcSprites";
        private const int CellWidth = 128;
        private const int CellHeight = 160;
        private const int Columns = 4;
        private const int Rows = 8;

        private static readonly string[] CharacterFolders =
        {
            "survivor_267285",
            "survivor_715836",
            "survivor_976103"
        };

        private static readonly string[] ResourceFileNames =
        {
            "npc1_hoodie.png",
            "npc2_sweater.png",
            "npc3_jacket.png"
        };

        private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };

        [MenuItem("Signal/Import NPC Rendering Test Sprites...")]
        public static void Import()
        {
            string archivePath = EditorUtility.OpenFilePanel(
                "Select TEST.zip containing the three survivor exports", "", "zip");
            if (string.IsNullOrEmpty(archivePath))
                return;

            byte[][] sheets = new byte[CharacterFolders.Length][];
            try
            {
                using (var file = File.OpenRead(archivePath))
                using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    for (int i = 0; i < CharacterFolders.Length; i++)
                        sheets[i] = BuildSheet(archive, CharacterFolders[i]);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("NPC sprite import failed",
                    "The archive could not be read or one of its animation frames was invalid. See the Console for details.",
                    "OK");
                return;
            }

            string fullRoot = Path.GetFullPath(DestinationRoot);
            Directory.CreateDirectory(fullRoot);
            for (int i = 0; i < sheets.Length; i++)
            {
                string destination = Path.GetFullPath(Path.Combine(fullRoot, ResourceFileNames[i]));
                if (!destination.StartsWith(fullRoot + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Invalid NPC sprite destination.");

                File.WriteAllBytes(destination, sheets[i]);
            }

            AssetDatabase.Refresh();
            for (int i = 0; i < ResourceFileNames.Length; i++)
            {
                string assetPath = DestinationRoot + "/" + ResourceFileNames[i];
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Default;
                importer.isReadable = true;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog("NPC rendering test sprites",
                "Imported all three survivor characters. Enter Play Mode to see the three city walkers using their directional idle and walking animations.",
                "OK");
            Debug.Log("Signal: imported three survivor NPC animation sheets from TEST.zip.");
        }

        private static byte[] BuildSheet(ZipArchive archive, string characterFolder)
        {
            var atlas = new Texture2D(CellWidth * Columns, CellHeight * Rows,
                TextureFormat.RGBA32, false, false);

            try
            {
                for (int row = 0; row < Rows; row++)
                {
                    bool walking = row >= 4;
                    string pose = walking ? "Walk" : "Idle";
                    string direction = Directions[row % 4];
                    int sourceFrameCount = walking ? 4 : 3;

                    for (int column = 0; column < sourceFrameCount; column++)
                    {
                        string entryPath = "TEST/" + characterFolder + "/Frames/" +
                            pose + "_" + direction + "_" + column + ".png";
                        ZipArchiveEntry entry = archive.GetEntry(entryPath);
                        if (entry == null)
                            throw new InvalidDataException("Missing animation frame: " + entryPath);

                        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
                        try
                        {
                            using (Stream input = entry.Open())
                            using (var memory = new MemoryStream())
                            {
                                input.CopyTo(memory);
                                if (!source.LoadImage(memory.ToArray(), false))
                                    throw new InvalidDataException("Could not decode animation frame: " + entryPath);
                            }

                            if (source.width != CellWidth || source.height != CellHeight)
                                throw new InvalidDataException("Unexpected dimensions for " + entryPath +
                                    ": expected " + CellWidth + "x" + CellHeight + ", received " +
                                    source.width + "x" + source.height + ".");

                            // Row zero is the top row of the authored sheet.
                            int y = (Rows - 1 - row) * CellHeight;
                            atlas.SetPixels32(column * CellWidth, y, CellWidth, CellHeight,
                                source.GetPixels32());
                        }
                        finally
                        {
                            DestroyImmediate(source);
                        }
                    }
                }

                atlas.Apply(false, false);
                return atlas.EncodeToPNG();
            }
            finally
            {
                DestroyImmediate(atlas);
            }
        }
    }
}
#endif
