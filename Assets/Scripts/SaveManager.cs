using System.IO;
using UnityEngine;

public static class SaveManager
{
    // Generates a file path for a specific save slot
    private static string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    public static void SaveGame(int slotIndex, SaveData data)
    {
        // Add the current system time to the save data
        data.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Convert the SaveData object into a JSON string
        string json = JsonUtility.ToJson(data, true);
        
        // Write the JSON string to a file on the device
        string path = GetSavePath(slotIndex);
        File.WriteAllText(path, json);
        
        Debug.Log($"Game saved to {path}");
    }

    public static SaveData LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        
        if (File.Exists(path))
        {
            // Read the JSON string from the file
            string json = File.ReadAllText(path);
            
            // Convert it back into a SaveData object
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }
        else
        {
            Debug.LogWarning($"No save file found at {path}");
            return null;
        }
    }

    public static void DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save file at {path}");
        }
    }

    public static bool DoesSaveExist(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    public static bool AnySaveExists()
    {
        // Check standard slots 0 through 3 (4 slots total as per UI)
        for (int i = 0; i < 4; i++)
        {
            if (DoesSaveExist(i))
            {
                return true;
            }
        }
        return false;
    }
}
