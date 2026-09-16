using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot Configuration")]
    [Tooltip("Which slot is this? (e.g., 0, 1, 2, 3)")]
    public int slotIndex = 0;

    [Header("UI References")]
    public TextMeshProUGUI infoText;
    public Image thumbnailImage;
    
    // Optional: a default sprite to show if the slot is empty
    public Sprite emptySlotSprite;

    private void OnEnable()
    {
        // Whenever this UI panel is turned on, refresh its data
        RefreshSlotUI();
    }

    public void RefreshSlotUI()
    {
        if (SaveManager.DoesSaveExist(slotIndex))
        {
            SaveData data = SaveManager.LoadGame(slotIndex);
            
            // Update Text
            if (infoText != null)
            {
                infoText.text = data.saveDate; // Display the saved date/time
            }

            // Update Image
            if (thumbnailImage != null && !string.IsNullOrEmpty(data.backgroundImageName))
            {
                // Try to load the background from a "Resources/Backgrounds" folder
                Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{data.backgroundImageName}");
                if (bgSprite != null)
                {
                    thumbnailImage.sprite = bgSprite;
                    thumbnailImage.color = Color.white; // Ensure it's fully opaque
                }
            }
        }
        else
        {
            // Slot is empty
            if (infoText != null)
            {
                infoText.text = "Empty";
            }

            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = emptySlotSprite;
                // Optionally dim or hide the thumbnail if there is no default empty sprite
                if (emptySlotSprite == null)
                {
                    thumbnailImage.color = new Color(1, 1, 1, 0); // Transparent
                }
            }
        }
    }

    // Call this from the Button component on the Slot itself!
    public void OnSlotClicked()
    {
        if (SaveMenuManager.Instance != null)
        {
            SaveMenuManager.Instance.HandleSlotClick(this);
        }
        else
        {
            Debug.LogWarning("No SaveMenuManager found in the scene! Please add one.");
        }
    }

    // Called by SaveMenuManager when in Save mode
    public void ExecuteSave()
    {
        // Gather current data from StoryManager
        SaveData newData = new SaveData();
        
        if (StoryManager.Instance != null)
        {
            newData.storyProgress = StoryManager.Instance.currentStoryProgress;
            newData.backgroundImageName = StoryManager.Instance.currentBackgroundImageName;
            newData.currentSceneName = StoryManager.Instance.currentSceneName;
            newData.collectedItemNames = StoryManager.Instance.collectedItemNames.ToArray();

            // Save full item data from the inventory grid
            if (InventoryController.Instance != null && InventoryController.Instance.itemGrid != null)
            {
                int childCount = InventoryController.Instance.itemGrid.childCount;
                var items = new System.Collections.Generic.List<ItemSaveData>();
                
                for (int i = 0; i < childCount; i++)
                {
                    InventoryItemUI itemUI = InventoryController.Instance.itemGrid.GetChild(i).GetComponent<InventoryItemUI>();
                    if (itemUI != null)
                    {
                        ItemSaveData itemData = new ItemSaveData();
                        itemData.itemName = itemUI.itemName;
                        itemData.description = itemUI.description;
                        itemData.iconSpriteName = (itemUI.icon != null) ? itemUI.icon.name : "";
                        itemData.hasBeenOpened = itemUI.hasBeenOpened;
                        items.Add(itemData);
                    }
                }
                
                newData.collectedItems = items.ToArray();
            }
        }
        else
        {
            Debug.LogWarning("No StoryManager found in the scene! Saving default values.");
        }

        // Save to disk
        SaveManager.SaveGame(slotIndex, newData);
        
        // Refresh the UI to show the new save
        RefreshSlotUI();
    }

    // Holds the item data to restore after the Map scene finishes loading
    private static ItemSaveData[] pendingItemRestore = null;

    // Called by SaveMenuManager when in Load mode
    public void ExecuteLoad()
    {
        if (SaveManager.DoesSaveExist(slotIndex))
        {
            SaveData data = SaveManager.LoadGame(slotIndex);
            
            if (StoryManager.Instance != null)
            {
                // Restore Story progress
                StoryManager.Instance.currentStoryProgress = data.storyProgress;
                StoryManager.Instance.currentBackgroundImageName = data.backgroundImageName;
                StoryManager.Instance.currentSceneName = data.currentSceneName;
                if (data.collectedItemNames != null)
                {
                    StoryManager.Instance.collectedItemNames = new System.Collections.Generic.List<string>(data.collectedItemNames);
                }

                // Clear the current persistent inventory grid so we don't get duplicates when the map scene reloads
                // We must unparent them immediately so they don't interfere with the new scene's Start() logic!
                if (InventoryController.Instance != null && InventoryController.Instance.itemGrid != null)
                {
                    int childCount = InventoryController.Instance.itemGrid.childCount;
                    for (int i = childCount - 1; i >= 0; i--)
                    {
                        Transform child = InventoryController.Instance.itemGrid.GetChild(i);
                        child.SetParent(null); // Remove from grid instantly
                        Destroy(child.gameObject); // Queue for memory cleanup
                    }
                }
                
                // Store the item data to restore after scene loads
                pendingItemRestore = data.collectedItems;
                
                // Subscribe to sceneLoaded to restore items after the Map scene is ready
                SceneManager.sceneLoaded += OnMapSceneLoadedForRestore;
                
                // Transition back to the main Map Scene unconditionally
                SceneManager.LoadScene("Map");
            }
            else
            {
                Debug.LogError("No StoryManager found to load data into!");
            }
        }
        else
        {
            Debug.Log("Trying to load an empty slot!");
        }
    }

    /// <summary>
    /// Callback: after the Map scene loads, restore collected items into the inventory.
    /// </summary>
    private static void OnMapSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe immediately so this only runs once
        SceneManager.sceneLoaded -= OnMapSceneLoadedForRestore;

        if (pendingItemRestore != null && pendingItemRestore.Length > 0)
        {
            if (InventoryController.Instance != null)
            {
                InventoryController.Instance.RestoreItemsFromSave(pendingItemRestore);
            }
            else
            {
                Debug.LogWarning("InventoryController not found after scene load — items not restored.");
            }
            pendingItemRestore = null;
        }
    }

    // Called by SaveMenuManager when in Delete mode
    public void ExecuteDelete()
    {
        SaveManager.DeleteSave(slotIndex);
        RefreshSlotUI();
    }
}
