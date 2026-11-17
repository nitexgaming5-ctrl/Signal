using System;
using System.IO;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Reads and writes the save file as JSON. All rules about what is saved live in Core.</summary>
    public static class SaveFileStore
    {
        /// <summary>In the editor the save sits beside the project (SignalSave/, which git ignores).</summary>
        public static string DefaultPath
        {
            get
            {
#if UNITY_EDITOR
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(projectRoot, "SignalSave", "signal-save.json");
#else
                return Path.Combine(Application.persistentDataPath, "signal-save.json");
#endif
            }
        }

        public static void Save(SaveData data, string path)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            // Write to a temporary file first so a crash cannot leave half a save behind.
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));

            if (File.Exists(path))
                File.Delete(path);

            File.Move(temp, path);
        }

        public static bool TryLoad(string path, out SaveData data, out string error)
        {
            data = null;
            error = null;

            if (!File.Exists(path))
            {
                error = "No save file found.";
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                error = "The save file could not be read: " + exception.Message;
                return false;
            }

            if (data == null)
            {
                error = "The save file is empty.";
                return false;
            }

            return true;
        }
    }
}
