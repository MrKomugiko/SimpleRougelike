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
    Button NewHero_BTN, Continue_BTN,Heroes_BTN, Options_BTN, Exit_BTN;
    TextMeshProUGUI NewHero_TMP, Continue_TMP, Heroes_TMP,Options_TMP,Exit_TMP;
    public TextMeshProUGUI HEROCREATORWINDOW_ErrorTMP;
    [SerializeField] GameObject HomeCanvas;
    [SerializeField] public Button HEROESLISTWINDOW_RemoveConfirmButton;
    // private void OnEnable() {
    //     try
    //     {
    //         NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.HighlightElementSwitch(n,false));    
    //     }
    //     catch (System.Exception)
    //     {
            
    //     }
    // }
    private void Awake() 
    {
        instance = this;
        LoadHeroesDataFromDevice();
        if(storedHeroesCard.Count > 0)
        {
            SetBtunState("Heroes",true);  
        }
        else
        {
           SetBtunState("Heroes");      
        }

        MENU.SetActive(true);

        SetBtunState("NewHero",true); 
        SetBtunState("Continue"); 
        SetBtunState("Options",true);        
        SetBtunState("Exit",true);      
    }

    internal void SetBtunState(string buttonName, bool isactive=false)
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

    [SerializeField] GameObject LoginHeroCardPrefab;
    [SerializeField] GameObject HerolistContainer;
    public List<HeroLoadCard> storedHeroesCard = new List<HeroLoadCard>();
    private void LoadHeroesDataFromDevice()
    {
        var storedHeroes = Directory.GetFiles(Application.persistentDataPath);
        foreach(var hero in storedHeroes)
        {
            if(hero.Contains("Hero_"))
            {
                string heroNick = hero.Replace(Application.persistentDataPath+"\\"+"Hero_","").Replace(".json","");
                print(heroNick);
                var heroDataFromDevice_JSON = File.ReadAllText(hero);
                var heroDataFromDevice = JsonConvert.DeserializeObject<PlayerProgressModel>(heroDataFromDevice_JSON);

                if(storedHeroesCard.Any(card=>card.data.NickName == heroNick))
                {
                    // update
                    print("update");
                    storedHeroesCard.First(card=>card.data.NickName == heroNick).ConfigureCard(heroDataFromDevice);
                    continue;
                }
                else
                {
                    // create new
                    print("new");
                    var heroCard = Instantiate(LoginHeroCardPrefab,HerolistContainer.transform).GetComponent<HeroLoadCard>();
                    heroCard.ConfigureCard(heroDataFromDevice);
                    storedHeroesCard.Add(heroCard);
                }
            }
        }
        
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
    
    public void LoadGameWithPlayerData(PlayerProgressModel heroData)
    {
        // TODO:
        GameManager.instance.PLAYER_PROGRESS_DATA = heroData;
        HEROESLISTWINDOW.SetActive(false);
        HEROCREATORWINDOW.SetActive(false);
        MENU.SetActive(false);

    }
    public void PauseMenu()
    {
        LoadHeroesDataFromDevice();
        if(storedHeroesCard.Count > 0)
        {
            SetBtunState("Heroes",true);   
        }
        else
        {
             SetBtunState("Heroes");    
        }
        OnClick_Back();

        SetBtunState("Continue",true); 
    }
    public void OnClick_Back()
    {
        MENU.SetActive(true);
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
