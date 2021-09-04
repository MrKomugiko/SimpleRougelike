using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Chest;
using static Treasure_Cell;

public class EquipmentScript : MonoBehaviour
{
    public string StorageName;
    [SerializeField] public bool PLAYER_EQUIPMENTSLOT= false;
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
    [SerializeField] public GameObject ItemDetailsWindow;
    [SerializeField] public List<ItemSlot> ItemSlots = new List<ItemSlot>();

    private void Start() 
    {
        if(PLAYER_EQUIPMENTSLOT)
        {   
            StorageName = "Player";
            
            ItemSlots.ForEach(slot=>slot.ParentStorage = this);
            ItemSlots.ForEach(slot=>slot.PLAYER_BACKPACK = true);

           return;
        } 
            

        for(int i = 0; i< MaxCapacity; i++)
        {
            StorageName = "Backpack";
            ItemSlot itemSlot = Instantiate(ItemSlotPrefab, ItemsContainer.transform).GetComponent<ItemSlot>();
            ItemSlots.Add(itemSlot);
            itemSlot.PLAYER_BACKPACK = true;
            itemSlot.itemSlotID = i;
            itemSlot.IsLocked = i < NumberOfUnlockedSlots?false:true;
            itemSlot.ParentStorage = this;
        }
    }

    public void Clear()
    {
        ItemSlots.ForEach(s=>Destroy(s.gameObject));
        ItemSlots.Clear();
        Start();
    }
    public void Reset_WipeOutDataAndImages()
    {
        ItemSlots.ForEach(s=>s._itemIcon.sprite = s._emptyBackground.sprite);
        ItemSlots.ForEach(s=>new ItemPack(0,null));
    }

    private static List<Button> turnOffButtonsList = new List<Button>();
    private static List<ItemSlot> selectedItemsForQuickslot = new List<ItemSlot>();

    public static bool AssignationItemToQuickSlotIsActive = false;
    public static int? CurrentSelectedActionButton = null;

    public static void AssignItemToActionSlot(int quickslotID)
    {
        print("AssignItemToActionSlot");
        if(AssignationItemToQuickSlotIsActive) 
        {    
            PlayerManager.instance._actionController.actionButtonsList[quickslotID].Description_TMP.SetText("Tap to Add Item");
            EquipmentScript.QuitFromQuickbarSelectionMode();
            return;
        }
        PlayerManager.instance._actionController.actionButtonsList[quickslotID].Description_TMP.SetText("Tap to Cancel");
        AssignationItemToQuickSlotIsActive = true;
        CurrentSelectedActionButton = quickslotID;

        if(AnimateWindowScript.instance.CurrentOpenedTab != "EquipmentTab")  
            AnimateWindowScript.instance.SwitchTab("EquipmentTab");

        foreach(var itemSlot in PlayerManager.instance._mainBackpack.ItemSlots)
        {
            if(itemSlot.ITEM.item == null) continue;

            if(itemSlot.ITEM.item.CanBeAssignToQuickActions == false || itemSlot.IsInQuickSlot)
            {
                turnOffButtonsList.Add(itemSlot._btn);  
                if( itemSlot.ITEM.count == 0 ) continue;
                itemSlot._btn.interactable = false;
            }
            else
            {
                selectedItemsForQuickslot.Add(itemSlot);
                itemSlot._selectionBorder.SetActive(true);
                itemSlot._selectionBorder.GetComponent<Button>().onClick.AddListener(()=>itemSlot.AssignToQuickSlot(quickslotID));
            }
        }
    }
    public static void QuitFromQuickbarSelectionMode()
    {
        print("QuitFromQuickbarSelectionMode");
        
        if(AssignationItemToQuickSlotIsActive == true)
        {
            if(PlayerManager.instance._actionController.actionButtonsList[(int)CurrentSelectedActionButton].Description_TMP.text.Contains("Cancel"))
            {
                PlayerManager.instance._actionController.actionButtonsList[(int)CurrentSelectedActionButton].Description_TMP.SetText("Tap to Add Item");
            }
        }

        foreach(var btn in turnOffButtonsList)
        {
            btn.interactable = true;
        }
        turnOffButtonsList.Clear();

        foreach(var slot in selectedItemsForQuickslot)
        {
            slot._selectionBorder.SetActive(false);
            slot._selectionBorder.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        selectedItemsForQuickslot.Clear();
        EquipmentScript.AssignationItemToQuickSlotIsActive = false;
        CurrentSelectedActionButton = null;
    }



    public bool AddSingleItemPackToBackpack(ItemPack item, int slotIndex)
    {
        print("AddSingleItemPackToBackpack");
        int avaiableSpace = ItemSlots.Count - ItemSlots.Where(s=>s.IsLocked).Count();
        if(avaiableSpace-1 < slotIndex) 
        {
            print("avaiableSpace-1 < slotindex => "+(avaiableSpace-1)+" < "+ slotIndex);
            Debug.LogWarning("Not enought space");
            return false;
        }
        if(slotIndex != -1)
            ItemSlots[slotIndex].AddNewItemToSlot(item);
        else
            ItemSlots[slotIndex].UpdateItemAmount(1);
        return true;
    }
    
 
    public bool EquipItemFromSlot(ItemSlot fromSlot, EquipmentScript toEquipment)
    {

        //TODO: REQUIRMENT CHECK

        //rozróżnienie czy item ma zostać założóny czy wrzucony do plecaka spowrotem
        bool _equipItem = toEquipment.StorageName == "Player";

        var equipmentItem = fromSlot.ITEM.item as EquipmentItem;
        print("zakładanie itemka typu "+equipmentItem.eqType);

        ItemPack ItemCopy = fromSlot.ITEM;

        if(_equipItem)
        {
            print("Zakładanie");
            // ZAKŁĄDANIE ITEMKA
            int matchEqSlotIndex = toEquipment.ItemSlots.Where(s=>s.ItemContentRestricion == equipmentItem.eqType).First().itemSlotID;
            print($"przypisany temu rodzajowi eq (w magazynie {toEquipment.StorageName}) slotem jest slot nr:"+(int)matchEqSlotIndex); 

            // sprawdzanie czy w tym miejscu juz jest jakis item
            if(toEquipment.ItemSlots[matchEqSlotIndex].ITEM.count == 0)
            {
                // pusto, tylko zakładamy nowy item
                // usuwamy z bierzącego położenia
                this.ItemSlots[fromSlot.itemSlotID].UpdateItemAmount(-1);

                // przekazujemy kopie
                toEquipment.ItemSlots[(int)matchEqSlotIndex].AddNewItemToSlot(ItemCopy);
                 // END succes nowy item zalozony
            }
            else
            {
                // inny item juz jest w tym slocie, 
                // zrob kopie i wyciągniej go z 
                ItemPack currentEquipedItemCopy = toEquipment.ItemSlots[matchEqSlotIndex].ITEM;
                // usunięcie go z zalozonego eq
                print("usunięcie przedmioru z gracza eq ( zapisanie w pamieci )");
                toEquipment.ItemSlots[matchEqSlotIndex].UpdateItemAmount(-1);
                
                this.ItemSlots[fromSlot.itemSlotID].UpdateItemAmount(-1);
                //dodanie nowego
                toEquipment.ItemSlots[(int)matchEqSlotIndex].AddNewItemToSlot(ItemCopy);
                //wrócenie starego spowrotem do plecaka
                this.ItemSlots[this.GetNextEmptySlot()].AddNewItemToSlot(currentEquipedItemCopy);
                // END succes stary item zdjęty spowrotem , nowy zalozony
            }
            if(ItemCopy.item.ItemCoreSettings.Name == "Roma Helmet")
            {
                PlayerManager.instance.HelmetIMG.enabled = true;
            }

            if(ItemCopy.item.ItemCoreSettings.Name == "Roma Armor")
            {
              PlayerManager.instance.ArmorIMG.enabled = true;
            }

              PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();
            return true;
        }
        
        if(_equipItem == false)
        {
            print("zdejmowanie");
            // ZDEJMOWANIE ITEMKA
            print($"wolny slot w {toEquipment.StorageName} = "+toEquipment.GetNextEmptySlot());
            this.ItemSlots[fromSlot.itemSlotID].UpdateItemAmount(-1);
            toEquipment.ItemSlots[toEquipment.GetNextEmptySlot()].AddNewItemToSlot(ItemCopy);
            
            if(ItemCopy.item.ItemCoreSettings.Name == "Roma Helmet")
              PlayerManager.instance.HelmetIMG.enabled = false;

            if(ItemCopy.item.ItemCoreSettings.Name == "Roma Armor")
              PlayerManager.instance.ArmorIMG.enabled = false;
            PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();
            return true; // END succes item z eq gracza zdjęty do plecaka

        }
        
        PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();
        return false;
    }
    public (bool result,bool update, int index) CheckWhereCanYouFitThisItemInBackpack(ItemPack _itemToStack)
    {
        
        bool IsThereAny_NonFullAndUnlocked_Slot = ItemSlots.Where(slot=>slot.IsFull == false && slot.IsLocked==false).Any();
        if(IsThereAny_NonFullAndUnlocked_Slot == false) 
        {
            return (false,false, -1);
        }

        bool IsThereAny_Empty_Slots = ItemSlots.Where(slot=>slot.IsEmpty == true).Any();
        if( _itemToStack.item.StackSettings.IsStackable == false)
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
           slot = nonEmptySlots.Where(s=>s.ITEM.item.ItemCoreSettings.ItemID == _itemToStack.item.ItemCoreSettings.ItemID && s.ITEM.item.ItemCoreSettings.Name == _itemToStack.item.ItemCoreSettings.Name)
                        .Where(s=>s.IsFull == false)
                        .FirstOrDefault();

            if(slot == null)
            {
                if(IsThereAny_Empty_Slots)
                    return (true,false, GetNextEmptySlot());
                else
                    return (false,false,-1);   
            }

            return (true,true,slot.itemSlotID);   
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
            return emptySlot.itemSlotID;
    }
}
