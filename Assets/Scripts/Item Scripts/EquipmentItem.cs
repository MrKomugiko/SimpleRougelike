using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="New Equipment Item",menuName="GameData/Item/Equipment")]
public class EquipmentItem : ItemData
{
    public void Awake() 
    {
        Type = ItemType.Default;
        Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = false;
    }

    public bool Equip(ItemSlot slot)
    {
        var targetStorage = slot.ParentStorage.StorageName=="Player"?PlayerManager.instance._mainBackpack:PlayerManager.instance._EquipedItems;
        slot.ParentStorage.EquipItemFromSlot(slot, targetStorage);
        return true;
    }

}

