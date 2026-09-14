using UnityEngine;

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
}
