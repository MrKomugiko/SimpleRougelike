using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Chest;

public class ItemSlot : MonoBehaviour
{   
    public EquipmentType ItemContentRestricion;
    public EquipmentScript ParentStorage;
    internal ChestLootWindowScript chest;
    public bool PLAYER_BACKPACK = false;
    public int itemSlotID;
    [SerializeField] private bool _isLocked = false;
    [SerializeField]internal bool IsLocked 
    { 
        get => _isLocked;
        set {
            _isLocked = value;
            if(value)
            {   
                _emptyBackground.gameObject.SetActive(false);
                _lockedBackground.gameObject.SetActive(true);
            }
            else
            {
                _emptyBackground.gameObject.SetActive(true);
                _lockedBackground.gameObject.SetActive(false);
            }
        } 
    }
    public bool IsEmpty {
        get{
            if(ITEM == null)
            {
                return true; // empty
            }

           return ITEM.Count == 0?true:false; // gdy liczba sztuk 0 -> empty
        }    
    }
    [SerializeField] bool ISFull;
    public bool IsFull {
        get{
            bool value = false;

            if(ITEM.item == null) 
            {
               // print("item null");
                ISFull = value;
                return false;
            }

            if(ITEM.Count == ITEM.item.StackSettings.StackSize)
            {
              //  print($"count {ITEM.count} == item stacksize {ITEM.item.ItemCoreSettings.StackSize} ");
                value = true;
            }
         
            ISFull = value;
            return value;
        }
    }
    
    [SerializeField] public Button _btn;
    [SerializeField] public Image _itemIcon;
    [SerializeField] public Image _emptyBackground;
    [SerializeField] private Image _lockedBackground;
    [SerializeField] public GameObject _counterBox;
    [SerializeField] public GameObject _selectionBorder;
    [SerializeField] private ItemPack iTEM;
    [SerializeField] private Image rarityBackgroundColor;
    public bool IsSelected => _selectionBorder.activeSelf;

    public ItemPack ITEM 
    { 
        get => iTEM; 
        set
        { 
            if(value == null)
            {
                value = new ItemPack(0,null);
            }
            iTEM = value; 
            if(value.item != null)
            {
            switch(value.item.ItemCoreSettings.Rarity)
            {
                case RarityTypes.Common:
                    rarityBackgroundColor.color = new Color32(192,191,191,155);break;
                case RarityTypes.Rare:
                    rarityBackgroundColor.color = new Color32(36,159,75,155);break;
                case RarityTypes.Epic:
                    rarityBackgroundColor.color = new Color32(35,90,165,155);break;
                case RarityTypes.Legend:
                    rarityBackgroundColor.color = new Color32(192,177,39,155);break;
                case RarityTypes.Ancient:
                    rarityBackgroundColor.color = new Color32(192,34,175,155); break;
            }
            }
            else
            {
                if(rarityBackgroundColor != null)
                    rarityBackgroundColor.color = new Color32(81,126,56,255);
            }

            if(value.Count == 0)
            {
                _btn.onClick.RemoveAllListeners();
                _itemIcon.enabled = false;
                rarityBackgroundColor.enabled = false;
            }
            else
            {
                _itemIcon.enabled = true;
                rarityBackgroundColor.enabled = true;
                _itemIcon.sprite = value.item.ItemCoreSettings.Item_Sprite;

            }

           
        }
    }
    private void ChangeItemCount(int value)
    {
        var _item = ITEM;
        _item.Count+=value;
        ITEM = _item;       
    }
    
    public TextMeshProUGUI _counterBox_TMP;
    public void AddNewItemToSlot(ItemPack _item)
    {
        ITEM = _item;
        _counterBox.SetActive(_item.Count>1?true:false);
        _counterBox_TMP = _counterBox.GetComponentInChildren<TextMeshProUGUI>();
        _counterBox_TMP.SetText(_item.Count.ToString());    

        _itemIcon.sprite = _item.item.ItemCoreSettings.Item_Sprite;   
         _btn.onClick.RemoveAllListeners();
        if(PLAYER_BACKPACK == false)
        {
            if(_item.item is GoldItem)
            {
                _btn.onClick.AddListener(()=>PickAllGoldFromSlot());   
                return;
            }

            _btn.onClick.AddListener(()=>MoveSinglePieceTo_Backpack());   
        }
        
        if(PLAYER_BACKPACK)
        {
            if(ParentStorage.StorageName == "Player") 
            {
                if(_item.item is AmmunitionItem)
                {
                    _btn.onClick.AddListener(()=>ParentStorage.AmmoManager.OpenAmmoSelectionWindow());
                    return;
                }
            }

            _btn.onClick.AddListener(()=>ShowDetailsWindow());   
        }
    }


    private void ShowDetailsWindow()
    {
        PlayerManager.instance._mainBackpack.ItemDetailsWindow
            .GetComponent<ItemDetailsWindow>()
            .SelfConfigure(ITEM.item, this);

        PlayerManager.instance._mainBackpack.ItemDetailsWindow.SetActive(true);

    }
    public void PickAllGoldFromSlot()
    {
        _counterBox.SetActive(ITEM.Count>1?true:false);

        PlayerManager.instance.AddGold(ITEM.Count);
        this.UpdateItemAmount(-ITEM.Count);
        
        chest.LootChest.SynchronizeItemDataWithParentCell();
        chest.IfEmptyRemoveEmptyChestFromMap();
    }
    public void MoveSinglePieceTo_Backpack()
    {
        ItemPack SinglePieceItem = new ItemPack(0,null);
            SinglePieceItem.item = ITEM.item;
            SinglePieceItem.Count = 1;

        _counterBox.SetActive(ITEM.Count>1?true:false);
        (bool result,bool update, int index) slotInBackpack =  
            PlayerManager.instance._mainBackpack.CheckWhereCanYouFitThisItemInBackpack(SinglePieceItem);
        
        if(slotInBackpack.result == true)
        {
            if(slotInBackpack.update)
            {
                PlayerManager.instance._mainBackpack.ItemSlots[slotInBackpack.index].UpdateItemAmount(1);
                this.UpdateItemAmount(-1);
            }
            else
            {
                if(PlayerManager.instance._mainBackpack.AddSingleItemPackToBackpack(SinglePieceItem,slotInBackpack.index) == true)
                {
                   this.UpdateItemAmount(-1);
                }
            }
        }
        chest.LootChest.SynchronizeItemDataWithParentCell();
        
        chest.IfEmptyRemoveEmptyChestFromMap();

        if(PLAYER_BACKPACK==false && ITEM.item is AmmunitionItem)
        {
            //Debug.Log("Podniesienie pierwszego itemku typu amunicja");
            AmmunitionManagerScript.RefreshAmmoDatafromBackPack();
        }
    }
    public void UpdateItemAmount(int value)
    {
        ChangeItemCount(value);
        if(IsInQuickSlot)
        {
            PlayerManager.instance._actionController.actionButtonsList[(int)AssignedToQuickSlot].UpdateItemCounter(ITEM.Count.ToString());
        }

        if(IsEmpty)
        {         
            if(IsInQuickSlot)
            {
                _counterBox.SetActive(false);
                RemoveFromQuickSlot((int)AssignedToQuickSlot);
                return;
            }
        }

        if(ITEM.Count <= 1)
        {
            _counterBox.SetActive(false);
        }
        if(ITEM.Count > 1)
        {
            _counterBox.SetActive(true);
            string countLeft = ITEM.Count.ToString();
            _counterBox_TMP.SetText(countLeft); 
        }

        if(PLAYER_BACKPACK && ITEM.item is AmmunitionItem)
        {
            //Debug.Log("zmiana ilosci itemu typu amunicja");
            AmmunitionManagerScript.RefreshAmmoDatafromBackPack();
        }
    }
    public bool IsInQuickSlot = false;
    public int? AssignedToQuickSlot = null;
    public void AssignToQuickSlot(int quickSlotID)
    {
        AssignedToQuickSlot = quickSlotID;
        IsInQuickSlot = true;

        PlayerManager.instance._actionController.actionButtonsList[quickSlotID].ButtonIcon_IMG.sprite = ITEM.item.ItemCoreSettings.Item_Sprite;
        
        PlayerManager.instance._actionController.actionButtonsList[quickSlotID].ConfigureDescriptionButtonClick(()=>(this.ITEM.item as IConsumable).Use(itemSlotID),$"{ITEM.item.ItemCoreSettings.Name}",false);
        PlayerManager.instance._actionController.actionButtonsList[quickSlotID].UpdateItemCounter(ITEM.Count.ToString());
        EquipmentScript.QuitFromQuickbarSelectionMode();
    }
    public void RemoveFromQuickSlot(int quickSlotID)
    {
        if(AssignedToQuickSlot == null)
        {
            return;
        }

        PlayerManager.instance.Reset_QuickSlotToDefault(quickSlotID);

        IsInQuickSlot = false;
        AssignedToQuickSlot = null;
    }
}

