using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager: MonoBehaviour
{
    public Image HelmetIMG;
    public Image ArmorIMG;
    [SerializeField] GameObject GraphicSwitchPrefab;
    [SerializeField] public PlayerEquipmentVisualSwitchScript GraphicSwitch;
    [SerializeField] public MoveValidatorScript MovmentValidator;
    // public TextMeshProUGUI GoldCounter_TMP;
    // public TextMeshProUGUI ExperienceCounter_TMP;
    // public TextMeshProUGUI HealthCounter_TMP;
    [SerializeField] private TextMeshProUGUI AvailablePoints_TMP;
    [SerializeField] private int _availablePoints;
    [SerializeField] List<Button> CoreStatButtonsList = new List<Button>();
    
    public int AvailablePoints 
    { 
        get => _availablePoints; 
        set {
            _availablePoints = value; 
            if(AvailablePoints <= 0)
            {
                _availablePoints = 0;
                foreach(var button in CoreStatButtonsList)
                {
                    button.interactable = false;
                }
            }
            else
            {
                foreach(var button in CoreStatButtonsList)
                {
                    button.interactable = true;
                }
            }

            AvailablePoints_TMP.SetText(_availablePoints.ToString());
        }
    }

    // [SerializeField] private TextMeshProUGUI Level_TMP;
    [SerializeField] private int _level;
    public int Level{
        get => _level;
        set
        {
            _level=value;
            UIManager.instance.Level_TMP.SetText(_level.ToString());
        }
    }
    public int Gold = 0;
    private int _experience;
    // [SerializeField] public  ResourceBarScript ExperienceBar;
    // [SerializeField] public  ResourceBarScript HealthBar;
    // [SerializeField] public  ResourceBarScript StaminaBar;
    // [SerializeField] public  ResourceBarScript EnergyBar;
    public int Experience 
    {
        get => _experience;
        set
        {
            _experience = value;
            // ExperienceCounter_TMP.SetText(Experience.ToString());
            UIManager.instance.Experience_Bar.UpdateBar(_experience,NextLevelExperience);
            if(Experience >= NextLevelExperience)
            {
                _experience = NextLevelExperience-Experience;

                Level++;
                AvailablePoints+=6;
               // print("Level UP");
                if(_experience<0) _experience=1;
                UIManager.instance.Experience_Bar.UpdateBar(_experience,NextLevelExperience);
            }
            if(_experience<0) _experience=1;

        }
    }
    public int Strength = 1;
    public int Inteligence = 1;
    public int Dexterity = 1;
    public int Vitality = 1;
    public int AttackRange = 1;
    public int MoveRange = 2;
    [SerializeField] private int _healthPoints;
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public EquipmentScript _EquipedItems;
    public static PlayerManager instance;
    public NotificationScript _notificationScript;
    public ActionSwitchController _actionController;
    public int NextLevelExperience => (Level)*(15*Level*2);

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
    public void SetPlayerManager(Player_Cell parentCell)
    {
        instance = this;
        _playerCell = parentCell;
        //_mainBackpack = GameObject.Find("Content_EquipmentTab").GetComponent<EquipmentScript>();

        var uicontroller = Instantiate(GraphicSwitchPrefab,_playerCell.playerSpriteObject.transform);
        GraphicSwitch = uicontroller.GetComponent<PlayerEquipmentVisualSwitchScript>();
        
        // init restart values
        Experience = 1;
        Level = 1;
        Strength = 0;
        Inteligence = 0;
        Dexterity = 0;
        Vitality = 0;
        AvailablePoints = 4;
        AddResource("STR");
        AddResource("DEX");
        AddResource("INT");
        AddResource("VIT");

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
    public void Restart_ClearStatsAndEquipedItems()
    {
        ArmorIMG.enabled = false;
        HelmetIMG.enabled = false;

        _EquipedItems.Reset_WipeOutDataAndImages();

        Level = 1; 
        Gold = 0; 
        Experience = 0; 
        Strength = 1; 
        Inteligence = 1; 
        Dexterity = 1; 
        _EquipedItems.ItemSlots.ForEach(slot=>slot.ITEM = new Chest.ItemPack(0, null));
    }

    public void AddExperience(int value)
    {
        Experience += value;
        CumulativeStageExperienceEarned +=value;
    }
    public void OnClick_AddResource(string _resource)
    {
        AddResource(_resource,1,false);
    }
    public void AddResource(string _resource, int value = 1, bool isFree = false)
    {
        ResourceType resource = (ResourceType)Enum.Parse(typeof(ResourceType),_resource);
        if(isFree == false)
        {
            if(value <= AvailablePoints)
            {
                AvailablePoints -= value;
            }
            else
            {
                print("not enought statistics point to add");
                return;
            }
        } 

        switch(resource)
        {
            case ResourceType.STR:
                Strength+= value;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Strength")
                    .First().transform.parent.transform.Find("Value")
                    .GetComponent<TextMeshProUGUI>()
                    .SetText(Strength.ToString());
                break;
                
            case ResourceType.DEX:
                Dexterity+= value;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Dexterity")
                    .First().transform.parent.transform.Find("Value")
                    .GetComponent<TextMeshProUGUI>()
                    .SetText(Dexterity.ToString());
                break;            
                
            case ResourceType.INT:
                Inteligence+= value;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Inteligence")
                    .First().transform.parent.transform.Find("Value")
                    .GetComponent<TextMeshProUGUI>()
                    .SetText(Inteligence.ToString());
               break;            
                
            case ResourceType.VIT:
                Vitality+= value;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Vitality")
                    .First().transform.parent.transform.Find("Value")
                    .GetComponent<TextMeshProUGUI>()
                    .SetText(Vitality.ToString());
                break;
        }
  
    }


    public bool AtackAnimationInProgress = false;
    public int CumulativeStageExperienceEarned;
    public int CumulativeStageGoldEarned;
    public int CumulativeStageDamageTaken;
    public int CumulativeStageDamageGained;
    public int CumulativeMonsterKilled;


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


}
