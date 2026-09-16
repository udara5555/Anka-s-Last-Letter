using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Story State")]
    [Tooltip("Tracks the player's overall progress in the story.")]
    public int currentStoryProgress = 0;

    [Header("Save Tracking")]
    [Tooltip("The name of the current scene, used for loading back in.")]
    public string currentSceneName = "MapScene";
    
    [Tooltip("The exact filename of the background sprite in Resources/Backgrounds (e.g. 'Village')")]
    public string currentBackgroundImageName = "";

    [Tooltip("List of item names that the player has collected in the inventory.")]
    public System.Collections.Generic.List<string> collectedItemNames = new System.Collections.Generic.List<string>();

    void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Keep this GameObject alive across all scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy any duplicate instances that might spawn when revisiting scenes
            Destroy(gameObject);
        }
    }

    // Call this method whenever a key dialog or event finishes
    public void AdvanceStory()
    {
        currentStoryProgress++;
        Debug.Log("Story advanced to stage: " + currentStoryProgress);
    }

    void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Automatically track the scene name
        currentSceneName = scene.name;

        // 2. Automatically try to find the background image
        // We look for objects named "BackgroundPanel" or "Background"
        GameObject bgObj = GameObject.Find("BackgroundPanel");
        if (bgObj == null) 
        {
            bgObj = GameObject.Find("Background");
        }

        if (bgObj != null)
        {
            // Check for UI Image
            Image uiImage = bgObj.GetComponent<Image>();
            if (uiImage != null && uiImage.sprite != null)
            {
                currentBackgroundImageName = uiImage.sprite.name;
            }
            else
            {
                // Check for 2D SpriteRenderer
                SpriteRenderer spriteRenderer = bgObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    currentBackgroundImageName = spriteRenderer.sprite.name;
                }
            }
        }
        
        Debug.Log($"[StoryManager] Scene auto-tracked: {currentSceneName} | Background: {currentBackgroundImageName}");
    }
}
