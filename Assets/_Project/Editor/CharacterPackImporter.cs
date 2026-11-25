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

            using (ZipArchive archive = ZipFile.OpenRead(archivePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryName = entry.FullName.Replace('\\', '/');
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
            EditorUtility.DisplayDialog("Signal Character Pack",
                "Imported " + imported + " PNG sheets into " + DestinationRoot +
                ". Enter Play Mode to see the modular character.", "Great");
            Debug.Log("Signal: imported " + imported + " character sheets.");
        }
    }
}
#endif
