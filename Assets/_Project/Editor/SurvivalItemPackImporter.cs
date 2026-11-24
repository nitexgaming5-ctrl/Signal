using System;
using System.Collections.Generic;
using System.IO;
using Signal.Game;
using UnityEditor;
using UnityEngine;

namespace Signal.EditorTools
{
    /// <summary>
    /// Imports the user-provided 100-icon survival pack as point-filtered sprites and
    /// creates reusable ItemDefinition assets from the checked-in text manifest.
    /// Artwork itself is a local licensed dependency and must not be committed publicly.
    /// </summary>
    public static class SurvivalItemPackImporter
    {
        private const string ManifestPath = "Assets/_Project/Editor/SurvivalItemPackManifest.tsv";
        private const string Root = "Assets/_Project/Resources/SurvivalItems";
        private const string IconRoot = Root + "/Icons";
        private const string DefinitionRoot = Root + "/Definitions";

        private sealed class Entry
        {
            public int Number;
            public string Name;
            public string Category;
            public string Description;
            public int BaseValue;
            public int Width;
            public int Height;
            public int MaxStack;
        }

        [MenuItem("Tools/Signal/Import Survival Items Pack")]
        public static void Import()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError("Survival item manifest is missing: " + ManifestPath);
                return;
            }

            List<Entry> entries = ParseManifest(File.ReadAllText(ManifestPath), out string parseError);
            if (entries == null)
            {
                Debug.LogError("Survival item manifest is invalid: " + parseError);
                return;
            }

            var missingIcons = new List<string>();
            foreach (Entry entry in entries)
            {
                string iconPath = IconPath(entry.Number);
                if (!File.Exists(iconPath))
                    missingIcons.Add(iconPath);
            }
            if (missingIcons.Count > 0)
            {
                Debug.LogError("Survival item import cancelled. Missing " + missingIcons.Count +
                    " icon file(s):\n" + string.Join("\n", missingIcons));
                return;
            }

            AssetDatabase.Refresh();
            EnsureFolder(Root);
            EnsureFolder(IconRoot);
            EnsureFolder(DefinitionRoot);

            int created = 0;
            int updated = 0;
            foreach (Entry entry in entries)
            {
                string iconPath = IconPath(entry.Number);
                AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogError("Could not configure sprite importer for " + iconPath);
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();

                Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (icon == null)
                {
                    Debug.LogError("Unity did not import a Sprite from " + iconPath);
                    continue;
                }

                string assetPath = DefinitionPath(entry);
                ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<ItemDefinition>();
                    AssetDatabase.CreateAsset(definition, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }

                var serialized = new SerializedObject(definition);
                SetString(serialized, "displayName", entry.Name);
                SetString(serialized, "description", entry.Description);
                SetString(serialized, "category", entry.Category);
                SetInt(serialized, "baseValue", entry.BaseValue);
                SetObject(serialized, "icon", icon);
                SetInt(serialized, "defaultQuality", 0);
                SetInt(serialized, "defaultCondition", 0);
                SetInt(serialized, "defaultAuthenticity", 0);
                SetInt(serialized, "defaultOwnership", 0);
                SetInt(serialized, "gridWidth", entry.Width);
                SetInt(serialized, "gridHeight", entry.Height);
                SetInt(serialized, "maxStack", entry.MaxStack);

                SerializedProperty id = serialized.FindProperty("id");
                if (id == null)
                    throw new InvalidOperationException("ItemDefinition is missing its stable id field.");
                if (string.IsNullOrWhiteSpace(id.stringValue))
                    id.stringValue = Guid.NewGuid().ToString();

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Survival item import finished. Created " + created + ", updated " + updated +
                " ItemDefinition assets from " + entries.Count + " manifest entries. Items are catalogued, " +
                "but are not granted to the player or added to loot/shop stock.");
        }

        private static List<Entry> ParseManifest(string text, out string error)
        {
            error = null;
            var entries = new List<Entry>();
            var numbers = new HashSet<int>();
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] fields = line.Split('|');
                if (fields.Length != 8 ||
                    !int.TryParse(fields[0], out int number) ||
                    !int.TryParse(fields[4], out int baseValue) ||
                    !int.TryParse(fields[5], out int width) ||
                    !int.TryParse(fields[6], out int height) ||
                    !int.TryParse(fields[7], out int maxStack) ||
                    number < 1 || number > 100 || !numbers.Add(number) ||
                    string.IsNullOrWhiteSpace(fields[1]) || string.IsNullOrWhiteSpace(fields[2]) ||
                    string.IsNullOrWhiteSpace(fields[3]) || baseValue < 0 ||
                    width < 1 || height < 1 || maxStack < 1)
                {
                    error = "Invalid or duplicate manifest row: " + line;
                    return null;
                }

                entries.Add(new Entry
                {
                    Number = number,
                    Name = fields[1],
                    Category = fields[2],
                    Description = fields[3],
                    BaseValue = baseValue,
                    Width = width,
                    Height = height,
                    MaxStack = maxStack
                });
            }

            if (entries.Count != 100)
            {
                error = "Expected exactly 100 entries, found " + entries.Count + ".";
                return null;
            }

            entries.Sort((left, right) => left.Number.CompareTo(right.Number));
            return entries;
        }

        private static string IconPath(int number)
        {
            return IconRoot + "/Item_" + number.ToString("000") + ".png";
        }

        private static string DefinitionPath(Entry entry)
        {
            return DefinitionRoot + "/Item_" + entry.Number.ToString("000") + "_" +
                MakeSlug(entry.Name) + ".asset";
        }

        private static string MakeSlug(string value)
        {
            var result = new System.Text.StringBuilder();
            foreach (char character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                    result.Append(character);
                else if (result.Length > 0 && result[result.Length - 1] != '_')
                    result.Append('_');
            }
            return result.ToString().Trim('_');
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static void SetString(SerializedObject serialized, string name, string value)
        {
            SerializedProperty property = RequireProperty(serialized, name);
            property.stringValue = value;
        }

        private static void SetInt(SerializedObject serialized, string name, int value)
        {
            SerializedProperty property = RequireProperty(serialized, name);
            property.intValue = value;
        }

        private static void SetObject(SerializedObject serialized, string name, UnityEngine.Object value)
        {
            SerializedProperty property = RequireProperty(serialized, name);
            property.objectReferenceValue = value;
        }

        private static SerializedProperty RequireProperty(SerializedObject serialized, string name)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null)
                throw new InvalidOperationException("ItemDefinition is missing serialized field '" + name + "'.");
            return property;
        }
    }
}
