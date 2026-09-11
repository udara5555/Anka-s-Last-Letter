using UnityEngine;

[System.Serializable]
public struct DialogLine
{
    [Tooltip("Type the exact name of the character from your Manager's database")]
    public string characterName;
    
    [TextArea(3, 5)]
    public string sentence;
}

public class DialogData : MonoBehaviour
{
    [Header("Dialog Content")]
    [Tooltip("Add a new element for every line of dialogue in this conversation.")]
    public DialogLine[] dialogueLines;

    // This is the function you select in your LocationController!
    public void StartDialog()
    {
        if (DialogManager.Instance != null)
        {
            // We tell the central Manager to display THIS specific data
            DialogManager.Instance.StartDialog(this);
        }
        else
        {
            Debug.LogWarning("DialogManager not found! Make sure it is in your scene.");
        }
    }
}
