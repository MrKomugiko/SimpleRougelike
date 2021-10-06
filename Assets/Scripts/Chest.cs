using System;
using System.Collections.Generic;
using UnityEngine;

public class Chest : IChest
{
    public ChestLootWindowScript ChestLootWindow  {get;set;}
    public ISpecialTile Parent {get;set;}
    public List<ItemPack> ContentItems {get;set;} = new List<ItemPack>();
    public int TotalValue =>GetChestTotalValue();   
    public bool ContentAlreadyGenerateed = false;
    public Chest(ISpecialTile source, List<ItemPack> contentItems )
    {
        Parent = source;
        ContentItems = contentItems;
        
        if(ContentItems.Count == 0) return;
        Action action = ()=>GenerateChestLootWindowPopulatedWithItems(this, ContentItems);
        
        Parent.AvaiableActions.Add((action, "Open Chest",ActionIcon.OpenChest,true));
    }    
    public void GenerateChestLootWindowPopulatedWithItems(IChest source, List<ItemPack> items)
    {
        AnimateWindowScript.instance.SwitchTab("EquipmentTab");
        NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,true));    
        ChestLootWindow = GameManager.instance.ContentLootWindow.GetComponent<ChestLootWindowScript>();
        ChestLootWindow.gameObject.SetActive(true);
        ChestLootWindow.Clear();
        ChestLootWindow.PopulateChestWithItems(source,items);
        ChestLootWindow.TotalValueText.SetText(TotalValue.ToString());
    }
    public void SynchronizeItemDataWithParentCell()
    {
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
            total += item.Count * item.item.ItemCoreSettings.GoldValue;
        }
        return total;
    }

    [Serializable]
    public class ItemPack
    {
        [SerializeField] private int count;
        public ItemData item;

        public int Count { get => count; set => count = value; }

        public ItemPack(int count, ItemData item)
        {
            this.count = count;
            this.item = item;
        }

    }
}