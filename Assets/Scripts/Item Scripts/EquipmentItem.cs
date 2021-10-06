using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="New Equipment Item",menuName="GameData/Item/Equipment")]
public class EquipmentItem : ItemData
{

    public EquipmentSpecifiedType EquipmentSpecificType = EquipmentSpecifiedType.Sword;
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
        var MoveItemTo = slot.ParentStorage.StorageName=="Player"?PlayerManager.instance._mainBackpack:PlayerManager.instance._EquipedItems;
        var result = slot.ParentStorage.EquipItemFromSlot(slot, MoveItemTo);
            if(result == true)
            {
                if(MoveItemTo == PlayerManager.instance._EquipedItems ) 
                {
                    {PlayerManager.instance.STATS.EquipItem_UpdateStatistics(this);}
                    PlayerManager.instance.RefreshWearedEquipmentUIonMap();
                }
               
               if(MoveItemTo == PlayerManager.instance._mainBackpack ) 
               {
                    {PlayerManager.instance.STATS.UnequipItem_UpdateStatistics(this);}
                    PlayerManager.instance.RefreshWearedEquipmentUIonMap();
               }
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
        CriticalHitDamage,      StaminaRegen,       VitalityBonus,
        BlockRate,              MaxHealth,          MaxStamina,
        MaxEnergy,
    }
    public enum EquipmentType
    {
        Armor,                  SecondaryWeapon,    Helmet,
        Shoulders,              Shoes,              Belt,
        PrimaryWeapon,          Gloves,
    }

    public enum EquipmentSpecifiedType
    {
        NoRestriction,
        Sword,
        LongSword,
        Bow,
        Crossbow, // kusza
        Wand,   // rozdzka
        Shield,
        Dagger,
        Axe,
        Grimoire, // księga czarów
        Quiver // kołczan

    }
