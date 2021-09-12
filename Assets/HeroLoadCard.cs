using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroLoadCard : MonoBehaviour
{    
    public int SlotID;

    [SerializeField] GameObject MainSection, DetailSection;
    [SerializeField] TextMeshProUGUI AddHero_TMP;

    [SerializeField] TextMeshProUGUI Level;
    [SerializeField] TextMeshProUGUI Nick;
    [SerializeField] TextMeshProUGUI Location;
    [SerializeField] TextMeshProUGUI CurrentHEalth;
    [SerializeField] TextMeshProUGUI Gold;
    [SerializeField] TextMeshProUGUI Cristals;
    [SerializeField] TextMeshProUGUI Power;
    [SerializeField] TextMeshProUGUI MaxDungeon;
    [SerializeField] Button MainButton;
    [SerializeField] Button RemoveButton;

    [SerializeField] public PlayerProgressModel data;

    public void ConfigureCard(PlayerProgressModel _data, int _slotID )
    {
        SlotID = _slotID;
        data = _data;

        if(_data.NickName == "_EMPTY_" || _data.NickName == "_DELETED_")
        {
            MainSection.SetActive(false);
            DetailSection.SetActive(false);
            AddHero_TMP.gameObject.SetActive(true);

            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(()=>{
                    MenuScript.instance.OnClick_NewHero(SlotID);
                }
            );
            return;
        }

        MainSection.SetActive(true);
        DetailSection.SetActive(true);
        AddHero_TMP.gameObject.SetActive(false);
        
        Level.SetText($"Lvl.: {data.Level}");
        Nick.SetText($"{data.NickName}");
        Location.SetText($"Location: {data.CurrentLocation}");
        CurrentHEalth.SetText($"Health: {data.CurrentHealth}");
        Gold.SetText($"Gold: {data.Gold}");
        Cristals.SetText($"Cristals: {data.Cristals}");
        Power.SetText($"Power: {data.Power}");
        MaxDungeon.SetText($"Top Dungeon: {data.HighestDungeonStage}");

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(
            ()=>{
                    MenuScript.instance.OpenGameScene();
                    HeroDataController.instance.LoadPlayerDataInGame(data);
                }
            );
        RemoveButton.onClick.RemoveAllListeners();
            RemoveButton.onClick.AddListener(()=>HeroDataController.instance.RemoveHeroFromDevice(data));
        }
}
