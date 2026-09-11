using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct StoryEventTrigger
{
    [Tooltip("The specific story progress stage required for this event.")]
    public int requiredStoryProgress;
    
    [Tooltip("Fires when the player arrives and the story progress matches.")]
    public UnityEvent onStoryEvent;
}

public class LocationController : MonoBehaviour
{
    [Header("Location Setup")]
    public string locationName;

    [Header("Story Triggers")]
    [Tooltip("Set up multiple events that can happen here at different story stages.")]
    public List<StoryEventTrigger> storyEvents = new List<StoryEventTrigger>();

    [Tooltip("Fires when the player arrives and NOTHING special is supposed to happen.")]
    public UnityEvent onNormalState;

    // We removed Start() so the dialog doesn't pop up instantly!

    // The Blacksmith button will call this function when clicked
    public void OnCharacterClicked()
    {
        // Make sure the StoryManager actually exists in our game right now
        if (StoryManager.Instance != null)
        {
            int currentProgress = StoryManager.Instance.currentStoryProgress;
            bool triggeredSpecialEvent = false;

            // Check all our possible events to see if one matches the current progress
            foreach (var storyEvent in storyEvents)
            {
                if (currentProgress == storyEvent.requiredStoryProgress)
                {
                    Debug.Log($"[{locationName}] Triggering special story event for stage: {currentProgress}");
                    storyEvent.onStoryEvent?.Invoke();
                    triggeredSpecialEvent = true;
                    break; // We found our event, no need to keep checking
                }
            }

            // If no special event matched the current progress, load the normal state
            if (!triggeredSpecialEvent)
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
