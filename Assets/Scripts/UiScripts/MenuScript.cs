using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public static MenuScript instance;
    [SerializeField] GameObject NewHero,Continue,Heroes,Options, Exit,OPTIONSWINDOW,HEROCREATORWINDOW,HEROESLISTWINDOW;
    [SerializeField] public GameObject MENU;
    [SerializeField] Color32 ButtonOff_TextColor,ButtonON_TextColor;
    Button NewHero_BTN, Continue_BTN,Heroes_BTN, Options_BTN, Exit_BTN;
    TextMeshProUGUI NewHero_TMP, Continue_TMP, Heroes_TMP,Options_TMP,Exit_TMP;

    [SerializeField] GameObject HomeCanvas;
    private void OnEnable() {
        // hide all border and selections 
        Debug.Log(Application.persistentDataPath);
        try
        {
            NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.HighlightElementSwitch(n,false));    
        }
        catch (System.Exception)
        {
            
        }
    }
    private void Awake() 
    {
        instance = this;
        
        MENU.SetActive(true);

        NewHero_BTN = NewHero.GetComponent<Button>();
            NewHero_BTN.interactable = true;
        NewHero_TMP = NewHero.GetComponentInChildren<TextMeshProUGUI>();
            NewHero_TMP.color = ButtonON_TextColor;

        //TODO: jezeli okno pojawilo sie poprzez kliknięcie przycisku X, kontynuuj zamyka tylko te okno 
        Continue_BTN = Continue.GetComponent<Button>();
            Continue_BTN.interactable = false;      
        Continue_TMP = Continue.GetComponentInChildren<TextMeshProUGUI>();
            Continue_TMP.color = ButtonOff_TextColor;      

        // TODO: aktywne jezeli w bazie widnieje jakikolwiek wpis bohatera
        Heroes_BTN = Heroes.GetComponent<Button>();
            Heroes_BTN.interactable = false;        
        Heroes_TMP = Heroes.GetComponentInChildren<TextMeshProUGUI>();
            Heroes_TMP.color = ButtonOff_TextColor;        

        //TODO: opcje, sam jeszcze neiw iem czy sie przydadza teraz xd
        Options_BTN = Options.GetComponent<Button>();
            Options_BTN.interactable = true;        
        Options_TMP = Options.GetComponentInChildren<TextMeshProUGUI>();
            Options_TMP.color = ButtonON_TextColor;        

        Exit_BTN = Exit.GetComponent<Button>();
            Exit_BTN.interactable = true;      
        Exit_TMP = Exit.GetComponentInChildren<TextMeshProUGUI>();
            Exit_TMP.color = ButtonON_TextColor;      
    }

    public void OnClick_NewHero()
    {
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
    public void OnClick_Back()
    {
        HEROESLISTWINDOW.SetActive(false);
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
