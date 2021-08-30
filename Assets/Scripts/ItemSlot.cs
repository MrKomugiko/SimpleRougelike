using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public int ID { get; internal set; }
    [SerializeField] private bool _isLocked;
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
    public bool IsEmpty { get; internal set; }
    public bool IsFull { get; internal set; }
    public bool IsSelected { get; internal set; }
    
    [SerializeField] private Button _btn;
    [SerializeField] private Image _emptyBackground;
    [SerializeField] private Image _lockedBackground;
    [SerializeField] private GameObject _counterBox;
    [SerializeField] private GameObject _selectionBorder;

    [SerializeField] private int _itemCount;
    [SerializeField] private int _maxItemCapacity;

    List<Item> Items = new List<Item>();
    
}
