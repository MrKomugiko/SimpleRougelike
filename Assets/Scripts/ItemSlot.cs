using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Chest;

public class ItemSlot : MonoBehaviour
{   
    public ITEMTYPES ItemContentRestricion = ITEMTYPES.none;
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
    public bool IsEmpty => ITEM.count == 0?true:false;
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

            if(ITEM.count == ITEM.item.StackSize)
            {
              //  print($"count {ITEM.count} == item stacksize {ITEM.item.StackSize} ");
                value = true;
            }
         
            ISFull = value;
            return value;
        }
    }
    
    [SerializeField] public Button _btn;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _emptyBackground;
    [SerializeField] private Image _lockedBackground;
    [SerializeField] private GameObject _counterBox;
    [SerializeField] public GameObject _selectionBorder;
    [SerializeField] public ItemPack ITEM;
    public bool IsSelected => _selectionBorder.activeSelf;
    TextMeshProUGUI _counterBox_TMP;
    public void AddNewItemToSlot(ItemPack _item)
    {
        ITEM = _item;
        // if(IsEmpty) return; 

        _counterBox.SetActive(_item.count>1?true:false);
        _counterBox_TMP = _counterBox.GetComponentInChildren<TextMeshProUGUI>();
        _counterBox_TMP.SetText(_item.count.ToString());    

        _itemIcon.sprite = _item.item.Item_Sprite;   
         _btn.onClick.RemoveAllListeners();
        if(PLAYER_BACKPACK == false)
        {
            _btn.onClick.AddListener(()=>MoveSinglePieceTo_Backpack());   
        }
        
        if(PLAYER_BACKPACK)
        {
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
    public void MoveSinglePieceTo_Backpack()
    {
        // extract one piece
        ItemPack SinglePieceItem;
            SinglePieceItem.item = ITEM.item;
            SinglePieceItem.count = 1;

        _counterBox.SetActive(ITEM.count>1?true:false);
        (bool result,bool update, int index) slotInBackpack =  PlayerManager.instance._mainBackpack.CheckWhereCanYouFitThisItemInBackpack(SinglePieceItem);
        
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
        // UPDATE VALUE IN SOURCE CHEST TO PREVEENT RESPAWN CONTENT ALL OVER AGAIN
        chest.LootChest.SynchronizeItemDataWithParentCell();
        
        // CLOSE CHEST IF LEFT EMPTY
        chest.IfEmptyRemoveEmptyChestFromMap();
    }
    public void UpdateItemAmount(int value)
    {
        ITEM.count = ITEM.count + value;
        if(IsEmpty)
        {         
             // usuwanie obrazka ze slotu
            _btn.onClick.RemoveAllListeners();
            _itemIcon.sprite = _emptyBackground.sprite;
            print("itemek jest pusty");
            if(IsInQuickSlot)
            {
                print("item jest podpięty pod quickslot, zostanie od niego usunięty");
                _counterBox.SetActive(false);
                RemoveFromQuickSlot((int)AssignedToQuickSlot);
                return;
            }
            Debug.Log("usunięcie itemku:"+ITEM.item.Name);
            ITEM.item = null;
        }

        if(ITEM.count <= 1)
        {
            // ukrycie counterboxa
            _counterBox.SetActive(false);
        }
        if(ITEM.count > 1)
        {
            _counterBox.SetActive(true);
            string countLeft = ITEM.count.ToString();
            _counterBox_TMP.SetText(countLeft); 
            if(IsInQuickSlot)
            {
                PlayerManager.instance._actionController.actionButtonsList[(int)AssignedToQuickSlot].UpdateItemCounter(countLeft);
            }
        }
        
    }
    public bool IsInQuickSlot = false;
    public int? AssignedToQuickSlot = null;
    public void AssignToQuickSlot(int quickSlotID)
    {
        AssignedToQuickSlot = quickSlotID;
        IsInQuickSlot = true;
        print("Assign to Quick Slot nr."+AssignedToQuickSlot);

        PlayerManager.instance._actionController.actionButtonsList[quickSlotID].ButtonIcon_IMG.sprite = ITEM.item.Item_Sprite;
        
        PlayerManager.instance._actionController.actionButtonsList[quickSlotID].ConfigureDescriptionButtonClick(()=>(this.ITEM.item as IConsumable).Use(itemSlotID),$"{ITEM.item.Name}",false);
        // przywróć eq do stanu przed wyboru tego itemka do slotu
        EquipmentScript.QuitFromQuickbarSelectionMode();
    }
    public void RemoveFromQuickSlot(int quickSlotID)
    {
        print("Remove from Quick Slot");
        if(AssignedToQuickSlot == null)
        {
            Debug.LogError("ERROR, proba usuniecia itemu z quick slota, mimo ze nic nie jest do niego przypięte");
            return;
        }

        PlayerManager.instance.Reset_QuickSlotToDefault(quickSlotID);

        IsInQuickSlot = false;
        AssignedToQuickSlot = null;
    }

    public enum ITEMTYPES
    {
        none,
        Armor,
        Shoulders,
        PrimaryWeapon,
        SecondaryWeapon,
        Shoes,
        Gloves,
        Helmet,
        Belt,
    }
}

