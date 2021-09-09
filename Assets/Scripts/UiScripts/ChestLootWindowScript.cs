using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Chest;
using static Treasure_Cell;

public class ChestLootWindowScript : MonoBehaviour
{
    public IChest LootChest;
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
    [SerializeField] public TextMeshProUGUI TotalValueText;

    [SerializeField] public List<ItemSlot> ItemSlots = new List<ItemSlot>();

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
            itemSlot.itemSlotID = i;
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
            if(slot.ITEM.item is GoldItem) {
                slot.PickAllGoldFromSlot();
                continue;
            }
            int itemCount = slot.ITEM.Count;
            for(int i =0; i < itemCount ; i++)
            {
                slot.MoveSinglePieceTo_Backpack();
            }
        }
        IfEmptyRemoveEmptyChestFromMap();
    }

    public void PopulateChestWithItems(IChest source, List<ItemPack> items)
    {
        (source as Chest).ContentAlreadyGenerateed = true;
        LootChest = source;
        int slotIndex = 0;
        foreach(var item in items)
        {
            if(item.Count == 0) continue;
            ItemSlots[slotIndex].AddNewItemToSlot(item);
            slotIndex++;
        }
        IfEmptyRemoveEmptyChestFromMap();
        
    }
    public void CloseChestWindow()
    {
      //  Debug.Log("Close Empty chest window");
        this.gameObject.SetActive(false);
        NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,false));    
    }

    public void IfEmptyRemoveEmptyChestFromMap()
    {
        if (LootChest.TotalValue == 0)
        {
            CloseChestWindow();
            
            var currentPosition = LootChest.Parent.ParentCell.CurrentPosition;

            if(GridManager.CellGridTable[currentPosition].SpecialTile is ISelectable)
                (GridManager.CellGridTable[currentPosition].SpecialTile as ISelectable).RemoveBorder();
                
            GridManager.CellGridTable[currentPosition].SpecialTile = null;
            GridManager.CellGridTable[currentPosition].SetCell(currentPosition, false);
            GridManager.CellGridTable[currentPosition].AssignType(TileTypes.grass,null);
         //   Debug.Log("Remove empty chest from map");
        }
    }
}
