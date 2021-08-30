using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Chest;
using static Treasure_Cell;

public class EquipmentScript : MonoBehaviour
{
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
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

    public static void AssignItemToActionSlot(int slotID)
    {
        print("assign item to "+slotID+ "slot position");
    }

    public bool AddSingleItemPackToBackpack(ItemPack item, int slotIndex)
    {
        print(" attempt to populate backpack with one item");
    
        if(slotIndex != -1)
            ItemSlots[slotIndex].AddNewItemToSlot(item);
        else
            ItemSlots[slotIndex].UpdateItemAmount(1);

        return true;
    }


    public (bool result,bool update, int index) CheckWhereCanYouFitThisItemInBackpack(ItemPack _itemToStack)
    {
        print("check if is possible to stack "+_itemToStack.item.name);

        bool IsThereAny_NonFullAndUnlocked_Slot = ItemSlots.Where(slot=>slot.IsFull == false && slot.IsLocked==false).Any();
        if(IsThereAny_NonFullAndUnlocked_Slot == false) 
        {
            Debug.LogError("Brak jakiegokolwiek wolnego miejsca");
            return (false,false, -1);
        }

        bool IsThereAny_Empty_Slots = ItemSlots.Where(slot=>slot.IsEmpty == true).Any();
        if( _itemToStack.item.IsStackable == false)
        {

            Debug.LogError("item jest niestackowalny");
            if(IsThereAny_Empty_Slots == true)
            {
                Debug.LogError("dodanie itemka do nowego pustego pola");
                return (true,false, GetNextEmptySlot());
            }
            Debug.LogError("Brak wolnych slotów = FAIL");
            return (false,false, -1);
        } 
        
        Debug.LogError("Znaleziono indeks do ktorego mozna wcisnac itemek");
        var nonEmptySlots = ItemSlots.Where(slot=>slot.ITEM.item != null).ToList();
        // sprawdzenie czy zostały do dyspozycji jakieś nieprzypisane pola

        ItemSlot slot = null;
        if(nonEmptySlots.Count <= ItemSlots.Count)
        {
           slot = nonEmptySlots.Where(s=>s.ITEM.item.ItemID == _itemToStack.item.ItemID)
                        .Where(s=>s.IsFull == false)
                        .FirstOrDefault();

            if(slot == null)
            {
                print("nie znaleziono odpowiedniego slotu do wypełnienia stackowalengo itemka, zostanie on przydzielony do nowego slotu");
                if(IsThereAny_Empty_Slots)
                {
                    Debug.LogError("dodanie itemka do nowego pustego pola");
                    return (true,false, GetNextEmptySlot());
                //  przydziel do jakeigos
                }
                else
                {
                    return (false,false,-1);   
                // nie ma gdzie go przydzielic, koniec
                }
            }   
            print("znaleziono slot z itemkiem tego samego typu o indexie"+slot.IndexID);
            return (true,true,slot.IndexID);   
        }


        Debug.LogError("error, pominięto sprawdzanie");
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
