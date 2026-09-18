using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftedItem : MonoBehaviour
{
    public void Configure(string itemName, Sprite icon)
    {
        InventoryItemUI itemUI = GetComponent<InventoryItemUI>();
        if (itemUI == null)
        {
            itemUI = gameObject.AddComponent<InventoryItemUI>();
        }

        Image itemImage = GetComponent<Image>();
        TMP_Text itemNameText = GetComponentInChildren<TMP_Text>(true);

        itemUI.itemName = itemName;
        itemUI.icon = icon;
        itemUI.iconImage = itemImage;
        itemUI.nameTextObject = itemNameText != null ? itemNameText.gameObject : null;

        if (itemImage != null)
        {
            itemImage.sprite = icon;
            itemImage.enabled = icon != null;
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemName;
            itemNameText.gameObject.SetActive(true);
        }
    }
}
