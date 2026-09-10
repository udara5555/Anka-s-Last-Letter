using UnityEngine;
using UnityEngine.Events;

public class LocationController : MonoBehaviour
{
    [Header("Location Setup")]
    public string locationName;

    [Header("Story Triggers")]
    [Tooltip("The required story progress stage to trigger a special event at this location.")]
    public int requiredStoryProgressForEvent = 1;

    [Tooltip("Fires when the player arrives and the story progress matches.")]
    public UnityEvent onSpecialStoryEvent;

    [Tooltip("Fires when the player arrives and nothing special is supposed to happen.")]
    public UnityEvent onNormalState;

    void Start()
    {
        CheckStoryState();
    }

    private void CheckStoryState()
    {
        // Make sure the StoryManager actually exists in our game right now
        if (StoryManager.Instance != null)
        {
            int currentProgress = StoryManager.Instance.currentStoryProgress;

            // Does the player's progress match what we need for a special event here?
            if (currentProgress == requiredStoryProgressForEvent)
            {
                Debug.Log($"[{locationName}] Triggering special story event!");
                onSpecialStoryEvent?.Invoke();
            }
            else
            {
                Debug.Log($"[{locationName}] Loading normal, everyday state.");
                onNormalState?.Invoke();
            }
        }
        else
        {
            Debug.LogWarning("StoryManager was not found! Did you forget to add it to your starting scene?");
        }
    }
}
