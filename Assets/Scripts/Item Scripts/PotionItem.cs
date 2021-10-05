using UnityEngine;

[CreateAssetMenu(fileName="New Potion Item",menuName="GameData/Item/Potion")]
public class PotionItem : ItemData, IConsumable 
{
    public enum Resource{
        Health,
        Stamina,
        Energy
    }
    public Resource TypeOfPotion;
    public int ResourceRegenerationValue;

    public void Awake() 
    {
        ItemCoreSettings.Type = ItemType.Consumable;
        CanBeAssignToQuickActions = true;
    }

    public bool Use(int itemSlotID)
    {
        if(PlayerManager.instance._mainBackpack.ItemSlots[itemSlotID].ITEM.Count <= 0) return false;
        
        switch (TypeOfPotion)
        {
            case Resource.Health:
                if(GameManager.Player_CELL != null)
                {
                    Debug.Log("LEczenie:"+ResourceRegenerationValue+" HP");
                    (GameManager.Player_CELL.SpecialTile as ILivingThing).TakeDamage(-ResourceRegenerationValue, ItemCoreSettings.Name);
                    NotificationManger.AddValueTo_Health_Notification(ResourceRegenerationValue);
                }
                else
                    PlayerManager.instance.CurrentHealth += ResourceRegenerationValue;
            break;
            case Resource.Stamina:
              PlayerManager.instance.CurrentStamina += ResourceRegenerationValue;
            break;
              case Resource.Energy:
              PlayerManager.instance.CurrentEnergy += ResourceRegenerationValue;
            break;
        }

        PlayerManager.instance._mainBackpack.ItemSlots[itemSlotID].UpdateItemAmount(-1);

        return true; 
    }
}

public interface IConsumable
{
    bool Use(int slotID);
}