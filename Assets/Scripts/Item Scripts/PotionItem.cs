using UnityEngine;

[CreateAssetMenu(fileName="New Potion Item",menuName="GameData/Item/Potion")]
public class PotionItem : ItemData, IConsumable
{
    public int HealthRegenerationValue;
    public int MinLevelRequirment;
    public void Awake() 
    {
        Type = ItemType.Consumable;
        Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = true;
    }

    public void AddToQuickBar(int slotIndex)
    {
        throw new System.NotImplementedException();
    }

    public void Use()
    {
       (GameManager.Player_CELL.SpecialTile as ILivingThing).TakeDamage(-HealthRegenerationValue, Name);
    }
}

public interface IConsumable
{
    void Use();
    void AddToQuickBar(int slotIndex);
}