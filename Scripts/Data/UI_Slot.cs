using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_Slot : MonoBehaviour
{
    [HideInInspector]
    public Item itemHolding;
    [HideInInspector]
    public int slotIndex;

    [Header("Accepts items of type: (Generic = all items)")]
    public Item.ItemType slotType;

    [Header ("RectTransform attached to this object")]
    public RectTransform rectTransform;

    [Header ("Image and Text in children to display the object")]
    public Image slotImage;
    public Text stackText;
}
