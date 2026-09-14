using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    private static PauseController _instance;

    [Tooltip("Assign the Pause Panel GameObject here.")]
    public GameObject pausePanel;

    private void Awake()
    {
        // Prevent duplicates when reloading the scene
        if (_instance == null)
        {
            _instance = this;
            // Make this object persist across scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure the panel is initially disabled
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);

            // Add a CanvasGroup to the pause panel so we can keep it interactable
            // even when we disable all other UI in the game.
            CanvasGroup panelGroup = pausePanel.GetComponent<CanvasGroup>();
            if (panelGroup == null)
            {
                panelGroup = pausePanel.AddComponent<CanvasGroup>();
            }
            // This is the magic property that prevents it from being disabled by its parent Canvas
            panelGroup.ignoreParentGroups = true;
        }
    }

    private void Update()
    {
        // Check for right mouse button click using the new Input System
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // Call this method from your UI Button (On Click) instead of GameObject.SetActive
    public void Resume()
    {
        if (pausePanel != null && pausePanel.activeSelf)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (pausePanel != null)
        {
            // Toggle the pause panel visibility
            bool isActive = !pausePanel.activeSelf;
            pausePanel.SetActive(isActive);

            // Find all root canvases in the scene to disable/enable their interactions
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.isRootCanvas)
                {
                    CanvasGroup group = canvas.GetComponent<CanvasGroup>();
                    if (group == null)
                    {
                        group = canvas.gameObject.AddComponent<CanvasGroup>();
                    }
                    
                    // Disable interactions for everything else when paused
                    group.interactable = !isActive;
                }
            }
        }
    }
}
