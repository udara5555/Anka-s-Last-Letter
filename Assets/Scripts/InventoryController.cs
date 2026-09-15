using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    [Tooltip("Reference to the Inventory Panel GameObject")]
    public GameObject inventoryPanel;

    [Tooltip("Reference to the Backpack Button GameObject")]
    public GameObject backpackButton;

    [Tooltip("List of scene names where the backpack button should be hidden")]
    public string[] hideInScenes = { "Home", "OpenCutScene", "SaveScene" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure the inventory panel is disabled by default
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fix: If a scene is loaded while paused, the CanvasGroup might be left uninteractable.
        // We reset it here to ensure the UI is usable.
        CanvasGroup[] canvasGroups = GetComponentsInChildren<CanvasGroup>();
        foreach(CanvasGroup group in canvasGroups)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (backpackButton != null)
        {
            bool shouldHide = hideInScenes.Contains(scene.name);
            backpackButton.SetActive(!shouldHide);
        }
        
        // Also ensure inventory panel is closed when entering a new scene
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}
