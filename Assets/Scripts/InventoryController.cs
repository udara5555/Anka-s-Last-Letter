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

    [Tooltip("Reference to the Hint Button GameObject")]
    public GameObject hintButton;

    [Tooltip("Reference to the Hint Panel GameObject")]
    public GameObject hintPanel;

    [Tooltip("List of scene names where the backpack button should be hidden")]
    public string[] hideInScenes = { "Home", "OpenCutScene", "SaveScene" };

    [Tooltip("List of scene names where the hint button should be hidden")]
    public string[] hideHintInScenes = { "Home", "OpenCutScene", "SaveScene" };

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

    [Header("Item Restoration")]
    [Tooltip("The ItemButton prefab to instantiate when restoring saved items")]
    public GameObject itemButtonPrefab;

    [Header("Item Details Popup")]
    public GameObject popupPanel;
    public Image popupImage;
    public TMP_Text popupDescriptionText;

    [Header("New Item Badge")]
    [Tooltip("GameObject containing the new item count badge (e.g. NewItemCount). Auto-detected under backpackButton if unassigned.")]
    public GameObject newItemBadgeObject;

    [Tooltip("TMP_Text component inside the badge that displays the number. Auto-detected if unassigned.")]
    public TMP_Text newItemCountText;

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

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        UpdateNewItemBadge();
    }

    public void RemoveItemFromInventory(string itemName)
    {
        if (itemGrid == null || string.IsNullOrEmpty(itemName)) return;

        for (int i = itemGrid.childCount - 1; i >= 0; i--)
        {
            InventoryItemUI itemUI = itemGrid.GetChild(i).GetComponent<InventoryItemUI>();
            if (itemUI != null && itemUI.itemName == itemName)
            {
                Destroy(itemUI.gameObject);
                break;
            }
        }

        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.collectedItemNames.Remove(itemName);
        }
    }

    public void AddCraftedItem(GameObject craftedItemPrefab, string itemName, Sprite icon, string description)
    {
        if (craftedItemPrefab == null || itemGrid == null || string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("Cannot add crafted item: missing prefab, item name, or inventory grid.");
            return;
        }

        GameObject craftedObject = Instantiate(craftedItemPrefab, itemGrid);
        craftedObject.transform.localScale = Vector3.one;

        CraftedItem craftedItem = craftedObject.GetComponent<CraftedItem>();
        if (craftedItem != null)
        {
            craftedItem.Configure(itemName, icon, description);
        }
        else
        {
            InventoryItemUI itemUI = craftedObject.GetComponent<InventoryItemUI>();
            if (itemUI == null)
            {
                itemUI = craftedObject.AddComponent<InventoryItemUI>();
            }

            itemUI.itemName = itemName;
            itemUI.description = description;
            itemUI.icon = icon;
            itemUI.iconImage = craftedObject.GetComponent<Image>();
            itemUI.nameTextObject = craftedObject.GetComponentInChildren<TMP_Text>(true)?.gameObject;
        }

        if (StoryManager.Instance != null && !StoryManager.Instance.collectedItemNames.Contains(itemName))
        {
            StoryManager.Instance.collectedItemNames.Add(itemName);
        }

        Button craftedButton = craftedObject.GetComponent<Button>();
        if (craftedButton == null)
        {
            craftedButton = craftedObject.AddComponent<Button>();
        }

        craftedButton.onClick.RemoveAllListeners();
        InventoryItemUI craftedUI = craftedObject.GetComponent<InventoryItemUI>();
        if (craftedUI != null)
        {
            craftedButton.onClick.AddListener(craftedUI.OnItemClicked);
        }

        UpdatePaginationUI();
        UpdateNewItemBadge();
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

        if (hintButton != null)
        {
            bool shouldHideHint = hideHintInScenes.Contains(scene.name);
            hintButton.SetActive(!shouldHideHint);
        }
        
        // Also ensure inventory panel is closed when entering a new scene
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        UpdateNewItemBadge();
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

    /// <summary>
    /// Restores collected items into the inventory grid from saved data.
    /// Called after loading a save file to populate the bag immediately.
    /// </summary>
    public void RestoreItemsFromSave(ItemSaveData[] savedItems)
    {
        if (savedItems == null || itemButtonPrefab == null || itemGrid == null)
        {
            Debug.LogWarning("Cannot restore items: missing prefab, grid, or save data.");
            return;
        }

        foreach (ItemSaveData savedItem in savedItems)
        {
            // Instantiate a new ItemButton as a child of the inventory grid
            GameObject newItemObj = Instantiate(itemButtonPrefab, itemGrid);
            newItemObj.transform.localScale = Vector3.one;

            InventoryItemUI itemUI = newItemObj.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.itemName = savedItem.itemName;
                itemUI.description = savedItem.description;
                itemUI.hasBeenOpened = savedItem.hasBeenOpened;
                itemUI.craftableItemName = savedItem.craftableItemName;
                itemUI.isRequiredCraftingItem = savedItem.isRequiredCraftingItem;
                itemUI.craftedItemDescription = savedItem.craftedItemDescription;
                if (!string.IsNullOrEmpty(savedItem.craftedItemUISpriteName))
                {
                    itemUI.craftedItemUISprite = LoadSpriteFromResources(savedItem.craftedItemUISpriteName);
                }

                // Try to load the icon sprite from Resources/ItemIcons/
                if (!string.IsNullOrEmpty(savedItem.iconSpriteName))
                {
                    Sprite loadedIcon = LoadSpriteFromResources(savedItem.iconSpriteName);
                    if (loadedIcon != null)
                    {
                        itemUI.icon = loadedIcon;
                        if (itemUI.iconImage != null)
                        {
                            itemUI.iconImage.sprite = loadedIcon;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load icon sprite for '{savedItem.itemName}' (sprite: '{savedItem.iconSpriteName}')");
                    }
                }

                // Show the name text since it's in the inventory
                if (itemUI.nameTextObject != null)
                {
                    itemUI.nameTextObject.SetActive(true);
                    TMPro.TMP_Text tmpText = itemUI.nameTextObject.GetComponent<TMPro.TMP_Text>();
                    if (tmpText != null)
                    {
                        tmpText.text = savedItem.itemName;
                    }
                }
            }

            Debug.Log($"Restored '{savedItem.itemName}' to inventory from save data.");
        }

        // Reset to page 0 and refresh
        currentPage = 0;
        UpdatePaginationUI();
        UpdateNewItemBadge();
    }

    /// <summary>
    /// Returns the number of items in the inventory grid that have not been opened/inspected yet.
    /// </summary>
    public int GetUnopenedItemCount()
    {
        if (itemGrid == null) return 0;

        int count = 0;
        foreach (Transform child in itemGrid)
        {
            InventoryItemUI itemUI = child.GetComponent<InventoryItemUI>();
            if (itemUI != null && !itemUI.hasBeenOpened)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Updates the NewItemCount badge UI. Shows badge with count if > 0, hides badge if count is 0.
    /// </summary>
    public void UpdateNewItemBadge()
    {
        // Auto-locate NewItemCount on backpackButton if unassigned
        if (newItemBadgeObject == null && backpackButton != null)
        {
            Transform badgeTransform = backpackButton.transform.Find("NewItemCount");
            if (badgeTransform != null)
            {
                newItemBadgeObject = badgeTransform.gameObject;
            }
        }

        if (newItemBadgeObject != null && newItemCountText == null)
        {
            newItemCountText = newItemBadgeObject.GetComponentInChildren<TMP_Text>();
        }

        int unopenedCount = GetUnopenedItemCount();

        if (newItemBadgeObject != null)
        {
            if (unopenedCount > 0)
            {
                newItemBadgeObject.SetActive(true);
                if (newItemCountText != null)
                {
                    newItemCountText.text = unopenedCount.ToString();
                }
            }
            else
            {
                newItemBadgeObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// Loads a sprite by name from Resources/ItemIcons/.
    /// Handles both Single and Multiple sprite mode textures.
    /// For a sprite named "letter_0", it tries loading from texture "letter" first.
    /// </summary>
    private Sprite LoadSpriteFromResources(string spriteName)
    {
        // First, try a direct load (works for Single sprite mode textures)
        Sprite directLoad = Resources.Load<Sprite>($"ItemIcons/{spriteName}");
        if (directLoad != null)
        {
            return directLoad;
        }

        // For Multiple sprite mode: sprite name is typically "textureName_N"
        // Extract the base texture name by removing the last "_N" suffix
        int lastUnderscore = spriteName.LastIndexOf('_');
        if (lastUnderscore > 0)
        {
            string baseName = spriteName.Substring(0, lastUnderscore);
            
            // Load all sprites from that texture
            Sprite[] allSprites = Resources.LoadAll<Sprite>($"ItemIcons/{baseName}");
            foreach (Sprite s in allSprites)
            {
                if (s.name == spriteName)
                {
                    return s;
                }
            }
        }

        // Fallback: search ALL sprites in ItemIcons folder
        Sprite[] allItemSprites = Resources.LoadAll<Sprite>("ItemIcons");
        foreach (Sprite s in allItemSprites)
        {
            if (s.name == spriteName)
            {
                return s;
            }
        }

        return null;
    }
}
