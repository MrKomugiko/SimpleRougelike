using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Treasure_Cell;

public class ChestLootScript : MonoBehaviour
{
    internal Treasure_Cell Source_TreasureCell;
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
    [SerializeField] List<ItemSlot> ItemSlots = new List<ItemSlot>();

    private void Awake() {

        for(int i = 0; i< MaxCapacity; i++)
        {
            ItemSlot itemSlot;
            if(ItemSlots.Count < MaxCapacity)
            {
                itemSlot = Instantiate(ItemSlotPrefab, ItemsContainer.transform).GetComponent<ItemSlot>();
                ItemSlots.Add(itemSlot);
            }
            else
            {
                itemSlot = ItemSlots[i];
            }
            itemSlot.IndexID = i;
            itemSlot.chest = this;
            itemSlot.IsLocked = i < NumberOfUnlockedSlots?false:true;
        }
    }
    public void Clear()
    {
        ItemSlots.ForEach(s=>Destroy(s.gameObject));
        ItemSlots.Clear();
        Awake();
    }
    public void OnClick_CollectAllItemsFromChest()
    {
        foreach(var slot in ItemSlots)
        {
            int itemCount = slot.ITEM.count;
            for(int i =0; i < itemCount ; i++)
            {
                slot.MoveSinglePieceTo_Backpack();
            }
        }
    }
    public void SynchronizeItemDataWithParentCell()
    {
        print("sync");
        List<ItemPack> updatedContent = new List<ItemPack>();
        ItemSlots.ForEach(item=>updatedContent.Add(item.ITEM));
        Source_TreasureCell.ContentItems = updatedContent;
    }
    public void PopulateChestWithItems(Treasure_Cell source, List<ItemPack> items)
    {
        Source_TreasureCell = source;
        print("populate chest iwth items");
        int slotIndex = 0;
        foreach(var item in items)
        {
            ItemSlots[slotIndex].AddItemToSlot(item);
            slotIndex++;
        }
    }
    public void CloseChestWindow()
    {
         NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,false));    
    }
}
