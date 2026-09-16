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

    [Header("UI References")]
    [Tooltip("The Image component that displays the icon. Optional if this GameObject already has an Image component.")]
    public Image iconImage;

    [Tooltip("The GameObject containing your Name Text. It will be hidden on the map and shown in the inventory.")]
    public GameObject nameTextObject;

    private bool isInInventory = false;

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
                }
                else
                {
                    // The inventory was cleared (e.g. because we just loaded a save file),
                    // so we need to add this fresh world copy directly into the inventory!
                    PickUpItem();
                }
            }
        }
    }

    // Call this from the Button's OnClick event in the inspector
    public void OnItemClicked()
    {
        if (!isInInventory)
        {
            PickUpItem();
        }
        else
        {
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

            // Refresh the pagination so the layout updates immediately
            InventoryController.Instance.UpdatePaginationUI();
            
            // Register it in our save tracking system
            if (StoryManager.Instance != null && !StoryManager.Instance.collectedItemNames.Contains(itemName))
            {
                StoryManager.Instance.collectedItemNames.Add(itemName);
            }

            Debug.Log($"Picked up {itemName} and added to inventory!");
        }
        else
        {
            Debug.LogWarning("Cannot pick up item: InventoryController or ItemGrid is missing.");
        }
    }
}
