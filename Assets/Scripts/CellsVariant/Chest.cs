using System;
using System.Collections.Generic;
using UnityEngine;

public class Chest : IChest
{
    public ISpecialTile Parent {get;set;}
    public List<ItemPack> ContentItems {get;set;} = new List<ItemPack>();
    public Chest(ISpecialTile source, List<ItemPack> contentItems )
    {
        Parent = source;
        ContentItems = contentItems;
        Action action = ()=>GenerateChestLootWindowPopulatedWithItems(this, ContentItems);
        (Parent as ISpecialTile).AvaiableActions.Add((action, "Open Chest",ActionIcon.OpenChest,true));
    }    
    public void GenerateChestLootWindowPopulatedWithItems(IChest source, List<ItemPack> items)
    {
        AnimateWindowScript.instance.SwitchTab("EquipmentTab");
        // Close map borders if open
        NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,true));    

        Debug.Log("TU BEDZIE SKRZYNECZKA");
        var chestWindow = GameManager.instance.ContentLootWindow.GetComponent<ChestLootScript>();
        chestWindow.gameObject.SetActive(true);
        chestWindow.PopulateChestWithItems(source,items);

    }
    [Serializable]
    public struct ItemPack
    {
        public int count;
        public ItemData item;
    }
}