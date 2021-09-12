using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{

    public static MenuScript instance;
    [SerializeField] GameObject NewHero,Continue,Heroes,Options, Exit,OPTIONSWINDOW,HEROESLISTWINDOW;
    [SerializeField] public GameObject MENU,HEROCREATORWINDOW;
    [SerializeField] Color32 ButtonOff_TextColor,ButtonON_TextColor;
    public TextMeshProUGUI HEROCREATORWINDOW_ErrorTMP;
    [SerializeField] GameObject HomeCanvas;
    [SerializeField] public Button HEROESLISTWINDOW_RemoveConfirmButton;

    private void Start() 
    {
    
        instance = this;

        if(HeroDataController.instance.storedHeroesCard.Count > 0)
        {
            Heroes.SetActive(true);
                SetButtonState("Heroes",true); 
            NewHero.SetActive(false);
                SetButtonState("NewHero");  
        }
        else
        {
            Heroes.SetActive(false);
                SetButtonState("Heroes");    
            NewHero.SetActive(true);
                SetButtonState("NewHero",true);  
        }

        MENU.SetActive(true);

        SetButtonState("Continue"); 
        SetButtonState("Options",true);        
        SetButtonState("Exit",true);      
    }

    internal void SetButtonState(string buttonName, bool isactive=false)
    {
        Button BTN = null;
        TextMeshProUGUI TMP = null;

        switch(buttonName)
        {
            case "NewHero":
                BTN = NewHero.GetComponent<Button>();
                TMP = NewHero.GetComponentInChildren<TextMeshProUGUI>();
                break;

            case "Exit":
                BTN = Exit.GetComponent<Button>();
                TMP = Exit.GetComponentInChildren<TextMeshProUGUI>();
                break;

            case "Options":
                BTN = Options.GetComponent<Button>();
                TMP =  Options.GetComponentInChildren<TextMeshProUGUI>();
                break;

            case "Continue":
                BTN = Continue.GetComponent<Button>();
                TMP =  Continue.GetComponentInChildren<TextMeshProUGUI>();
                break;

            case "Heroes":
                BTN = Heroes.GetComponent<Button>();
                TMP = Heroes.GetComponentInChildren<TextMeshProUGUI>();
                break;
        }

        BTN.interactable = isactive;
        TMP.color = isactive?ButtonON_TextColor:ButtonOff_TextColor;
    }


   
    public void OnClick_NewHero(int slotID)
    {
        HEROESLISTWINDOW.SetActive(false);
        HeroDataController.instance.CreateHeroButton.onClick.RemoveAllListeners();
        HeroDataController.instance.CreateHeroButton.onClick.AddListener(()=>HeroDataController.instance.CreateNewHero(slotID));
        
        HEROCREATORWINDOW.SetActive(true);
        Options.SetActive(false);
        Exit.SetActive(false);
        Continue.SetActive(false);
        Heroes.SetActive(false);
    }
    public void OnClick_Continue()
    {
        // TODO: Continue Button
        MENU.SetActive(false);
    }
    
    public void OpenGameScene()
    {
        // TODO:
        HEROESLISTWINDOW.SetActive(false);
        HEROCREATORWINDOW.SetActive(false);
        MENU.SetActive(false);
    }
    public void PauseMenu()
    {
        
        if(HeroDataController.instance.storedHeroesCard.Count > 0)
        {
            SetButtonState("Heroes",true);   
        }
        else
        {
             SetButtonState("Heroes");    
        }
        OnClick_Back();

        SetButtonState("Continue",true); 
    }
    public void OnClick_Back()
    {
        MENU.SetActive(true);
        HEROESLISTWINDOW.SetActive(HEROCREATORWINDOW.gameObject.activeSelf?true:false);
        HEROCREATORWINDOW.SetActive(false);
        OPTIONSWINDOW.SetActive(false);

        NewHero.SetActive(true);
        Options.SetActive(true);
        Exit.SetActive(true);
        Continue.SetActive(true);
        Heroes.SetActive(true);
        
    }
    public void OnClick_Heroes()
    {
        
        HeroDataController.instance.LoadHeroesDataFromDevice();
        HEROESLISTWINDOW.SetActive(true);
    }
    public void OnClick_Options()
    {
        //TODO: Options Button
        OPTIONSWINDOW.SetActive(true);
    }
    public void OnClick_Exit()
    {
        Application.Quit();
    }

}
