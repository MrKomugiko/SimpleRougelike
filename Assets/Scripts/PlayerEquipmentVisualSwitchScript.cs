using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEquipmentVisualSwitchScript : MonoBehaviour
{

    [SerializeField] string LastDirection => GameManager.LastPlayerDirection;
    [SerializeField] SpriteRenderer Hero_Sprite;
    [SerializeField] SpriteRenderer Helmet_Sprite;
    [SerializeField] SpriteRenderer Armor_Sprite;

    private void Start()
    {
        GetComponentInParent<SpriteRenderer>().enabled = false;
    }
    public string HelmetName;
    public string ArmorName;
    public string LastFaceDirection;

    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite LeftSprite;
    public Sprite RightSprite;

    public void UpdatePlayerGraphics()
    {

        // try
        // {

        //     LastFaceDirection = LastDirection;
        //     EquipmentItem Helmet = null, Armor = null;
        //     if (PlayerManager.instance._EquipedItems.ItemSlots.Where(slot => slot.ItemContentRestricion == EquipmentType.Helmet).First().ITEM.item != null)
        //     {
        //         Helmet = (PlayerManager.instance._EquipedItems.ItemSlots.Where(slot => slot.ItemContentRestricion == EquipmentType.Helmet).First().ITEM.item as EquipmentItem);
        //         HelmetName = Helmet.ItemCoreSettings.Name;
        //     }
        //     else
        //         HelmetName = null;

        //     if (PlayerManager.instance._EquipedItems.ItemSlots.Where(slot => slot.ItemContentRestricion == EquipmentType.Armor).First().ITEM.item != null)
        //     {
        //         Armor = (PlayerManager.instance._EquipedItems.ItemSlots.Where(slot => slot.ItemContentRestricion == EquipmentType.Armor).First().ITEM.item as EquipmentItem);
        //         ArmorName = Armor.ItemCoreSettings.Name;
        //     }
        //     else
        //         ArmorName = null;

        //     switch (LastDirection)
        //     {
        //         case "Front":
        //             Hero_Sprite.sprite = FrontSprite;
        //             Helmet_Sprite.sprite = Helmet == null ? null : Helmet.FrontSprite;
        //             Armor_Sprite.sprite = Armor == null ? null : Armor.FrontSprite;
        //             break;

        //         case "Back":
        //             Hero_Sprite.sprite = BackSprite;
        //             Helmet_Sprite.sprite = Helmet == null ? null : Helmet.BackSprite;
        //             Armor_Sprite.sprite = Armor == null ? null : Armor.BackSprite;
        //             break;

        //         case "Right":
        //             Hero_Sprite.sprite = RightSprite;
        //             Helmet_Sprite.sprite = Helmet == null ? null : Helmet.RightSprite;
        //             Armor_Sprite.sprite = Armor == null ? null : Armor.RightSprite;
        //             break;

        //         case "Left":
        //             Hero_Sprite.sprite = LeftSprite;
        //             Helmet_Sprite.sprite = Helmet == null ? null : Helmet.LeftSprite;
        //             Armor_Sprite.sprite = Armor == null ? null : Armor.LeftSprite;
        //             break;
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Debug.LogError("ex: "+ex.Message);
        // }
    }

}
