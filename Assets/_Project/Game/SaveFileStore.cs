using System;
using System.IO;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Reads and writes the save file as JSON. All rules about what is saved live in Core.</summary>
    public static class SaveFileStore
    {
        /// <summary>Core/Game uses a separate file so the legacy prototype save is never overwritten.</summary>
        public static string DefaultPath
        {
            get
            {
#if UNITY_EDITOR
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(projectRoot, "SignalSave", "signal-core-save.json");
#else
                return Path.Combine(Application.persistentDataPath, "signal-core-save.json");
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

            // Write beside the destination, then atomically replace it. Never delete the last good save first.
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));

            if (File.Exists(path))
                File.Replace(temp, path, null);
            else
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
