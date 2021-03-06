using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimateWindowScript : MonoBehaviour
{
    public static AnimateWindowScript instance;
    private void Awake() {
        instance = this;
    }
    [SerializeField] RectTransform _transform;
    [SerializeField] Vector3 _position_OFF;
    [SerializeField] Vector3 _position_ON;
    [SerializeField] int _slideSpeed;
    [SerializeField] List<Button> tabList = new List<Button>();
    [SerializeField] List<GameObject> contentList = new List<GameObject>();
    [SerializeField] private bool Active = false;

    private Coroutine routineInProgress = null;
    
    public void MoveWindow()
    {
        if(routineInProgress == null)
        {
            routineInProgress = StartCoroutine(SlideAnimation());
        }
        else
        {
            //print("trwa juz animacja ruchu okna");
        }
    }
    public void HideTabWindow()
    {
        _transform.anchoredPosition = _position_OFF;
        if(CurrentOpenedTab == "CharacterTab") 
            CharacterEquipmentWindow.SetActive(false);

        CurrentOpenedTab = "";
        Active = false;
        _DarkBackground.SetActive(false);

    }
    [SerializeField] GameObject CharacterEquipmentWindow;
    private IEnumerator SlideAnimation()
    {
      //  HideNotificationElementsExceptPlayer(_active);

        var bgImg = _DarkBackground.GetComponent<Image>();
        Color32 colorstart = new Color32(0, 0, 0, 200);
        Color32 colorend = Color.clear;

        Vector3 start = Vector3.zero, end = Vector3.zero;
        if (_active)
        {

            start = _position_ON;
            end = _position_OFF;

            colorstart = new Color32(0, 0, 0, 200);
            colorend = Color.clear;
        }
        if (!_active)
        {
            _DarkBackground.SetActive(true);
            start = _position_OFF;
            end = _position_ON;

            colorstart = Color.clear;
            colorend = new Color32(0, 0, 0, 200);
        }

        float progress = 1f / _slideSpeed;
        // print(_transform.anchoredPosition+" / start / "+start);
        for (float i = 0; i < 1.1f; i += progress)
        {
            // print(i);
            _transform.anchoredPosition = Vector3.Lerp(start, end, i);
            bgImg.color = Color32.Lerp(colorstart, colorend, i);

            yield return new WaitForFixedUpdate();
        }
        // print(_transform.anchoredPosition+" / end / "+end);

        _active = !_active;
        if (_active == false)
            _DarkBackground.SetActive(false);

        routineInProgress = null;
        yield return null;
    }
    private void HideNotificationElementsExceptPlayer(bool value)
    {
        // and show only first notification => pla
        // make background bark transparent overlay on , 
        foreach (var noti in transform.parent.GetComponentInChildren<NotificationManger>().NotificationList)
        {
            if (noti.BaseCell.Type != TileTypes.player)
                noti.gameObject.SetActive(value);
        }
    }
    public void MinimalizeCurrentlyOpenedWindow()
    {
        SwitchTab(CurrentOpenedTab);
    }
    public string CurrentOpenedTab = "";
    public void SwitchTab(string tabName)
    {
        if(CurrentOpenedTab == "")
        {
            if(tabName == "CharacterTab") 
                CharacterEquipmentWindow.SetActive(true);
           // print("zadna zakladka nie by??a widoczna, otwieramy okno");
           // print("otwarta zak??adka "+tabName);
            MoveWindow();
            LoadTabData(tabName);
        }
        else if(CurrentOpenedTab == tabName)
        {
            if(CurrentOpenedTab == "CharacterTab") 
              CharacterEquipmentWindow.SetActive(false);
           // print("ponowne wybranie tej samej zak??adki = zamkni??cie okna");
            foreach(var tab in tabList)
            {
                tab.interactable = true;
            }
            tabName = "";
            MoveWindow();

            // gdy zminimalizowany, wszytskie zakladki sa "aktywne"
            foreach(var tab in tabList)
                ChangeTabButtonState(tab.name, true);

        }
        else
        {
           // print("prze????czono zak??adk?? na "+tabName);
            if(tabName == "CharacterTab") 
              CharacterEquipmentWindow.SetActive(true);
            LoadTabData(tabName);
        }

        CurrentOpenedTab = tabName;

    }
    [SerializeField] GameObject Content;
    [SerializeField] GameObject _DarkBackground;
    public bool _active { 
        get => Active; 
        set 
        {
            Active = value; 

            
        } 
    }

    private void LoadTabData(string tabname)
    {
        foreach(var tab in tabList)
            ChangeTabButtonState(tab.name, tab.name==tabname?true:false);
        
        contentList.ForEach(c=>c.SetActive(false));
        Content = contentList.Where(c=>c.name == "Content_"+tabname).First();
        Content.SetActive(true);

    }

    private void ChangeTabButtonState(string tabName, bool isActive)
    {
        var tab = tabList.Where(tab=>tab.name == tabName).First();

        if(isActive)
        {
            tab.GetComponent<Image>().color = new Color32(255,255,255,255);
            tab.transform.Find("icon").GetComponent<Image>().color = new Color32(255,255,255,255);
            tab.transform.Find("label").GetComponent<TextMeshProUGUI>().color = new Color32(213,230,145,255);
        }
        if(! isActive)
        {
            tab.GetComponent<Image>().color = new Color32(255,255,255,128);
            tab.transform.Find("icon").GetComponent<Image>().color = new Color32(255,255,255,128);
            tab.transform.Find("label").GetComponent<TextMeshProUGUI>().color = new Color32(213,230,145,128);
        }
    }
}
