using System;
using System.Collections.Generic;
using System.Linq;
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
    public void AddItemsToContent(List<ItemPack> items)
    {
        foreach(var _item in items)
        {
            if(ContentItems.Any(item=>item.item == _item.item))
            {
                var itemMaxStackQuantity = _item.item.StackSettings.StackSize;
                // sprawdzenie czy stack nie jest przepełniony
                var existingMatchedItems = ContentItems.Where(item=>item.item == _item.item).ToList();
                foreach(var matchItem in existingMatchedItems)
                {
                    Debug.Log("kolejne wystąpienie tego itemka w zawartosci skrzynki. przed doaniem znajdowało sie ich:"+matchItem.Count);

                    // wypełnianie na maxa zawartosci skrzynki
                    var countToFillStackMAtchedItem = matchItem.item.StackSettings.StackSize - matchItem.Count;
                    var itemcountLeft = _item.Count;
                    var newCount = itemcountLeft>countToFillStackMAtchedItem?countToFillStackMAtchedItem:itemcountLeft;
                    matchItem.Count+= newCount;
                    _item.Count -= newCount;
                    Debug.Log("dodanie "+newCount+" zostało "+_item.Count+ " slot ma teraz w sobie szt:"+matchItem.Count);

                }
                Debug.Log("pasujace istniejace pozycje przejrzane, jezeli nadal są itemki do rozdystrybuowania, dodaj nowy wpis");
                while(true)
                {
                    if(_item.Count > 0)
                    {
                        // dodanie maksymalnej wartosci lub reszte
                        var itemcountLeft = _item.Count;
                        var newCount = itemcountLeft>_item.item.StackSettings.StackSize?_item.item.StackSettings.StackSize:itemcountLeft;
                        ContentItems.Add(new ItemPack(newCount,_item.item));
                        _item.Count -= newCount;
                        Debug.Log("dodanie "+newCount+" zostało "+_item.Count);
                    }
                    else
                    {
                        Debug.Log("wszystko dodane");
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("pierwsze wystąpienie tego itemka w zawartosci skrzynki");
                while(true)
                {
                    if(_item.Count > 0)
                    {
                        // dodanie maksymalnej wartosci lub reszte
                        var itemcountLeft = _item.Count;
                        var newCount = itemcountLeft>_item.item.StackSettings.StackSize?_item.item.StackSettings.StackSize:itemcountLeft;
                        ContentItems.Add(new ItemPack(newCount,_item.item));
                        _item.Count -= newCount;
                        Debug.Log("dodanie "+newCount+" zostało "+_item.Count);
                    }
                    else
                        break;
                }
            }
        }
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