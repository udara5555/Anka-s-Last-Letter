using UnityEngine;

public class DialogData : MonoBehaviour
{
    [Header("Dialog Content")]
    public string characterName;
    
    [Tooltip("The portrait to display for this character")]
    public Sprite characterSprite;
    
    [TextArea(3, 5)] 
    public string[] sentences;

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
