using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Chest;
using static Treasure_Cell;

public class EquipmentScript : MonoBehaviour
{
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
    [SerializeField] public GameObject ItemDetailsWindow;
    [SerializeField] public List<ItemSlot> ItemSlots = new List<ItemSlot>();

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

    private static List<Button> turnOffButtonsList = new List<Button>();
    private static List<ItemSlot> selectedItemsForQuickslot = new List<ItemSlot>();
    public static void AssignItemToActionSlot(int slotID)
    {
        print("Start assigning item to "+slotID+ "slot position");
        // OPEN inventory if closed
        if(AnimateWindowScript.instance.CurrentOpenedTab != "EquipmentTab")  
            AnimateWindowScript.instance.SwitchTab("EquipmentTab");

        // Podświetlić wszystkie przedmiory dowolnego typu z opcją assignable = true
        print("podświetlanie / wyłączenie przyciskó innych elementow");
        // zapisanie wyłączonych przycisków

        foreach(var slot in PlayerManager.PlayerInstance._mainBackpack.ItemSlots)
        {
            if(slot.ITEM.item == null) continue;

            if(slot.ITEM.item.CanBeAssignToQuickActions == false)
            {
                slot._btn.interactable = false;
                turnOffButtonsList.Add(slot._btn);
            }
            else
            {
                var ActionButton = NotificationManger.instance.NotificationList
                    .First(l=>l.BaseCell.SpecialTile is Player_Cell)
                    .PossibleActions.GetComponent<ActionSwitchController>().
                    actionButtonsList[slotID];

                selectedItemsForQuickslot.Add(slot);
                slot.EnableSelectionButton(true);
                slot._selectionBorder.GetComponent<Button>().onClick.AddListener(()=>slot.AssignToQuickSlot(ActionButton));
            
            }
        }
        
    }
    
    public void QuitFromQuickbarSelectionMode()
    {
        print("Quit from selecting quickslot item ");
        foreach(var btn in turnOffButtonsList)
        {
            btn.interactable = true;
        }
        turnOffButtonsList.Clear();

        foreach(var slot in selectedItemsForQuickslot)
        {
            slot.EnableSelectionButton(false);
            slot._selectionBorder.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        selectedItemsForQuickslot.Clear();
    }

    public bool AddSingleItemPackToBackpack(ItemPack item, int slotIndex)
    {
        if(slotIndex != -1)
            ItemSlots[slotIndex].AddNewItemToSlot(item);
        else
            ItemSlots[slotIndex].UpdateItemAmount(1);

        return true;
    }


    public (bool result,bool update, int index) CheckWhereCanYouFitThisItemInBackpack(ItemPack _itemToStack)
    {

        bool IsThereAny_NonFullAndUnlocked_Slot = ItemSlots.Where(slot=>slot.IsFull == false && slot.IsLocked==false).Any();
        if(IsThereAny_NonFullAndUnlocked_Slot == false) 
        {
            return (false,false, -1);
        }

        bool IsThereAny_Empty_Slots = ItemSlots.Where(slot=>slot.IsEmpty == true).Any();
        if( _itemToStack.item.IsStackable == false)
        {

            if(IsThereAny_Empty_Slots == true)
            {
                return (true,false, GetNextEmptySlot());
            }
            return (false,false, -1);
        } 
        
        var nonEmptySlots = ItemSlots.Where(slot=>slot.ITEM.item != null).ToList();

        ItemSlot slot = null;
        if(nonEmptySlots.Count <= ItemSlots.Count)
        {
           slot = nonEmptySlots.Where(s=>s.ITEM.item.ItemID == _itemToStack.item.ItemID && s.ITEM.item.Name == _itemToStack.item.Name)
                        .Where(s=>s.IsFull == false)
                        .FirstOrDefault();

            if(slot == null)
            {
                if(IsThereAny_Empty_Slots)
                    return (true,false, GetNextEmptySlot());
                else
                    return (false,false,-1);   
            }

            return (true,true,slot.IndexID);   
        }

        return (false,false,-1);   
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
