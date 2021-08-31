using UnityEngine;

[CreateAssetMenu(fileName="New Item",menuName="GameData/Item")]
public class ItemData : ScriptableObject
{
    public Sprite Item_Sprite;
    public int ItemID = 0;
    public string Name = "Potion";
    public int Value = 10;
    public string Description = "Hp recovery potion";
    public RarityTypes Rarity = RarityTypes.Common;
    public ItemType Type = ItemType.Consumable;
    //------------------------------
    public int minCount;
    public int maxCount;
    public bool IsStackable = true;
    public int StackSize = 5;
    //------------------------------
    public bool CanBeAssignToQuickActions = true;
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
        Consumable,
        Equipment,
        CraftComponent,
        Trash
    }