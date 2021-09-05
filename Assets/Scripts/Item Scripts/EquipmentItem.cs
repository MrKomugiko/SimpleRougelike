using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="New Equipment Item",menuName="GameData/Item/Equipment")]
public class EquipmentItem : ItemData
{
    public EquipmentType eqType = EquipmentType.Helmet;
    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite LeftSprite;
    public Sprite RightSprite;   
    
    public void Awake() 
    {
        ItemCoreSettings.Type = ItemType.Equipment;
        CanBeAssignToQuickActions = false;
    }

    public bool Equip(ItemSlot slot)
    {
        var targetStorage = slot.ParentStorage.StorageName=="Player"?PlayerManager.instance._mainBackpack:PlayerManager.instance._EquipedItems;
        
        var output = slot.ParentStorage.EquipItemFromSlot(slot, targetStorage);
        
        return output;
    }

    //------------------------------



}
    public enum EquipmentType
    {
        Armor,
        Shoulders,
        PrimaryWeapon,
        SecondaryWeapon,
        Shoes,
        Gloves,
        Helmet,
        Belt,
    }
