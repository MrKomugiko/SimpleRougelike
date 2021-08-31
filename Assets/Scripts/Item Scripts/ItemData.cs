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
    
    public bool CanBeAssignToQuickActions;
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