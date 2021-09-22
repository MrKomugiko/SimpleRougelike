using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDataController : MonoBehaviour
{    
    [SerializeField] public Button CreateHeroButton;
    [SerializeField] public GameObject HeroCard_Prefab;
    [SerializeField] public GameObject HerolistContainer;
    public static HeroDataController instance;
    public Dictionary<int,HeroLoadCard> storedHeroesCard = new Dictionary<int, HeroLoadCard>();

    private void Awake() {
        instance = this;

        CreateEmptyHeroCards();
        
        LoadHeroesDataFromDevice();
    }
    public void CreateEmptyHeroCards()
    {
        for (int i = 0; i < 6; i++)
        {
            CreateEmptyHeroDataIfNotExist(i);
            if(storedHeroesCard.ContainsKey(i)) continue;
            storedHeroesCard.Add(i,Instantiate(HeroCard_Prefab,HerolistContainer.transform).GetComponent<HeroLoadCard>());
        }
    }
    public void CreateEmptyHeroDataIfNotExist(int _ID)
    {
        List<int> existingIndexes = new List<int>();

        foreach(string herourl in Directory.GetFiles(Application.persistentDataPath))
        {
            var x = herourl.Split(char.Parse("["),char.Parse("]"))[1];
            int heroSlotID = Int32.Parse(x);
            existingIndexes.Add(heroSlotID);
        } 

        if(existingIndexes.Contains(_ID)) 
        {
         //   print("Zawiera juz wpi z id "+_ID);
            return;
        }

       // print("create empty hero data file");
        PlayerProgressModel newHero = new PlayerProgressModel("_EMPTY_", _ID);
        newHero.SlotID = _ID;    
        string JSONresult = JsonConvert.SerializeObject(newHero);
        File.WriteAllText(Application.persistentDataPath + $"/[{_ID}]_Hero__EMPTY_.json", JSONresult);        
    }
    public void LoadHeroesDataFromDevice()
    {
       // print("load heroes form device");
        Dictionary<int,PlayerProgressModel> AllPlayersData = new Dictionary<int, PlayerProgressModel>();
        for (int slotID = 0; slotID < 6; slotID++)
        {
            storedHeroesCard[slotID].ConfigureCard(GetPlayerDataModelFromFile(slotID),slotID);
        }
    }
    private PlayerProgressModel GetPlayerDataModelFromFile(int slotID)
    {
       // print("szukanie pliku z id slotu = "+slotID);
        PlayerProgressModel data;
        int heroSlotID = -1;
        string heroPath = "";
        string[] heroesStoredOnDevice = Directory.GetFiles(Application.persistentDataPath);
       // print("heroesStoredOnDevice zawiera elementow: "+heroesStoredOnDevice.Length);
        foreach(string herourl in heroesStoredOnDevice)
        {
            heroSlotID = Int32.Parse(herourl.Split(char.Parse("["),char.Parse("]"))[1]);
           // print("check : "+heroSlotID);
            if(heroSlotID == slotID)
            {   
                heroPath = herourl;
              //  print("jest dopasowanie dla slotu:"+heroSlotID+" => url pliku = "+heroPath);
                break;
            }
        }

        if(heroSlotID == -1)
        {
          //  Debug.LogError("nie znaleziono pliku z id "+slotID);
            return null;
        }

        string heroDataFromDevice_JSON = File.ReadAllText(heroPath);

        data = JsonConvert.DeserializeObject<PlayerProgressModel>(heroDataFromDevice_JSON); 

        //print("odczytano dane: nick == "+data.NickName);
        if(data.isDeleted == true) 
        {
            data = new PlayerProgressModel("_DELETED_", slotID);
            data.SlotID = slotID;
            data.isDeleted = false;
            return data;
        }

        return data;
    } 
    public void CreateNewHero(int slotID)
    {
       // Debug.Log("tworzenie nowego bohatera.");

        string nickname = GameObject.Find("InputField_NewHeroNickName").GetComponent<InputField>().text;
        //  sprawdzenie czy taki bohater juz istnieje
        if(storedHeroesCard.Any(hero=>hero.Value.data.NickName == nickname))  
        {
            Debug.LogError("hero with this name already exist");
            MenuScript.instance.HEROCREATORWINDOW_ErrorTMP.SetText("hero with this name already exist");
            return;
        }

        MenuScript.instance.HEROCREATORWINDOW_ErrorTMP.SetText("");

        PlayerProgressModel newCreatedHero = new PlayerProgressModel(nickname, slotID);
        string JSONresult = JsonConvert.SerializeObject(newCreatedHero);
        // nadpisanie pliku

        if(File.Exists(Application.persistentDataPath + $"/[{slotID}]_Hero__EMPTY_.json"))
        {
          //  print("podmianka pliku emptyy");
            File.Move(Application.persistentDataPath + $"/[{slotID}]_Hero__EMPTY_.json",Application.persistentDataPath + $"/[{slotID}]_Hero_{nickname}.json");
        }
        if(File.Exists(Application.persistentDataPath + $"/[{slotID}]_Hero__DELETED_.json"))
        {
          //  print("podmianka usunietego pliku");
            File.Move(Application.persistentDataPath + $"/[{slotID}]_Hero__DELETED_.json",Application.persistentDataPath + $"/[{slotID}]_Hero_{nickname}.json");
        }

        //print("zapisanie danych do pliku");
        File.WriteAllText(Application.persistentDataPath + $"/[{slotID}]_Hero_{nickname}.json", JSONresult);

        Debug.Log(JSONresult);
        
        LoadHeroesDataFromDevice();
        LoadPlayerDataInGame(newCreatedHero);
     //   Debug.Log("przejscie do gry");
        MenuScript.instance.OpenCampScene();

    }
    public void LoadPlayerDataInGame(PlayerProgressModel heroData)
    {

        if(heroData.NickName != GameManager.instance.PLAYER_PROGRESS_DATA.NickName)
        {
            GameManager.instance.PLAYER_PROGRESS_DATA = heroData;
            PlayerManager.instance.LoadPlayerData(heroData);
        }
    }  
    public void RemoveHeroFromDevice(PlayerProgressModel data)
    {
        //print("usuwanie gracza z pamieci");

        if (GameManager.instance.PLAYER_PROGRESS_DATA.NickName == data.NickName)
        {
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.transform.parent.gameObject.SetActive(true);
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.onClick.RemoveAllListeners();
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.onClick.AddListener(
                () =>
                {
                    RemovePlayerDataSwapWithEmpty(data);
                    LoadHeroesDataFromDevice();
                    MenuScript.instance.SetButtonState("Continue", false);
                }
                );
            //wyswietlenie ostrzerzenia ze usuwane jest aktualnie zalogowane konto,
            MenuScript.instance.HEROESLISTWINDOW_RemoveConfirmButton.transform.parent.GetComponentInChildren<TextMeshProUGUI>().SetText($"This Hero <b>({data.NickName})</b> is currently logged.\n\nAre you shure You want to delete this account?\n\n<b>[This operation is permanent!]</b>");
            return;
        }

        RemovePlayerDataSwapWithEmpty(data);

        LoadHeroesDataFromDevice();

    }

    private void RemovePlayerDataSwapWithEmpty(PlayerProgressModel data)
    {
        PlayerProgressModel blank_emptyhero = new PlayerProgressModel("_EMPTY_", data.SlotID);
        storedHeroesCard[data.SlotID].ConfigureCard(blank_emptyhero, data.SlotID);
        string JSONresult = JsonConvert.SerializeObject(blank_emptyhero);
        if (File.Exists(Application.persistentDataPath + $"/[{data.SlotID}]_Hero_{data.NickName}.json"))
        {
            //  print("podmianka pliku emptyy");
            File.Move(Application.persistentDataPath + $"/[{data.SlotID}]_Hero_{data.NickName}.json", Application.persistentDataPath + $"/[{data.SlotID}]_Hero__EMPTY_.json");
        }
        File.WriteAllText(Application.persistentDataPath + $"/[{data.SlotID}]_Hero__EMPTY_.json", JSONresult);
    }

    public void UpdatePlayerDataFileOnDevice(PlayerProgressModel _updatedData)
    {
        if(storedHeroesCard.Any(hero=>hero.Value.data.NickName == _updatedData.NickName))  
        {
            Debug.LogError("ZAPISANIE POSTEPOW NA URZADZENIU - w pliku");
          //  Debug.LogError("hero exist, overrite data");
            string JSONresult = JsonConvert.SerializeObject(_updatedData);

            File.WriteAllText(Application.persistentDataPath + $"/[{_updatedData.SlotID}]_Hero_{_updatedData.NickName}.json", JSONresult);

            LoadHeroesDataFromDevice();

         //   Debug.Log(JSONresult);
            return;
        }
    }
}
