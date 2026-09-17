using UnityEngine;
using TMPro;

public class StoryHint : MonoBehaviour
{
    [Tooltip("The TMP_Text component that will display the hint. If not assigned, will try to find one in children.")]
    public TMP_Text hintText;

    [Tooltip("Array of hints. The index corresponds to the StoryManager's currentStoryProgress.")]
    [TextArea(2, 5)]
    public string[] hints;

    private void OnEnable()
    {
        UpdateHintText();
    }

    public void UpdateHintText()
    {
        if (hintText == null)
        {
            hintText = GetComponentInChildren<TMP_Text>();
            if (hintText == null)
            {
                Debug.LogWarning("StoryHint: No TMP_Text component found to display hints.");
                return;
            }
        }

        if (StoryManager.Instance == null)
        {
            Debug.LogWarning("StoryHint: StoryManager instance not found. Cannot determine story progress.");
            return;
        }

        int progress = StoryManager.Instance.currentStoryProgress;

        if (hints != null && hints.Length > 0)
        {
            // Clamp the progress to ensure we don't go out of bounds
            int index = Mathf.Clamp(progress, 0, hints.Length - 1);
            hintText.text = hints[index];
        }
        else
        {
            hintText.text = "No hints available.";
        }
    }
}
