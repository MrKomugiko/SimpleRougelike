using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField] GameObject NewGame;
        Button NewGame_BTN;
        TextMeshProUGUI NewGame_TMP;
    [SerializeField] GameObject Continue;
        Button Continue_BTN;
        TextMeshProUGUI Continue_TMP;
    [SerializeField] GameObject Save;
        Button Save_BTN;
        TextMeshProUGUI Save_TMP;
    [SerializeField] GameObject Options;
        Button Options_BTN;
        TextMeshProUGUI Options_TMP;
    [SerializeField] GameObject Exit;
        Button Exit_BTN;
        TextMeshProUGUI Exit_TMP;


    [SerializeField] Color32 ButtonOff_TextColor;
    [SerializeField] Color32 ButtonON_TextColor;

    [SerializeField] GameObject MENU;

    private void Start() 
    {
        MENU = this.gameObject;
        MENU.SetActive(true);

        NewGame_BTN = NewGame.GetComponent<Button>();
            NewGame_BTN.interactable = true;
        NewGame_TMP = NewGame.GetComponentInChildren<TextMeshProUGUI>();
            NewGame_TMP.color = ButtonON_TextColor;

        Continue_BTN = Continue.GetComponent<Button>();
            Continue_BTN.interactable = false;      
        Continue_TMP = Continue.GetComponentInChildren<TextMeshProUGUI>();
            Continue_TMP.color = ButtonOff_TextColor;      

        Save_BTN = Save.GetComponent<Button>();
            Save_BTN.interactable = false;        
        Save_TMP = Save.GetComponentInChildren<TextMeshProUGUI>();
            Save_TMP.color = ButtonOff_TextColor;        

        Options_BTN = Options.GetComponent<Button>();
            Options_BTN.interactable = false;        
        Options_TMP = Options.GetComponentInChildren<TextMeshProUGUI>();
            Options_TMP.color = ButtonOff_TextColor;        

        Exit_BTN = Exit.GetComponent<Button>();
            Exit_BTN.interactable = true;      
        Exit_TMP = Exit.GetComponentInChildren<TextMeshProUGUI>();
            Exit_TMP.color = ButtonON_TextColor;      
    }

    public void OnClick_NewGame()
    {
        MENU.SetActive(false);

        Continue_BTN.interactable = true;
        Continue_TMP.color = ButtonON_TextColor;

        GameManager.Restart();
    }
    public void OnClick_Continue()
    {
        // TODO: Continue Button
        MENU.SetActive(false);
    }
    public void OnClick_Save()
    {
        // TODO: Save Button
    }
    public void OnClick_Options()
    {
        //TODO: Options Button
    }
    public void OnClick_Exit()
    {
        Application.Quit();
    }

}