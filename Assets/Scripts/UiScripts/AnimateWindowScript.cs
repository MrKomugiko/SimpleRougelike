using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimateWindowScript : MonoBehaviour
{
    [SerializeField] RectTransform _transform;
    [SerializeField] Vector3 _position_OFF;
    [SerializeField] Vector3 _position_ON;
    [SerializeField] int _slideSpeed;
    [SerializeField] List<Button> tabList = new List<Button>();
    [SerializeField] private bool Active = false;

    private Coroutine routineInProgress = null;
    
    private void MoveWindow()
    {
        if(routineInProgress == null)
        {
            routineInProgress = StartCoroutine(SlideAnimation());
        }
        else
        {
            print("trwa juz animacja ruchu okna");
        }
    }
    private IEnumerator SlideAnimation()
    {
        Vector3 start = Vector3.zero, end=Vector3.zero;
        if(Active)
        {
            start = _position_ON;
            end = _position_OFF;
        }
        if(! Active)
        {
            start = _position_OFF;
            end = _position_ON;
        }

        float progress = 1f/_slideSpeed;
        // print(_transform.anchoredPosition+" / start / "+start);
         for (float i = 0; i < 1.1f; i+=progress)
        {
           // print(i);
            _transform.anchoredPosition = Vector3.Lerp(start,end,i);
            yield return new WaitForFixedUpdate();
        }
        // print(_transform.anchoredPosition+" / end / "+end);
    
        Active =! Active;
        routineInProgress = null;
        yield return null;
    }

    public string CurrentOpenedTab = "";
    public void SwitchTab(string tabName)
    {
        if(CurrentOpenedTab == "")
        {
           // print("zadna zakladka nie była widoczna, otwieramy okno");
           // print("otwarta zakładka "+tabName);
            MoveWindow();
            LoadTabData(tabName);
        }
        else if(CurrentOpenedTab == tabName)
        {
           // print("ponowne wybranie tej samej zakładki = zamknięcie okna");
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
           // print("przełączono zakładkę na "+tabName);
            LoadTabData(tabName);
        }

        CurrentOpenedTab = tabName;

    }
    [SerializeField] GameObject Content;
    private void LoadTabData(string tabname)
    {
        foreach(var tab in tabList)
            ChangeTabButtonState(tab.name, tab.name==tabname?true:false);
        
        Content.GetComponentInChildren<TextMeshProUGUI>().SetText(tabname);
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
