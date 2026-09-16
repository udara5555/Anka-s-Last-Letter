using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    [Tooltip("Reference to the Inventory Panel GameObject")]
    public GameObject inventoryPanel;

    [Tooltip("Reference to the Backpack Button GameObject")]
    public GameObject backpackButton;

    [Tooltip("List of scene names where the backpack button should be hidden")]
    public string[] hideInScenes = { "Home", "OpenCutScene", "SaveScene" };

    [Header("Pagination")]
    [Tooltip("The parent transform that has the Grid Layout Group and holds the items")]
    public Transform itemGrid;
    
    [Tooltip("Maximum number of items to show on a single page")]
    public int itemsPerPage = 20;
    
    [Tooltip("Button to go to the next page")]
    public Button nextButton;
    
    [Tooltip("Button to go to the previous page")]
    public Button prevButton;
    
    private int currentPage = 0;

    [Header("Item Details Popup")]
    public GameObject popupPanel;
    public Image popupImage;
    public TMP_Text popupDescriptionText;

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
        // Ensure the inventory panel and popup are disabled by default
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
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
            UpdatePaginationUI();
        }
    }

    public void NextPage()
    {
        if (itemGrid == null) return;
        
        int maxPage = Mathf.Max(0, (itemGrid.childCount - 1) / itemsPerPage);
        if (currentPage < maxPage)
        {
            currentPage++;
            UpdatePaginationUI();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePaginationUI();
        }
    }

    public void UpdatePaginationUI()
    {
        if (itemGrid == null) return;

        int totalItems = itemGrid.childCount;
        int startIndex = currentPage * itemsPerPage;
        int endIndex = startIndex + itemsPerPage;

        // Loop through all children in the grid and enable only those for the current page
        for (int i = 0; i < totalItems; i++)
        {
            Transform child = itemGrid.GetChild(i);
            child.gameObject.SetActive(i >= startIndex && i < endIndex);
        }

        // Update Next/Prev button interactivity
        if (prevButton != null)
        {
            prevButton.interactable = currentPage > 0;
        }

        if (nextButton != null)
        {
            int maxPage = Mathf.Max(0, (totalItems - 1) / itemsPerPage);
            nextButton.interactable = currentPage < maxPage;
        }
    }

    public void ShowItemDetails(string description, Sprite icon)
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            
            if (popupImage != null)
            {
                popupImage.sprite = icon;
                // Only enable the image if there's actually an icon
                popupImage.enabled = (icon != null);
            }
            
            if (popupDescriptionText != null)
            {
                popupDescriptionText.text = description;
            }
        }
    }

    public void CloseItemDetails()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
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
