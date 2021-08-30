using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            itemSlot.ID = i;
            itemSlot.IsEmpty = true;
            itemSlot.IsLocked = i < NumberOfUnlockedSlots?false:true;
        }
    }

    public static void AssignItemToActionSlot(int slotID)
    {
        print("assign item to "+slotID+ "slot position");
    }
}
