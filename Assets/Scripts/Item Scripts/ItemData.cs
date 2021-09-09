using System;
using UnityEngine;


public abstract class ItemData : ScriptableObject
{
    public ItemCoreSettingsData ItemCoreSettings;
    public DropSettingsData DropSettings;
    public StackSettingsData StackSettings;
    public RequirmentsSettingsData RequirmentsSettings;
    public bool CanBeAssignToQuickActions;
    public void Sell(ItemSlot from)
    {
        if(from.ITEM.Count <= 0) 
            return;

        PlayerManager.instance.AddGold(ItemCoreSettings.GoldValue);
        from.UpdateItemAmount(-1);
    }
    public bool CheckRequirments()
    {
        if(PlayerManager.instance.Level < RequirmentsSettings.Level)
            return false;
        if(PlayerManager.instance.Strength < RequirmentsSettings.Strength)
            return false;
        if(PlayerManager.instance.Dexterity < RequirmentsSettings.Dexterity)
            return false;
        if(PlayerManager.instance.Inteligence < RequirmentsSettings.Inteligence)
            return false;
           
        return true;
    }
}
[Serializable]
public class ItemCoreSettingsData
{
    public Sprite Item_Sprite;
    public int ItemID;
    public string Name;
    public int GoldValue;
    public string Description;
    public RarityTypes Rarity;
    public ItemType Type;
}
[Serializable]
public class DropSettingsData
{
    public int minCount = 1;
    public int maxCount = 1;
}
[Serializable]
public class StackSettingsData
{
    public bool IsStackable = true;
    public int StackSize = 5;
}

public enum RarityTypes
    {
        Common,
        Rare,
        Epic,
        Legend,
        Ancient
    }
public enum ItemType
    {
        Default,
        Consumable,
        Equipment,
        CraftComponent,
        Trash,
    Gold
}
[Serializable]
public class RequirmentsSettingsData
{
    [SerializeField] public int Level = 0;
    [SerializeField] public int Strength = 0;
    [SerializeField] public int Inteligence = 0;
    [SerializeField] public int Dexterity = 0;
}