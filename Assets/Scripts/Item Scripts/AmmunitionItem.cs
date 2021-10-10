using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="New Amunition Item",menuName="GameData/Item/Ammo")]
public class AmmunitionItem : ItemData
{
    public AmmunitionType _Type = AmmunitionType.StandardArrow;
    public float BaseDamageMultiplifer = 1f;
    public void Awake() 
    {
        ItemCoreSettings.Type = ItemType.Ammunition;
        CanBeAssignToQuickActions = false;
    }
    public bool Equip(ItemSlot slot)
    {
        // var MoveItemTo = slot.ParentStorage.StorageName=="Player"?PlayerManager.instance._mainBackpack:PlayerManager.instance._EquipedItems;
        // var result = slot.ParentStorage.EquipItemFromSlot(slot, MoveItemTo);
        //     if(result == true)
        //     {
        //         if(MoveItemTo == PlayerManager.instance._EquipedItems ) 
        //         {
        //             {PlayerManager.instance.STATS.EquipItem_UpdateStatistics(this);}
        //             PlayerManager.instance.RefreshWearedEquipmentUIonMap();
        //             GameManager.instance.attackSelectorPopup.OPENandSpawnInitNodesTree();
        //         }
               
        //        if(MoveItemTo == PlayerManager.instance._mainBackpack ) 
        //        {
        //             {PlayerManager.instance.STATS.UnequipItem_UpdateStatistics(this);}
        //             PlayerManager.instance.RefreshWearedEquipmentUIonMap();
        //             GameManager.instance.attackSelectorPopup.OPENandSpawnInitNodesTree();
        //        }
        //     }
        // return result;
   // }
   return false;
}

    public enum AmmunitionType
    {
        Default,
        StandardArrow,
        CrossbowBolts,
        BrokenArrow,
        DamagedBolts

        // rock
        // inne śmieci ?
    }
}