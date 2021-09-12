using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroLoadCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Level;
    [SerializeField] TextMeshProUGUI Nick;
    [SerializeField] TextMeshProUGUI Location;
    [SerializeField] TextMeshProUGUI CurrentHEalth;
    [SerializeField] TextMeshProUGUI Gold;
    [SerializeField] TextMeshProUGUI Cristals;
    [SerializeField] TextMeshProUGUI Power;
    [SerializeField] TextMeshProUGUI MaxDungeon;
    
    [SerializeField] Button MainButton;
   
    [SerializeField] public PlayerProgressModel data;
    private string deviceFilePath = "";

    public void ConfigureCard(PlayerProgressModel _data)
    {
        data = _data;
        Level.SetText($"Lvl.: {data.Level}");
        Nick.SetText($"{data.NickName}");
        Location.SetText($"Location: {data.CurrentLocation}");
        CurrentHEalth.SetText($"Health: {data.CurrentHealth}");
        Gold.SetText($"Gold: {data.Gold}");
        Cristals.SetText($"Cristals: {data.Cristals}");
        Power.SetText($"Power: {data.Power}");
        MaxDungeon.SetText($"Top Dungeon: {data.HighestDungeonStage}");

        deviceFilePath = $"{Application.persistentDataPath}\\Hero_{data.NickName}.json";

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(()=>MenuScript.instance.LoadGameWithPlayerData(data));
    }

    public void RemoveHeroFromDevice()
    {
        if(GameManager.instance.PLAYER_PROGRESS_DATA.NickName == data.NickName)
        {
            
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.transform.parent.gameObject.SetActive(true);
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.onClick.RemoveAllListeners();
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.onClick.AddListener(
                ()=>{
                        MenuScript.instance.storedHeroesCard.Remove(this);
                        Destroy(this.gameObject);
                        File.Delete(deviceFilePath);
                        MenuScript.instance.SetBtunState("Continue",false);
                    }
                );
            //wyswietlenie ostrzerzenia ze usuwane jest aktualnie zalogowane konto,
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.transform.parent.GetComponentInChildren<TextMeshProUGUI>().SetText($"This Hero <b>({data.NickName})</b> is currently logged.\n\nAre you shure You want to delete this account?\n\n<b>[This operation is permanent!]</b>");
            return;
        }

        MenuScript.instance.storedHeroesCard.Remove(this);
        Destroy(this.gameObject);
        File.Delete(deviceFilePath);
    }
}
