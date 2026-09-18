using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("Item Data")]
    public string itemName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;

    [Header("Story Progress Gate")]
    [Tooltip("Minimum story progress required before this item becomes visible/pickable. Set to 0 for always available.")]
    public int requiredStoryProgress = 0;

    [Header("Story Progression")]
    [Tooltip("Check this box if picking up this item should advance the main story progress!")]
    public bool advancesStory = false;

    [Header("UI References")]
    [Tooltip("The Image component that displays the icon. Optional if this GameObject already has an Image component.")]
    public Image iconImage;

    [Tooltip("The GameObject containing your Name Text. It will be hidden on the map and shown in the inventory.")]
    public GameObject nameTextObject;

    [Header("Status")]
    [Tooltip("Has the player opened/clicked this item in the inventory?")]
    public bool hasBeenOpened = false;

    private bool isInInventory = false;

    // Whether this item is currently hidden while waiting for story progress
    private bool isWaitingForStoryProgress = false;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Get or add CanvasGroup for controlling UI visibility and raycasts without disabling GameObject
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        // Set the item name text and hide it initially since it's on the map
        if (nameTextObject != null)
        {
            nameTextObject.SetActive(false);
            
            TMP_Text tmpText = nameTextObject.GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = itemName;
            }
        }

        // Automatically try to find an Image component if one isn't assigned
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        // Apply the sprite icon to the Image component
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        // --- Save System Logic ---
        if (StoryManager.Instance != null && StoryManager.Instance.collectedItemNames.Contains(itemName))
        {
            // The item is already collected in our save data
            if (transform.parent != null && InventoryController.Instance != null && transform.parent == InventoryController.Instance.itemGrid)
            {
                // This is the persistent copy that is already living in the inventory.
                isInInventory = true;
            }
            else
            {
                // This is a newly spawned copy from the Map Scene loading.
                // Does the inventory already have this item?
                bool alreadyInGrid = false;
                if (InventoryController.Instance != null && InventoryController.Instance.itemGrid != null)
                {
                    foreach (Transform child in InventoryController.Instance.itemGrid)
                    {
                        InventoryItemUI existingItem = child.GetComponent<InventoryItemUI>();
                        if (existingItem != null && existingItem.itemName == itemName)
                        {
                            alreadyInGrid = true;
                            break;
                        }
                    }
                }

                if (alreadyInGrid)
                {
                    // The persistent inventory already has this item, so destroy this duplicate world copy
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    // The inventory was cleared (e.g. because we just loaded a save file),
                    // so we need to add this fresh world copy directly into the inventory!
                    PickUpItem();
                    return;
                }
            }
        }

        // --- Story Progress Gate Check ---
        if (!isInInventory)
        {
            CheckStoryProgressGate();
        }
        else
        {
            SetItemVisibility(true);
            // Item is already in the inventory — show the name text that was hidden above
            if (nameTextObject != null)
            {
                nameTextObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        // Continuously check if we're waiting for story progress and should become visible
        if (isWaitingForStoryProgress && !isInInventory)
        {
            CheckStoryProgressGate();
        }
    }

    /// <summary>
    /// Checks current story progress and updates item visibility accordingly.
    /// </summary>
    private void CheckStoryProgressGate()
    {
        if (requiredStoryProgress <= 0)
        {
            // No story requirement — item is always visible
            SetItemVisibility(true);
            isWaitingForStoryProgress = false;
            return;
        }

        int currentProgress = (StoryManager.Instance != null) ? StoryManager.Instance.currentStoryProgress : 0;

        if (currentProgress < requiredStoryProgress)
        {
            // Progress not reached yet — hide item
            SetItemVisibility(false);
            isWaitingForStoryProgress = true;
        }
        else
        {
            // Story progress requirement met — show item!
            SetItemVisibility(true);
            isWaitingForStoryProgress = false;
        }
    }

    /// <summary>
    /// Shows or hides the item visually and controls whether it can be interacted with.
    /// Keeps GameObject active so Update() continues running.
    /// </summary>
    private void SetItemVisibility(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = visible;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = visible;
        }
    }

    // Call this from the Button's OnClick event in the inspector
    public void OnItemClicked()
    {
        // Don't allow clicking if item is still waiting for story progress
        if (isWaitingForStoryProgress)
        {
            return;
        }

        if (!isInInventory)
        {
            PickUpItem();
        }
        else
        {
            // Mark item as opened when inspected in the inventory
            if (!hasBeenOpened)
            {
                hasBeenOpened = true;
                if (InventoryController.Instance != null)
                {
                    InventoryController.Instance.UpdateNewItemBadge();
                }
            }

            // It's already in the inventory, just show the description
            if (InventoryController.Instance != null)
            {
                InventoryController.Instance.ShowItemDetails(description, icon);
            }
        }
    }

    private void PickUpItem()
    {
        if (InventoryController.Instance != null && InventoryController.Instance.itemGrid != null)
        {
            // Make sure item is fully visible when added to inventory
            SetItemVisibility(true);
            isWaitingForStoryProgress = false;

            // Move this object to become a child of the inventory grid
            transform.SetParent(InventoryController.Instance.itemGrid, false);
            
            // Mark it as collected
            isInInventory = true;
            
            // Show the name text now that it's in the inventory
            if (nameTextObject != null)
            {
                nameTextObject.SetActive(true);
            }
            
            // Ensure its scale is reset just in case it was resized in the world canvas
            transform.localScale = Vector3.one;

            // Refresh the pagination and new item badge
            InventoryController.Instance.UpdatePaginationUI();
            InventoryController.Instance.UpdateNewItemBadge();
            
            // Register it in our save tracking system
            if (StoryManager.Instance != null)
            {
                bool isNewPickup = !StoryManager.Instance.collectedItemNames.Contains(itemName);
                if (isNewPickup)
                {
                    StoryManager.Instance.collectedItemNames.Add(itemName);
                    
                    // Advance main story progress if configured for this item
                    if (advancesStory)
                    {
                        StoryManager.Instance.AdvanceStory();
                    }
                }
            }

            Debug.Log($"Picked up {itemName} and added to inventory!");
        }
        else
        {
            Debug.LogWarning("Cannot pick up item: InventoryController or ItemGrid is missing.");
        }
    }
}
