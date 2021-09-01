using UnityEngine;

[CreateAssetMenu(fileName="New Potion Item",menuName="GameData/Item/Potion")]
public class PotionItem : ItemData, IConsumable
{
    public int HealthRegenerationValue;

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

    public bool Use(int slotID)
    {
       (GameManager.Player_CELL.SpecialTile as ILivingThing).TakeDamage(-HealthRegenerationValue, Name);
       NotificationManger.AddValueTo_Health_Notification(HealthRegenerationValue);
       PlayerManager.PlayerInstance._mainBackpack.ItemSlots[slotID].UpdateItemAmount(-1);

       return true; 
    }
}

public interface IConsumable
{
    bool Use(int slotID);
    void AddToQuickBar(int slotIndex);
}