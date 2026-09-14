using UnityEngine;

public class SaveMenuManager : MonoBehaviour
{
    public static SaveMenuManager Instance { get; private set; }

    public enum ActionMode
    {
        None,
        Save,
        Load,
        Delete
    }

    [Header("Current State")]
    [Tooltip("The action that will be performed when a slot is clicked.")]
    public ActionMode currentMode = ActionMode.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this from the "Save" button on the left
    public void SetSaveMode()
    {
        currentMode = ActionMode.Save;
        Debug.Log("Mode set to SAVE. Click a slot to save your game.");
    }

    // Call this from the "Load" button on the left
    public void SetLoadMode()
    {
        currentMode = ActionMode.Load;
        Debug.Log("Mode set to LOAD. Click a slot to load your game.");
    }

    // Call this from the "Delete" button on the left
    public void SetDeleteMode()
    {
        currentMode = ActionMode.Delete;
        Debug.Log("Mode set to DELETE. Click a slot to delete the save file.");
    }

    // Called automatically by the slots when they are clicked
    public void HandleSlotClick(SaveSlotUI slot)
    {
        switch (currentMode)
        {
            case ActionMode.Save:
                slot.ExecuteSave();
                // Reset mode after saving to prevent accidental overwrites
                currentMode = ActionMode.None; 
                break;
            case ActionMode.Load:
                slot.ExecuteLoad();
                // Reset mode after loading
                currentMode = ActionMode.None; 
                break;
            case ActionMode.Delete:
                slot.ExecuteDelete();
                // Reset mode after deleting
                currentMode = ActionMode.None; 
                break;
            case ActionMode.None:
                Debug.LogWarning("Please click 'Save', 'Load', or 'Delete' on the left menu first!");
                break;
        }
    }
}
