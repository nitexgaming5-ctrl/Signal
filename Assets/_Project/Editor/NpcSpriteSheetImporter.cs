#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace Signal.Game.Editor
{
    /// <summary>Imports the three visual-only NPC test sheets from the supplied ZIP.</summary>
    public static class NpcSpriteSheetImporter
    {
        private const string SourcePrefix = "signal_npc_sprites/";
        private const string DestinationRoot = "Assets/_Project/Resources/NpcSprites";

        [MenuItem("Signal/Import NPC Rendering Test Sprites...")]
        public static void Import()
        {
            string archivePath = EditorUtility.OpenFilePanel(
                "Select signal_npc_sprites.zip", "", "zip");
            if (string.IsNullOrEmpty(archivePath))
                return;

            string fullRoot = Path.GetFullPath(DestinationRoot);
            Directory.CreateDirectory(fullRoot);
            int imported = 0;

            using (var file = File.OpenRead(archivePath))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
            {
                string[] expected = { "npc1_hoodie.png", "npc2_sweater.png", "npc3_jacket.png" };
                foreach (string fileName in expected)
                {
                    ZipArchiveEntry entry = archive.GetEntry(SourcePrefix + fileName);
                    if (entry == null)
                    {
                        EditorUtility.DisplayDialog("NPC sprite import failed",
                            "The archive is missing " + fileName + ".", "OK");
                        return;
                    }

                    string destination = Path.GetFullPath(Path.Combine(fullRoot, fileName));
                    if (!destination.StartsWith(fullRoot + Path.DirectorySeparatorChar))
                        throw new IOException("Invalid NPC sprite destination.");
                    using (Stream input = entry.Open())
                    using (FileStream output = File.Create(destination))
                        input.CopyTo(output);
                    imported++;
                }
            }

            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { DestinationRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
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
                "Imported " + imported + " sprite sheets. Enter Play Mode to see three generic city residents using them.", "OK");
            Debug.Log("Signal: imported three visual-only NPC rendering test sprite sheets.");
        }
    }
}
#endif
