#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace Signal.Game.Editor
{
    public static class CharacterPackImporter
    {
        private const string RequiredPrefix = "Character/Body_A/";
        private const string DestinationRoot = "Assets/_Project/Resources/Character/Body_A";

        [MenuItem("Signal/Import World Build Character Pack...")]
        public static void ImportPack()
        {
            string archivePath = EditorUtility.OpenFilePanel(
                "Select World Build Character Pack ZIP", "", "zip");
            if (string.IsNullOrEmpty(archivePath))
                return;

            int imported = 0;
            string fullDestinationRoot = Path.GetFullPath(DestinationRoot);
            Directory.CreateDirectory(fullDestinationRoot);

            using (var archiveStream = File.OpenRead(archivePath))
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryName = entry.FullName.Replace('\\', '/');
                    if (entryName == "Character/Terms.txt")
                    {
                        string termsPath = Path.GetFullPath("Assets/_Project/Art/CharacterPackTerms.txt");
                        Directory.CreateDirectory(Path.GetDirectoryName(termsPath));
                        using (Stream input = entry.Open())
                        using (FileStream output = File.Create(termsPath))
                            input.CopyTo(output);
                        continue;
                    }

                    if (!entryName.StartsWith(RequiredPrefix) ||
                        !entryName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    string relativePath = entryName.Substring(RequiredPrefix.Length);
                    string destination = Path.GetFullPath(Path.Combine(fullDestinationRoot,
                        relativePath.Replace('/', Path.DirectorySeparatorChar)));
                    if (!destination.StartsWith(fullDestinationRoot + Path.DirectorySeparatorChar))
                        continue;

                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    using (Stream input = entry.Open())
                    using (FileStream output = File.Create(destination))
                        input.CopyTo(output);
                    imported++;
                }
            }

            AssetDatabase.Refresh();
            string[] importedAssets = AssetDatabase.FindAssets("t:Texture2D", new[] { DestinationRoot });
            foreach (string assetGuid in importedAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 4096;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog("Signal Character Pack",
                "Imported " + imported + " PNG sheets into " + DestinationRoot +
                ". Enter Play Mode to see the modular character.", "Great");
            Debug.Log("Signal: imported " + imported + " character sheets.");
        }
    }
}
#endif
