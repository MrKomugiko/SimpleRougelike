using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Treasure_Cell;

public class ItemSlot : MonoBehaviour
{   
    internal ChestLootScript chest;
    public bool PLAYER_BACKPACK = false;
    public int IndexID;
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
                print("item null");
                ISFull = value;
                return false;
            }

            if(ITEM.count == ITEM.item.StackSize)
            {
                print($"count {ITEM.count} == item stacksize {ITEM.item.StackSize} ");
                value = true;
            }
         
            ISFull = value;
            return value;
        }
    }
    public bool IsSelected { get; internal set; }
    
    [SerializeField] private Button _btn;
    [SerializeField] private Image _itemIcon;

    [SerializeField] private Image _emptyBackground;
    [SerializeField] private Image _lockedBackground;
    [SerializeField] private GameObject _counterBox;
    [SerializeField] private GameObject _selectionBorder;
    [SerializeField] public ItemPack ITEM;
    TextMeshProUGUI _counterBox_TMP;
    public void AddNewItemToSlot(ItemPack _item)
    {
        ITEM = _item;
        if(IsEmpty) return; 

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
            _btn.onClick.AddListener(()=>Use(ITEM));   
        }
    }
    public void MoveSinglePieceTo_Backpack()
    {
        // extract one piece
        ItemPack SinglePieceItem;
            SinglePieceItem.item = ITEM.item;
            SinglePieceItem.count = 1;

        _counterBox.SetActive(ITEM.count>1?true:false);
        (bool result,bool update, int index) slotInBackpack =  PlayerManager.PlayerInstance._mainBackpack.CheckWhereCanYouFitThisItemInBackpack(SinglePieceItem);
        
        if(slotInBackpack.result == true)
        {
            if(slotInBackpack.update)
            {
                PlayerManager.PlayerInstance._mainBackpack.ItemSlots[slotInBackpack.index].UpdateItemAmount(1);
            }
            else
            {
                PlayerManager.PlayerInstance._mainBackpack.AddSingleItemPackToBackpack(SinglePieceItem,slotInBackpack.index);
            }
            this.UpdateItemAmount(-1);
        }

        // UPDATE VALUE IN SOURCE CHEST TO PREVEENT RESPAWN CONTENT ALL OVER AGAIN
        chest.SynchronizeItemDataWithParentCell();
    }
    
    private void Use(ItemPack ITEM)
    {
        Console.WriteLine("use item"+ ITEM.item.name);
    }

    public void UpdateItemAmount(int value)
    {
        ITEM.count = ITEM.count + value;
        if(IsEmpty)
        {

            // usuwanie obrazka ze slotu
            _btn.onClick.RemoveAllListeners();
            _itemIcon.sprite = _emptyBackground.sprite;

        }

        if(ITEM.count <= 1)
        {
            // ukrycie counterboxa
            _counterBox.SetActive(false);
        }
        if(ITEM.count > 1)
        {
            _counterBox.SetActive(true);
            _counterBox_TMP.SetText(ITEM.count.ToString()); 
        }
        
    }
}
