using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WorkbenchController : MonoBehaviour
{
    [Header("UI Panels & Buttons")]
    [Tooltip("Reference to the WorkPanel GameObject")]
    public GameObject workPanel;

    [Tooltip("Reference to the Workbench Button GameObject")]
    public Button workbenchButton;

    [Tooltip("Reference to the Close Button inside WorkPanel")]
    public Button closeButton;

    [Header("Workbench Inventory Display")]
    [Tooltip("Parent transform inside WorkPanel to display inventory items (Assign 'ItemPanel' here!)")]
    public Transform workbenchItemGrid;

    [Tooltip("Prefab for displaying items in Workbench. Auto-retrieved from InventoryController if unassigned.")]
    public GameObject itemSlotPrefab;

    [Header("Item Details (Optional)")]
    [Tooltip("UI Image for selected item icon. If unassigned, opens InventoryController popup.")]
    public Image selectedItemIcon;

    [Tooltip("TMP_Text for selected item description.")]
    public TMP_Text selectedItemDescriptionText;

    [Tooltip("TMP_Text for selected item name.")]
    public TMP_Text selectedItemNameText;

    private void Awake()
    {
        AutoFindReferences();
    }

    private void Start()
    {
        AutoFindReferences();

        // Hook up button listeners automatically
        if (workbenchButton != null)
        {
            workbenchButton.onClick.RemoveListener(OpenWorkbench);
            workbenchButton.onClick.AddListener(OpenWorkbench);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseWorkbench);
            closeButton.onClick.AddListener(CloseWorkbench);
        }
    }

    private void OnEnable()
    {
        AutoFindReferences();
        if (workPanel == gameObject || (workPanel != null && workPanel.activeSelf))
        {
            RefreshWorkbenchItems();
        }
    }

    /// <summary>
    /// Auto-detects missing UI references in the hierarchy.
    /// </summary>
    private void AutoFindReferences()
    {
        // 1. Auto-find workbenchButton if script is attached to WorkbenchButton
        if (workbenchButton == null)
        {
            workbenchButton = GetComponent<Button>();
        }

        // 2. Auto-find WorkPanel if unassigned
        if (workPanel == null)
        {
            if (gameObject.name == "WorkPanel")
            {
                workPanel = gameObject;
            }
            else
            {
                Transform foundPanel = transform.Find("WorkPanel");
                if (foundPanel != null)
                {
                    workPanel = foundPanel.gameObject;
                }
                else if (transform.parent != null)
                {
                    foundPanel = transform.parent.Find("WorkPanel");
                    if (foundPanel != null)
                    {
                        workPanel = foundPanel.gameObject;
                    }
                }
            }
        }

        // 3. Auto-find ItemPanel inside WorkPanel
        if (workbenchItemGrid == null)
        {
            if (workPanel != null)
            {
                Transform itemPanelTransform = workPanel.transform.Find("ItemPanel");
                if (itemPanelTransform != null)
                {
                    workbenchItemGrid = itemPanelTransform;
                }
                else
                {
                    workbenchItemGrid = workPanel.transform;
                }
            }
            else
            {
                Transform itemPanelTransform = transform.Find("ItemPanel");
                if (itemPanelTransform != null)
                {
                    workbenchItemGrid = itemPanelTransform;
                }
            }
        }

        // 4. Auto-find CloseButton inside WorkPanel
        if (closeButton == null && workPanel != null)
        {
            Transform closeBtnTransform = workPanel.transform.Find("CloseButton");
            if (closeBtnTransform != null)
            {
                closeButton = closeBtnTransform.GetComponent<Button>();
            }
        }
    }

    /// <summary>
    /// Opens the Workbench panel and populates items from InventoryController.
    /// </summary>
    public void OpenWorkbench()
    {
        AutoFindReferences();

        if (workPanel != null)
        {
            workPanel.SetActive(true);
        }

        RefreshWorkbenchItems();
    }

    /// <summary>
    /// Closes the Workbench panel.
    /// </summary>
    public void CloseWorkbench()
    {
        if (workPanel != null)
        {
            workPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Reads all items from InventoryController / StoryManager and populates the workbench item grid.
    /// </summary>
    public void RefreshWorkbenchItems()
    {
        AutoFindReferences();

        if (workbenchItemGrid == null)
        {
            Debug.LogWarning("[WorkbenchController] workbenchItemGrid (ItemPanel) is missing.");
            return;
        }

        // Clear old workbench slots first
        for (int i = workbenchItemGrid.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(workbenchItemGrid.GetChild(i).gameObject);
        }

        // Get item slot prefab to use
        GameObject prefabToUse = itemSlotPrefab;
        if (prefabToUse == null && InventoryController.Instance != null)
        {
            prefabToUse = InventoryController.Instance.itemButtonPrefab;
        }

        // Gather all collected items
        List<ItemDisplayData> itemsToDisplay = GetCollectedItems();

        if (itemsToDisplay.Count == 0)
        {
            Debug.Log("[WorkbenchController] Player currently has no inventory items collected.");
            return;
        }

        Debug.Log($"[WorkbenchController] Displaying {itemsToDisplay.Count} items in Workbench.");

        // Log diagnostic info about ItemPanel
        RectTransform gridRect = workbenchItemGrid.GetComponent<RectTransform>();
        if (gridRect != null)
        {
            Debug.Log($"[WorkbenchController] ItemPanel size: {gridRect.rect.width} x {gridRect.rect.height}, pos: {gridRect.anchoredPosition}");
        }

        foreach (ItemDisplayData item in itemsToDisplay)
        {
            GameObject slotObj = null;

            if (prefabToUse != null)
            {
                slotObj = Instantiate(prefabToUse, workbenchItemGrid);
            }
            else
            {
                slotObj = CreateDefaultItemSlot(item.itemName, item.icon);
            }

            if (slotObj == null) continue;

            // CRITICAL: Remove InventoryItemUI IMMEDIATELY before it can run Start/Awake
            // Destroy() only removes at end of frame, so InventoryItemUI.Start() would still
            // run and set CanvasGroup.alpha = 0. DestroyImmediate prevents this.
            InventoryItemUI slotItemUI = slotObj.GetComponent<InventoryItemUI>();
            if (slotItemUI != null)
            {
                DestroyImmediate(slotItemUI);
            }

            // Reset scale
            slotObj.transform.localScale = Vector3.one;
            slotObj.SetActive(true);

            // Set Icon Image
            Image slotIcon = slotObj.GetComponent<Image>();
            if (slotIcon == null || slotIcon.sprite == null)
            {
                Image childImg = slotObj.GetComponentInChildren<Image>(true);
                if (childImg != null) slotIcon = childImg;
            }

            if (slotIcon != null)
            {
                if (item.icon != null)
                {
                    slotIcon.sprite = item.icon;
                }
                slotIcon.enabled = true;
                slotIcon.color = Color.white;
                slotIcon.gameObject.SetActive(true);
            }

            // Hide Name Text (prefab layout doesn't fit workbench grid)
            TMP_Text slotText = slotObj.GetComponentInChildren<TMP_Text>(true);
            if (slotText != null)
            {
                slotText.gameObject.SetActive(false);
            }

            // Force CanvasGroup to visible
            CanvasGroup cg = slotObj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            // Bind Button onClick for Workbench selection
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton != null)
            {
                string itemNameCopy = item.itemName;
                Sprite itemIconCopy = item.icon;
                string itemDescCopy = item.description;

                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => OnWorkbenchItemSelected(itemNameCopy, itemIconCopy, itemDescCopy));
            }

            Debug.Log($"[WorkbenchController] Created slot for '{item.itemName}' | icon: {(item.icon != null ? item.icon.name : "NULL")} | active: {slotObj.activeSelf} | parent: {slotObj.transform.parent?.name}");
        }

        // Safety: Force visibility on all children after a frame in case anything overrides it
        StartCoroutine(ForceVisibilityNextFrame());
    }

    /// <summary>
    /// Waits one frame then forces all workbench item slots to be fully visible.
    /// This catches any late Start()/Awake() calls that may have hidden them.
    /// </summary>
    private IEnumerator ForceVisibilityNextFrame()
    {
        yield return null; // wait one frame

        if (workbenchItemGrid == null) yield break;

        foreach (Transform child in workbenchItemGrid)
        {
            child.gameObject.SetActive(true);

            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            // Also check children for hidden CanvasGroups
            CanvasGroup[] childGroups = child.GetComponentsInChildren<CanvasGroup>(true);
            foreach (CanvasGroup childCg in childGroups)
            {
                childCg.alpha = 1f;
                childCg.interactable = true;
                childCg.blocksRaycasts = true;
            }
        }

        Debug.Log($"[WorkbenchController] ForceVisibility applied. ItemPanel child count: {workbenchItemGrid.childCount}");
    }

    private class ItemDisplayData
    {
        public string itemName;
        public Sprite icon;
        public string description;
    }

    /// <summary>
    /// Retrieves all collected items from InventoryController or StoryManager.
    /// </summary>
    private List<ItemDisplayData> GetCollectedItems()
    {
        List<ItemDisplayData> list = new List<ItemDisplayData>();
        HashSet<string> addedNames = new HashSet<string>();

        // 1. Try fetching directly from InventoryController's itemGrid child objects
        if (InventoryController.Instance != null && InventoryController.Instance.itemGrid != null)
        {
            InventoryItemUI[] inventoryItems = InventoryController.Instance.itemGrid.GetComponentsInChildren<InventoryItemUI>(true);
            Debug.Log($"[WorkbenchController] Found {inventoryItems.Length} InventoryItemUI in InventoryController.itemGrid");
            foreach (InventoryItemUI item in inventoryItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemName) && !addedNames.Contains(item.itemName))
                {
                    list.Add(new ItemDisplayData
                    {
                        itemName = item.itemName,
                        icon = item.icon,
                        description = item.description
                    });
                    addedNames.Add(item.itemName);
                }
            }
        }

        // 2. Fallback to StoryManager collectedItemNames list if InventoryController grid had no objects
        if (list.Count == 0 && StoryManager.Instance != null && StoryManager.Instance.collectedItemNames != null)
        {
            Debug.Log($"[WorkbenchController] Falling back to StoryManager. collectedItemNames count: {StoryManager.Instance.collectedItemNames.Count}");
            foreach (string name in StoryManager.Instance.collectedItemNames)
            {
                if (!string.IsNullOrEmpty(name) && !addedNames.Contains(name))
                {
                    Sprite loadedIcon = Resources.Load<Sprite>($"ItemIcons/{name}");
                    list.Add(new ItemDisplayData
                    {
                        itemName = name,
                        icon = loadedIcon,
                        description = ""
                    });
                    addedNames.Add(name);
                }
            }
        }

        return list;
    }

    private GameObject CreateDefaultItemSlot(string itemName, Sprite icon)
    {
        GameObject btnObj = new GameObject(itemName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(workbenchItemGrid, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);

        Image img = btnObj.GetComponent<Image>();
        if (icon != null) img.sprite = icon;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = itemName;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 16;
        tmp.color = Color.black;

        return btnObj;
    }

    /// <summary>
    /// Called when an inventory item inside the workbench is clicked.
    /// </summary>
    public void OnWorkbenchItemSelected(string itemName, Sprite icon, string description)
    {
        Debug.Log($"[WorkbenchController] Selected item in Workbench: {itemName}");

        if (selectedItemNameText != null)
        {
            selectedItemNameText.text = itemName;
        }

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = icon;
            selectedItemIcon.enabled = (icon != null);
        }

        if (selectedItemDescriptionText != null)
        {
            selectedItemDescriptionText.text = description;
        }

        if (selectedItemIcon == null && selectedItemDescriptionText == null && InventoryController.Instance != null)
        {
            InventoryController.Instance.ShowItemDetails(description, icon);
        }
    }
}
