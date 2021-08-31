using System;
using System.Collections.Generic;
using UnityEngine;

public class Chest : IChest
{
    ChestLootWindowScript ChestLootWindow;
    public ISpecialTile Parent {get;set;}
    public List<ItemPack> ContentItems {get;set;} = new List<ItemPack>();
    public int TotalValue =>GetChestTotalValue();   
    public Chest(ISpecialTile source, List<ItemPack> contentItems )
    {
        Parent = source;
        ContentItems = contentItems;
        if(contentItems.Count > 0)
        {
            Action action = ()=>GenerateChestLootWindowPopulatedWithItems(this, ContentItems);
           // Debug.Log("skrzynka w gotowosci");
            
            Parent.AvaiableActions.Add((action, "Open Chest",ActionIcon.OpenChest,true));
        }  
        else
        {
            Debug.Log("pusty loot -> nie pokazuje sie na mapie");
        }

    }    
    public void GenerateChestLootWindowPopulatedWithItems(IChest source, List<ItemPack> items)
    {
        AnimateWindowScript.instance.SwitchTab("EquipmentTab");
        // Close map borders if open
        NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,true));    

      //  Debug.Log("TU BEDZIE SKRZYNECZKA");
        ChestLootWindow = GameManager.instance.ContentLootWindow.GetComponent<ChestLootWindowScript>();
        ChestLootWindow.gameObject.SetActive(true);
        ChestLootWindow.PopulateChestWithItems(source,items);

        ChestLootWindow.TotalValueText.SetText(TotalValue.ToString());
    }
    public void SynchronizeItemDataWithParentCell()
    {
      //  Debug.Log("sync");
        List<ItemPack> updatedContent = new List<ItemPack>();
        ChestLootWindow.ItemSlots.ForEach(item=>updatedContent.Add(item.ITEM));
        ContentItems = updatedContent; 

        ChestLootWindow.TotalValueText.SetText(TotalValue.ToString());
    }
    public int GetChestTotalValue()
    {
        int total = 0;
        foreach(var item in ContentItems)
        {
            if(item.item == null) continue;
            total += item.count * item.item.Value;
        }
        return total;
    }

    [Serializable]
    public struct ItemPack
    {
        public int count;
        public ItemData item;
    }
}