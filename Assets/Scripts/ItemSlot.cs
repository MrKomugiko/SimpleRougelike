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
    public ChestLootScript chest;
    public bool PLAYER_BACKPACK = false;
    public int IndexID;
    [SerializeField] private bool _isLocked = false;
    internal bool IsLocked 
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
    public bool IsFull => (ITEM.count == ITEM.item.StackSize || !ITEM.item.IsStackable)?true:false;
    public bool IsSelected { get; internal set; }
    
    [SerializeField] private Button _btn;
    [SerializeField] private Image _itemIcon;

    [SerializeField] private Image _emptyBackground;
    [SerializeField] private Image _lockedBackground;
    [SerializeField] private GameObject _counterBox;
    [SerializeField] private GameObject _selectionBorder;
    [SerializeField] public ItemPack ITEM;
    TextMeshProUGUI _counterBox_TMP;
    public void AddItemToSlot(ItemPack _item)
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

        // FIRST: check if you can fit this item in backpack
        if(PlayerManager.PlayerInstance._mainBackpack.CanFitInPlayerBackpack(SinglePieceItem) == false)
            return;

        ITEM.count--;
        _counterBox.SetActive(ITEM.count>1?true:false);
        PlayerManager.PlayerInstance._mainBackpack.AddSingleItemPackToBackpack(SinglePieceItem);
        
        if(IsEmpty)
        {
            // usuwanie obrazka ze slotu
            _btn.onClick.RemoveAllListeners();
            _itemIcon.sprite = _emptyBackground.sprite;
           //ITEm 
        }
        else
        {
            if(ITEM.count == 1)
            {
                // ukrycie counterboxa
                _counterBox.SetActive(false);
            }
            _counterBox_TMP.SetText(ITEM.count.ToString()); 
        }

        // UPDATE VALUE IN SOURCE CHEST TO PREVEENT RESPAWN CONTENT ALL OVER AGAIN
        chest.SynchronizeItemDataWithParentCell();
    }
    
    private void Use(ItemPack ITEM)
    {
        Console.WriteLine("use item"+ ITEM.item.name);
    }
}
