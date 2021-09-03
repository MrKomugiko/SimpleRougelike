using UnityEngine;


public abstract class ItemData : ScriptableObject
{
    public Sprite Item_Sprite;
    public int ItemID;
    public string Name;
    public int Value;
    public string Description;
    public RarityTypes Rarity;
    public ItemType Type;
    //------------------------------
    public int minCount;
    public int maxCount;
    public bool IsStackable = true;
    public int StackSize = 5;
    //------------------------------

    [Header("Requirments")]
    public int Level = 1;
    public int Strength = 1;
    public int Inteligence = 1;
    public int Dexterity = 1;

    
    public bool CanBeAssignToQuickActions;

    public void Sell(ItemSlot from)
    {
        if(from.ITEM.count <= 0) 
            return;

        PlayerManager.instance.AddGold(Value);
        from.UpdateItemAmount(-1);
    }
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
        Trash
    }