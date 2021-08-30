using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Treasure_Cell;

public class EquipmentScript : MonoBehaviour
{
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
    List<ItemSlot> ItemSlots = new List<ItemSlot>();

    private void Start() {

        for(int i = 0; i< MaxCapacity; i++)
        {
            ItemSlot itemSlot = Instantiate(ItemSlotPrefab, ItemsContainer.transform).GetComponent<ItemSlot>();
            ItemSlots.Add(itemSlot);
            itemSlot.PLAYER_BACKPACK = true;
            itemSlot.IndexID = i;
            itemSlot.IsLocked = i < NumberOfUnlockedSlots?false:true;
        }
    }

    public void Clear()
    {
        ItemSlots.ForEach(s=>Destroy(s.gameObject));
        ItemSlots.Clear();
        Start();
    }

    public static void AssignItemToActionSlot(int slotID)
    {
        print("assign item to "+slotID+ "slot position");
    }

    public void AddSingleItemPackToBackpack(ItemPack item)
    {
        print("populate backpack with one item");
        int slotIndex = GetNextEmptySlot();
        ItemSlots[slotIndex].AddItemToSlot(item);
        // print("populate backpack with items");
        // int slotIndex = 0;

        // foreach(var item in items)
        // {
        //     //check what item is it:
        //         // check if stackable
        //             // find this item if is already in backpack and stack to full
        //             //  when slot is full, put rest this kind item next
        //                 //check if avaiable = notlocked



        //     ItemSlots[slotIndex].AddItemToSlot(item);
        //     slotIndex++;
        // }
    }
    public bool CanFitInPlayerBackpack(ItemPack item)
    {
        print("check if is space for item");
        int slotIndex = GetNextEmptySlot();
        if(slotIndex == -1) 
        {
            Debug.Log("not enought space");
            return false;
        }
        return true;
    }

    public int GetNextEmptySlot()
    {
        var emptySlot = ItemSlots.FirstOrDefault(s=>s.IsEmpty && !s.IsLocked);
        if(emptySlot == null)
        {
            return -1;
        }
        else
            return emptySlot.IndexID;
    }
}
