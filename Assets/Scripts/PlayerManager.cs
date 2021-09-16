using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Chest;

public class PlayerManager: MonoBehaviour
{
    [SerializeField] public Statistics STATS;
    public Image HelmetIMG;
    public Image ArmorIMG;
    [SerializeField] GameObject GraphicSwitchPrefab;
    [SerializeField] public PlayerEquipmentVisualSwitchScript GraphicSwitch;
    [SerializeField] public MoveValidatorScript MovmentValidator;

   
    public string NickName;
    
    public int AttackRange,MoveRange,Gold,Cristals;

   
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public EquipmentScript _EquipedItems;
    public static PlayerManager instance;
    public NotificationScript _notificationScript;
    public ActionSwitchController _actionController;
 

    public int Power { get; private set; }
    public int CurrentHealth { 
        get => _currentHealth; 
        set 
        {
            _currentHealth = value; 
            UIManager.instance.Health_Bar.UpdateBar(_currentHealth,Mathf.RoundToInt(STATS.HealthPoints));
        }
    } 

    public Coroutine currentAutopilot = null;
    public bool playerCurrentlyMoving = false;


    public IEnumerator Autopilot(CellScript target)
    {
        int i = 0;
        NodeGrid.UpdateMapObstacleData();
        PlayerManager.instance._playerCell._pathfinder.FindPath(target);
        if(PlayerManager.instance._playerCell._pathfinder.FinalPath.Count > PlayerManager.instance.MoveRange) 
        {
            print("point is too far");
            GameManager.instance.MovingRequestTriggered = false;
            yield break;
        }
        if(playerCurrentlyMoving == true) 
        {
            print("autopilot przerwany");
            GameManager.instance.MovingRequestTriggered = false;
            yield break;
        }
        print("wystartowanie autopilota");
        playerCurrentlyMoving = true;
        PlayerManager.instance.MovmentValidator.HideGrid();
        foreach(var path in  PlayerManager.instance._playerCell._pathfinder.FinalPath)
        {
            Vector2Int direction = GameManager.Player_CELL.CurrentPosition-path.Coordination;
            if(direction.x == 0)
                GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
            if(direction.y == 0)
                GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";

            PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();

            yield return new WaitUntil(()=>GameManager.instance.TurnPhaseBegin );
            
            if(i < PlayerManager.instance._playerCell._pathfinder.FinalPath.Count)
            {
                if(GameManager.instance.SwapTilesAsDefault)
                {
                    GridManager.SwapTiles(_playerCell.ParentCell,PlayerManager.instance._playerCell._pathfinder.FinalPath[i].Coordination);
                }

                if(GameManager.instance.SlideAsDefault)       
                {
                    GridManager.CascadeMoveTo(_playerCell.ParentCell,PlayerManager.instance._playerCell._pathfinder.FinalPath[i].Coordination);
                }         
            }
            else
            {
                Debug.LogError("hola hola o co chodzi");
            }

            i++;
        }
        currentAutopilot = null;
 
        playerCurrentlyMoving = false;
        GameManager.instance.PlayerMoved = true;
        PlayerManager.instance.MovmentValidator.HideGrid();
        GameManager.instance.MovingRequestTriggered = false;

        yield return null;
    }
    public void LoadPlayerData(PlayerProgressModel _progressData)
    {
        // clear extra data from items and perks before load new data from saveFile
        STATS.ResetExtraValues(); 

        // wyczyszczenie dunga przed załądowaniem danych nowego gracza
        DungeonManager.instance.DungeonClearAndGoToCamp();

        Debug.Log("Load data from player progress file");
        // progress & resources 
        STATS.Experience       =  _progressData.Experience;                   // OK - setter
        NickName         =  _progressData.NickName;             // TODO: aktualizuje sie w PlayerCell przy starcie - nowa mapa           
        STATS.Level            =  _progressData.Level;                        // OK - setter
        STATS.AvailablePoints  =  _progressData.AvailablePoints;              // OK - setter
        STATS.Strength         =  _progressData.Strength;             // TODO: aktualizacja przy starcie gracza                                         
        STATS.Inteligence      =  _progressData.Inteligence;          // TODO: aktualizacja przy starcie gracza                             
        STATS.Dexterity  =  _progressData.Dexterity;            // TODO: aktualizacja przy starcie gracza                                     
        STATS.Vitality         =  _progressData.Vitality;             // TODO: aktualizacja przy starcie gracza                                     
        AttackRange      =  _progressData.AttackRange;                                                      
        MoveRange        =  _progressData.MoveRange;                            
        Gold             =  _progressData.Gold;
            UIManager.instance.Gold_TMP.SetText(Gold.ToString());   // TODO:
        Cristals         =  _progressData.Cristals;
            UIManager.instance.Diamonds_TMP.SetText(Cristals.ToString());   // TODO:
        STATS.HealthPoints        =  _progressData.MaxHealth;
        CurrentHealth    =  _progressData.CurrentHealth;                        // TODO wyniesc z playercell do managera i wstawic do settera
            UIManager.instance.Health_Bar.UpdateBar(CurrentHealth,Mathf.RoundToInt(STATS.HealthPoints));   // TODO:
      //  BaseDamage       =  _progressData.BaseDamage;

        _mainBackpack.GenerateEquipment();
        _EquipedItems.GenerateEquipment();
    }
 
    public void LoadPlayerItensAndEq(PlayerProgressModel _progressData, string equipmentTypeTarget)
    {
        Debug.Log("LOAD ITEMS");

       List<ItemData> itemsDatabase = Resources.LoadAll<ItemData>("Items").ToList();
       if(equipmentTypeTarget == "MainBackpack")
       {
        Debug.Log("dodawanie itemkow z save'a do backpacka");
        foreach(var data in _progressData.BagpackItems)
        {
            if(data.Count == 0) continue;
            var item = itemsDatabase.Where(i=>i.name == data.ScriptableObjectName).First();
            
            ItemPack loadedItem = new ItemPack(data.Count, item);
            Debug.Log(loadedItem.item.name);
            _mainBackpack.ItemSlots[data.SlotID].AddNewItemToSlot(loadedItem);
        }
       }

        if(equipmentTypeTarget == "PlayerEQ")
        {
            // wyczyszczenie aktualnie zalozonych itemkow gracza
            _EquipedItems.ItemSlots.ForEach(i=>i.ITEM = new ItemPack(0,null));
            foreach(var data in _progressData.EquipedItems)
            {           
                if(data.Count == 0) continue;
                var item = itemsDatabase.Where(i=>i.name == data.ScriptableObjectName).First();
                ItemPack loadedItem = new ItemPack(data.Count, item);
                Debug.Log("equip => "+loadedItem.item.name);
                _EquipedItems.LoadItemInPlayerEq(data.SlotID, loadedItem);
            }
        }
    }

    private void Awake() {
         instance = this;
    }
    public void SetPlayerManager(Player_Cell parentCell)
    {
       
        _playerCell = parentCell;
        //_mainBackpack = GameObject.Find("Content_EquipmentTab").GetComponent<EquipmentScript>();

        var uicontroller = Instantiate(GraphicSwitchPrefab,_playerCell.playerSpriteObject.transform);
        GraphicSwitch = uicontroller.GetComponent<PlayerEquipmentVisualSwitchScript>();
        
        MovmentValidator = GetComponentInChildren<MoveValidatorScript>();
        MovmentValidator.ParentPathfinder = parentCell._pathfinder;
    }
    public void Reset_QuickSlotToDefault(int quickslotID)
    {
        Action defaultAction = () => 
        {
            EquipmentScript.AssignItemToActionSlot(quickslotID);
        };
        _actionController.actionButtonsList[quickslotID].ButtonIcon_IMG.sprite = _actionController.actionButtonsList[quickslotID].IconSpriteList
            .First(n=>n.name == "ICON_"+ActionIcon.Empty.ToString());
            
        _actionController.actionButtonsList[quickslotID].ConfigureDescriptionButtonClick(
                    action: ()=>defaultAction(),
                    description: "Empty Slot.",
                    singleAction: false,
                    actionNameString: "reset to default"
                );
        EquipmentScript.AssignationItemToQuickSlotIsActive = false;
        EquipmentScript.CurrentSelectedActionButton = null;
    }
    public void AddGold(int value)
    {
        int oldGoldvalue = Int32.Parse(UIManager.instance.Gold_TMP.text);
        Gold = oldGoldvalue + value;
        UIManager.instance.Gold_TMP.SetText(Gold.ToString());
        NotificationManger.AddValueTo_Gold_Notification(value);
        CumulativeStageGoldEarned+=value;
        
    }

    public void AddExperience(int value)
    {
        STATS.Experience += value;
        CumulativeStageExperienceEarned +=value;
    }
    public void OnClick_AddResource(string _resource)
    {

        STATS.AddValue(statname:_resource);
        STATS.AvailablePoints--;
    }

    public bool AtackAnimationInProgress = false;
    public int CumulativeStageExperienceEarned;
    public int CumulativeStageGoldEarned;
    public float CumulativeStageDamageTaken;
    public float CumulativeStageDamageGained;
    public int CumulativeMonsterKilled;
    private int _currentHealth;

    public IEnumerator PerformRegularAttackAnimation(CellScript attacker, CellScript target, int _aniamtionFrames)
    {
        AtackAnimationInProgress = true;

        Vector2Int direction = target.CurrentPosition - attacker.CurrentPosition;
   
        int directionShift  = 112; // TODO: potem rozmiar komórki moze ulec zmianie, wiec dopracować
        Vector3 StartPosition = attacker.transform.localPosition;
        Vector3 EndPosition = new Vector3(attacker.transform.localPosition.x + (direction.x*directionShift),
                                        attacker.transform.localPosition.y + (direction.y*directionShift),
                                        0);

        int framesPerMove = _aniamtionFrames/2;
       // print((1f/framesPerMove).ToString());
        // wejscie
        for (float progress = 0; progress < 1; progress+=(1f/framesPerMove))
        {
          //  print("Start:"+progress);
            attacker.transform.localPosition = Vector3.Lerp(StartPosition,EndPosition,progress);
            if(progress > .75)
            {
                target._cellImage.color = Color.red;
            }
           yield return new WaitForFixedUpdate();
        }        
        

        // podswietlenie mobka na czerwono 
        for (float progress = 0; progress <= 1; progress+=(1f/framesPerMove))
        {
         //   print("end:"+progress);
            attacker.transform.localPosition = Vector3.Lerp(EndPosition,StartPosition,progress);
            if(progress > .25)
            {
                target._cellImage.color = Color.white;
            }
            yield return new WaitForFixedUpdate();
        }       

        AtackAnimationInProgress = false;
        yield return null;
    }



    [ContextMenu("Wykonaj zrzut danych gracza")]
    public void SavePlayerData()
    {
        PlayerProgressModel _updatedData = GameManager.instance.PLAYER_PROGRESS_DATA;
        _updatedData.LastVisitedDate = DateTime.Now;
   // -------------------
        _updatedData.Level =                    this.STATS.Level;
        _updatedData.Experience =               this.STATS.Experience;
        _updatedData.MaxHealth=                 Mathf.RoundToInt(this.STATS.HealthPoints);
        _updatedData.CurrentHealth =            this.CurrentHealth;
        _updatedData.Power =                    this.Power;
        _updatedData.BaseDamage =               this.STATS.BaseDamage;
    // -------------------
        _updatedData.Strength =                 this.STATS.Strength;
        _updatedData.Inteligence =              this.STATS.Inteligence;
        _updatedData.Dexterity =                this.STATS.Dexterity;
        _updatedData.Vitality =                 this.STATS.Vitality;
    // -------------------
        _updatedData.Gold =                     this.Gold;
        _updatedData.Cristals =                 this.Cristals;
    // -------------------
        _updatedData.CurrentLocation =          "Home";
        _updatedData.MoveRange =                this.MoveRange;
        _updatedData.AttackRange =              this.AttackRange;
        _updatedData.AvailablePoints =          this.STATS.AvailablePoints;


        _updatedData.ItemAssignedToAuicslot_0 = PlayerManager.instance._mainBackpack.GetItemsAssignedToQuickslot(0);
        _updatedData.ItemAssignedToAuicslot_1 = PlayerManager.instance._mainBackpack.GetItemsAssignedToQuickslot(1);
        _updatedData.ItemAssignedToAuicslot_2 = PlayerManager.instance._mainBackpack.GetItemsAssignedToQuickslot(2);
        _updatedData.ItemAssignedToAuicslot_3 = PlayerManager.instance._mainBackpack.GetItemsAssignedToQuickslot(3);
        _updatedData.ItemAssignedToAuicslot_4 = PlayerManager.instance._mainBackpack.GetItemsAssignedToQuickslot(4);
    // ----------------------------- EQUIPMENT --------------------
        _updatedData.EquipedItems = (PlayerManager.instance._EquipedItems.GetBackupListOfItemsAndSlots());
        _updatedData.BagpackItems = (PlayerManager.instance._mainBackpack.GetBackupListOfItemsAndSlots());

        HeroDataController.instance.UpdatePlayerDataFileOnDevice(_updatedData);
    }








    
}
