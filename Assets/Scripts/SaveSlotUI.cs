using UnityEngine;
using UnityEngine.UI;
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
                
                // Transition back to the main Map Scene unconditionally
                UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
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

    // Called by SaveMenuManager when in Delete mode
    public void ExecuteDelete()
    {
        SaveManager.DeleteSave(slotIndex);
        RefreshSlotUI();
    }
}
