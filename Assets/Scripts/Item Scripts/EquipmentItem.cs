using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="New Equipment Item",menuName="GameData/Item/Equipment")]
public class EquipmentItem : ItemData
{
    public EquipmentType eqType = EquipmentType.Helmet;
    public List<Perk> MainPerks  = new List<Perk>();
    public List<Perk> ExtraPerks = new List<Perk>();

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
        var MoveItemFromStorage = slot.ParentStorage.StorageName=="Player"?PlayerManager.instance._mainBackpack:PlayerManager.instance._EquipedItems;
        
        var result = slot.ParentStorage.EquipItemFromSlot(slot, MoveItemFromStorage);

            if(result == true)
            {
                if(MoveItemFromStorage == PlayerManager.instance._EquipedItems )  //  item wyciągany z założonych == UNEQUIP
                    {PlayerManager.instance.STATS.UnequipItem_UpdateStatistics(this);}
               
               if(MoveItemFromStorage == PlayerManager.instance._mainBackpack )  // item wyciągany z plecaka == EQUIP
                    {PlayerManager.instance.STATS.EquipItem_UpdateStatistics(this);}
            }
        return result;
    }
}
    [Serializable]
    public struct Perk
    {
        public PerkType type;
        public string value;
    }
    public enum PerkType
    {
        MinAttack,              Accuracy,           Evasion,
        MaxAttack,              DamageReduction,    StrengthBonus,
        Range,                  Armor,              InteligenceBonus,
        CrticalHitRate,         HealthRegen,        DexterityBonus,
        CriticalHitDamage,      StaminaRegen,       VitalityBonus
    }
    public enum EquipmentType
    {
        Armor,                  SecondaryWeapon,    Helmet,
        Shoulders,              Shoes,              Belt,
        PrimaryWeapon,          Gloves,
    }
