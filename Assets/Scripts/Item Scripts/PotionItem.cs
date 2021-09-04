using UnityEngine;

[CreateAssetMenu(fileName="New Potion Item",menuName="GameData/Item/Potion")]
public class PotionItem : ItemData, IConsumable 
{
    public int HealthRegenerationValue;

    public void Awake() 
    {
        ItemCoreSettings.Type = ItemType.Consumable;
        ItemCoreSettings.Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = true;
        
    }

    public bool Use(int itemSlotID)
    {
        if(PlayerManager.instance._mainBackpack.ItemSlots[itemSlotID].ITEM.count <= 0) return false;

        (GameManager.Player_CELL.SpecialTile as ILivingThing).TakeDamage(-HealthRegenerationValue, ItemCoreSettings.Name);
        NotificationManger.AddValueTo_Health_Notification(HealthRegenerationValue);
        PlayerManager.instance._mainBackpack.ItemSlots[itemSlotID].UpdateItemAmount(-1);

        return true; 
    }
}

public interface IConsumable
{
    bool Use(int slotID);
}