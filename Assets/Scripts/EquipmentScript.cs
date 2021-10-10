using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Chest;
using static Treasure_Cell;

public class EquipmentScript : MonoBehaviour
{
    public AmmunitionManagerScript AmmoManager;
    public string StorageName;
    [SerializeField] public bool PLAYER_EQUIPMENTSLOT;
    [SerializeField] GameObject ItemSlotPrefab;
    [SerializeField] int MaxCapacity;
    [SerializeField] int NumberOfUnlockedSlots;
    [SerializeField] GameObject ItemsContainer;
    [SerializeField] public GameObject ItemDetailsWindow;
    [SerializeField] public List<ItemSlot> ItemSlots = new List<ItemSlot>();

    private void OnEnable() {   
        if(PLAYER_EQUIPMENTSLOT)
        {   
            this.transform.Find("NickName").GetComponentInChildren<TextMeshProUGUI>().SetText(GameManager.instance.PLAYER_PROGRESS_DATA.NickName);
        }
    }
    public void GenerateEquipment() 
    {
        if(PLAYER_EQUIPMENTSLOT == false)
        { 
            if(ItemSlots.Count>0)
            {
                foreach(var slot in ItemSlots)
                {
                    slot.ITEM = null;
                    slot.UpdateItemAmount(0);
                }
            }
            else
            {
                for(int i = 0; i< MaxCapacity; i++)
                {
                    StorageName = "Backpack";
                    ItemSlot itemSlot = Instantiate(ItemSlotPrefab, ItemsContainer.transform).GetComponent<ItemSlot>();
                    ItemSlots.Add(itemSlot);
                    itemSlot.PLAYER_BACKPACK = true;
                    itemSlot.itemSlotID = i;
                    itemSlot.IsLocked = i < NumberOfUnlockedSlots?false:true;
                    itemSlot.ParentStorage = this;
                    itemSlot.IsInQuickSlot = false;
                }
            }
        }

        if(PLAYER_EQUIPMENTSLOT)
        {   
            StorageName = "Player";
            ItemSlots.ForEach(slot=>slot.ParentStorage = this);
            ItemSlots.ForEach(slot=>slot.PLAYER_BACKPACK = true);
            PlayerManager.instance.LoadPlayerItensAndEq(GameManager.instance.PLAYER_PROGRESS_DATA,"PlayerEQ");
        }
        else
        {
            PlayerManager.instance.LoadPlayerItensAndEq(GameManager.instance.PLAYER_PROGRESS_DATA,"MainBackpack");
        }
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
            if(itemSlot.ITEM.item == null || itemSlot.ITEM.Count == 0) continue;

            if(itemSlot.ITEM.item.CanBeAssignToQuickActions == false || itemSlot.IsInQuickSlot)
            {
                turnOffButtonsList.Add(itemSlot._btn);  
                itemSlot._btn.interactable = false;
            }
            else
            {
                itemSlot._btn.interactable = true;
                selectedItemsForQuickslot.Add(itemSlot);
                itemSlot._selectionBorder.SetActive(true);
                itemSlot._selectionBorder.GetComponent<Button>().onClick.AddListener(()=>itemSlot.AssignToQuickSlot(quickslotID));
            }
        }
    }

    public void TakeItemFromBackpack(int _takeCount, ItemData _findingItem)
    {
        // ściągniecie ze stanu wybranego itemka z pierwszego dostepnego slotu do wyczerpania zasobu
        var slotWithItem = ItemSlots.Where(slot=>slot.ITEM.item == _findingItem && slot.ITEM.Count != 0).First();
        slotWithItem.UpdateItemAmount(-_takeCount);
     
        Debug.Log("uzyto "+_takeCount+ " "+_findingItem.ItemCoreSettings.Name);
    }

    public static void QuitFromQuickbarSelectionMode()
    {
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
        int avaiableSpace = ItemSlots.Count - ItemSlots.Where(s=>s.IsLocked).Count();
        if(avaiableSpace-1 < slotIndex) 
        {
            return false;
        }
        if(slotIndex != -1)
            ItemSlots[slotIndex].AddNewItemToSlot(item);
        else
        {
                return false;
        }
        return true;
    } 
    public void LoadItemInPlayerEq( int asociatedSlotId, ItemPack _item )
    {
        EquipmentItem itemEq = _item.item as EquipmentItem;
        PlayerManager.instance.STATS.EquipItem_UpdateStatistics(itemEq);
        this.ItemSlots[asociatedSlotId].AddNewItemToSlot(_item);
        
        if(_item.item.ItemCoreSettings.Name == "Roma Helmet")  
            PlayerManager.instance.HelmetIMG.enabled = true;
        
        if(_item.item.ItemCoreSettings.Name == "Roma Armor")
            PlayerManager.instance.ArmorIMG.enabled = true;
    }
    public EquipmentItem GetEquipmentItemFromSlotType(EquipmentType eqType)
    {
        int slotIndex  = this.ItemSlots.Where(s=>s.ItemContentRestricion == eqType).First().itemSlotID;
        var equipmentItem = ItemSlots[slotIndex].ITEM.Count == 0?null:ItemSlots[slotIndex].ITEM.item as EquipmentItem;
        return equipmentItem;
    }
    public static ItemSlot GetPlayerEqSlotByEqtype(EquipmentType eqType) => PlayerManager.instance._EquipedItems.ItemSlots.Where(s=>s.ItemContentRestricion == eqType).FirstOrDefault();

    public List<ItemPack> GetSumListAvailableAmmunition()
    {
        var ammunitionList = new List<ItemPack>();

        foreach(var slot in ItemSlots.Where(i => i.ITEM.Count > 0))
        {
            int count = slot.ITEM.Count;
            var existingAmmoRecord = ammunitionList.Where(a=>a.item.name == slot.ITEM.item.name).FirstOrDefault();
            if(existingAmmoRecord != null)
            {
                ammunitionList.First(a=>a == existingAmmoRecord).Count += count;
            }
            else
            {
                // brak aktualnego wpisu itemku na liscie, dodanie nowego
                if( slot.ITEM.item is AmmunitionItem )
                {
                    if(slot.ITEM.Count > 0)
                        ammunitionList.Add(new ItemPack(slot.ITEM.Count, slot.ITEM.item));
                }
            }
        }

        return ammunitionList; 
    }
    public bool EquipItemFromSlot(ItemSlot fromSlot, EquipmentScript toEquipment)
    {
        bool _equipItem = toEquipment.StorageName == "Player";
        var equipmentItem = fromSlot.ITEM.item as EquipmentItem;
        ItemPack ItemCopy = new ItemPack(fromSlot.ITEM.Count,fromSlot.ITEM.item);
        if(_equipItem)
        {
            if(fromSlot.ITEM.item.CheckRequirments() == false) return false;

            if(equipmentItem.eqType == EquipmentType.SecondaryWeapon || equipmentItem.eqType == EquipmentType.PrimaryWeapon)
            {
                bool synergyunquipcheck = CheckWeaponsTypeSynergies(equipmentItem.EquipmentSpecificType);
                if(synergyunquipcheck == false)
                    return false;
            }

            int matchEqSlotIndex = toEquipment.ItemSlots.Where(s=>s.ItemContentRestricion == equipmentItem.eqType).First().itemSlotID;

            if(toEquipment.ItemSlots[matchEqSlotIndex].ITEM.Count == 0)
            {
                this.ItemSlots[fromSlot.itemSlotID].UpdateItemAmount(-1);
                toEquipment.ItemSlots[(int)matchEqSlotIndex].AddNewItemToSlot(ItemCopy);
            }
            else 
            {
                ItemPack currentEquipedItemCopy = new ItemPack(toEquipment.ItemSlots[matchEqSlotIndex].ITEM.Count,toEquipment.ItemSlots[matchEqSlotIndex].ITEM.item);
                toEquipment.ItemSlots[matchEqSlotIndex].UpdateItemAmount(-1);
                PlayerManager.instance.STATS.UnequipItem_UpdateStatistics(currentEquipedItemCopy.item as EquipmentItem);   
                this.ItemSlots[fromSlot.itemSlotID].UpdateItemAmount(-1);
                this.ItemSlots[fromSlot.itemSlotID].AddNewItemToSlot(currentEquipedItemCopy);
                toEquipment.ItemSlots[(int)matchEqSlotIndex].AddNewItemToSlot(ItemCopy);
            }

            return true;
        }
        
        if(_equipItem == false) // zdejmowanie
        {
            if(toEquipment.GetNextEmptySlot() != -1)
            {
                this.ItemSlots[fromSlot.itemSlotID].UpdateItemAmount(-1);
            }
            else
            {
                return false;
            }
            toEquipment.ItemSlots[toEquipment.GetNextEmptySlot()].AddNewItemToSlot(ItemCopy);
            
            return true; // END succes item z eq gracza zdjęty do plecaka
        }
        return false;
    }

    public bool CheckWeaponsTypeSynergies(EquipmentSpecifiedType recentEquippedSpecificType)
    {
        EquipmentItem PrimaryWeapon_Item= default;
        EquipmentItem SecondaryWeapon_Item = default;

        bool isAvailableExtraSlotInCaseOfUnequip = PlayerManager.instance._mainBackpack.GetNextEmptySlot() != -1;

        var CurrentPrimaryWeapon_Slot = GetPlayerEqSlotByEqtype(EquipmentType.PrimaryWeapon);
        if(CurrentPrimaryWeapon_Slot != null )
        {
            if(CurrentPrimaryWeapon_Slot.ITEM.Count >0)
                PrimaryWeapon_Item = (CurrentPrimaryWeapon_Slot.ITEM.item as EquipmentItem);
            else
                PrimaryWeapon_Item = null;
        }

        var CurrentSecondaryWeapon_Slot = GetPlayerEqSlotByEqtype(EquipmentType.SecondaryWeapon);
        if(CurrentSecondaryWeapon_Slot != null)
        {
            if(CurrentSecondaryWeapon_Slot.ITEM.Count >0)
                SecondaryWeapon_Item = (CurrentSecondaryWeapon_Slot.ITEM.item as EquipmentItem);
            else
                SecondaryWeapon_Item = null;
        }
        if(recentEquippedSpecificType == EquipmentSpecifiedType.Sword)
        {
            if(SecondaryWeapon_Item == null)
                return true; // jest git, nie ma zalozonej pobocznej broni, nie ma co sprawdzac
            
            if(SecondaryWeapon_Item.EquipmentSpecificType == EquipmentSpecifiedType.NoRestriction || SecondaryWeapon_Item.EquipmentSpecificType == EquipmentSpecifiedType.Shield )
                return true; // miecz moze wystąpić tylko gdy poboczna bron to nic lub tarcza
    
            // w przeciwnym wypadku, konieczne jest zdjęcie pobocznej broni
            if(isAvailableExtraSlotInCaseOfUnequip)
            {
                SecondaryWeapon_Item.Equip(CurrentSecondaryWeapon_Slot);
                return true;
            }
            else
            {   
                return false;
            }
        }
        if(recentEquippedSpecificType == EquipmentSpecifiedType.Bow)
        {
            if(SecondaryWeapon_Item == null)
                return true; // jest git, nie ma zalozonej pobocznej broni, nie ma co sprawdzac
            
            if(SecondaryWeapon_Item.EquipmentSpecificType == EquipmentSpecifiedType.NoRestriction)
                return true; // miecz moze wystąpić tylko gdy poboczna bron to nic lub tarcza
    
            // w przeciwnym wypadku, konieczne jest zdjęcie pobocznej broni
            if(isAvailableExtraSlotInCaseOfUnequip)
            {
                SecondaryWeapon_Item.Equip(CurrentSecondaryWeapon_Slot);
                return true;
            }
            else
            {   
                return false;
            }
        }
        if(recentEquippedSpecificType == EquipmentSpecifiedType.Shield)
        {
            if(PrimaryWeapon_Item == null)
                return true; // jest git, nie ma zalozonej głównej broni, nie ma co sprawdzac
            
            if(PrimaryWeapon_Item.EquipmentSpecificType == EquipmentSpecifiedType.NoRestriction || PrimaryWeapon_Item.EquipmentSpecificType == EquipmentSpecifiedType.Sword )
                return true; // tarcza moze wystąpić tylko gdy głowna bron to sword lub nic
    
            // w przeciwnym wypadku, konieczne jest zdjęcie aktualnie zaloznej glownej broni tj łuk lub rozdzka
            if(isAvailableExtraSlotInCaseOfUnequip)
            {
                PrimaryWeapon_Item.Equip(CurrentPrimaryWeapon_Slot);
                return true;
            }
            else
            {   
                return false;
            }
        }    

        // reszta eq nie ma ograniczen na przebywanie miedzy sobą
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
    
            if(slot.IsEmpty == true)
            {
                return (true,false,slot.itemSlotID);   
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

    public List<ItemBackupData> GetBackupListOfItemsAndSlots()
    {
        List<ItemBackupData> _backup = new List<ItemBackupData>();

        foreach(var slot in ItemSlots)
        {
            if(slot.ITEM.item == null || slot.ITEM.Count == 0) continue;
            _backup.Add(
                new ItemBackupData(
                    slot.itemSlotID,
                    slot.ITEM.Count,
                    slot.ITEM.item.name
                    )
                );
        }
        return _backup;
    }

    public ItemBackupData GetItemsAssignedToQuickslot(int quickSlotID)
    {
        ItemBackupData _quickslotbackup = null;

        foreach(var slot in ItemSlots)
        {
            if(slot.IsInQuickSlot)
            {
                if(slot.AssignedToQuickSlot != null)
                {
                    if((int)slot.AssignedToQuickSlot == quickSlotID)
                    {  
                        if(slot.ITEM.item == null || slot.ITEM.Count == 0) continue;
                    
                        _quickslotbackup = new ItemBackupData(
                            slot.itemSlotID,
                            slot.ITEM.Count,
                            slot.ITEM.item.name
                            );

                            Debug.LogWarning($"zapisano z quickslta [{quickSlotID}] = item z backpacka [{_quickslotbackup.SlotID}]");
                    }
                }
            }
        }
        
        return _quickslotbackup;
    }

    [Serializable]
    public class ItemBackupData
    {
        public int SlotID;
        public int Count;
        public string ScriptableObjectName;

        public ItemBackupData(int slotID, int count, string scriptableObjectName)
        {
            SlotID = slotID;
            Count = count;
            ScriptableObjectName = scriptableObjectName;
        }
    }
}
