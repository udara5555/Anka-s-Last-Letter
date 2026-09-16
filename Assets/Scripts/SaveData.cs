using System;

[Serializable]
public class ItemSaveData
{
    public string itemName;
    public string description;
    public string iconSpriteName; // Sprite name for Resources.Load("ItemIcons/<name>")
}

[Serializable]
public class SaveData
{
    public int storyProgress;
    public string saveDate;
    public string currentSceneName;
    public string backgroundImageName;
    public string[] collectedItemNames;
    public ItemSaveData[] collectedItems; // Full item data for restoring inventory on load
}
