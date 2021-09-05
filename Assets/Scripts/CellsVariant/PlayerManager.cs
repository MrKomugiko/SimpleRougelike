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

    public TextMeshProUGUI GoldCounter_TMP;
    public TextMeshProUGUI ExperienceCounter_TMP;
    public TextMeshProUGUI HealthCounter_TMP;
    [SerializeField] private TextMeshProUGUI AvailablePoints_TMP;
    [SerializeField] private int _availablePoints;
    [SerializeField] List<Button> CoreStatButtonsList = new List<Button>();
    
    public int AvailablePoints 
    { 
        get => _availablePoints; 
        set {
            _availablePoints = value; 
            AvailablePoints_TMP.SetText(_availablePoints.ToString());
            if(AvailablePoints <= 0)
            {
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
        }
    }

    [SerializeField] private TextMeshProUGUI Level_TMP;
    [SerializeField] private int _level;
    public int Level{
        get => _level;
        set
        {
            _level=value;
            Level_TMP.SetText(_level.ToString());
        }
    }
    public int Gold = 0;
    private int _experience;
    [SerializeField] public  ResourceBarScript ExperienceBar;
    [SerializeField] public  ResourceBarScript HealthBar;
    [SerializeField] public  ResourceBarScript StaminaBar;
    [SerializeField] public  ResourceBarScript EnergyBar;
    public int Experience 
    {
        get => _experience;
        set
        {
            _experience = value;
            ExperienceCounter_TMP.SetText(Experience.ToString());
            ExperienceBar.UpdateBar(_experience,NextLevelExperience);
            if(Experience >= NextLevelExperience)
            {
                _experience = NextLevelExperience-Experience;
                Level++;
                AvailablePoints+=5;
                print("Level UP");
                ExperienceBar.UpdateBar(_experience,NextLevelExperience);
            }
        }
    }
    public int Strength = 1;
    public int Inteligence = 1;
    public int Dexterity = 1;
    public int Vitality = 1;
    [SerializeField] private int _healthPoints;
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public EquipmentScript _EquipedItems;
    public static PlayerManager instance;
    public NotificationScript _notificationScript;
    public ActionSwitchController _actionController;
    public int NextLevelExperience => (Level)*(15*Level*2);

    public Coroutine currentAutopilot = null;
    public IEnumerator Autopilot(CellScript target)
    {
        // yield return new WaitUntil(()=>GameManager.instance.TurnFinished );
        // print("szukanie trasy");
        
        // yield return new WaitForSeconds(.1f);
        // Color32 randomcolor = new Color32((byte)UnityEngine.Random.Range(0,255),(byte)UnityEngine.Random.Range(0,255),(byte)UnityEngine.Random.Range(0,255),255);
        // int i = 0;
        // NodeGrid.UpdateMapObstacleData();
        // PlayerManager.instance._playerCell._pathfinder.FindPath(target);
        // foreach(var path in  PlayerManager.instance._playerCell._pathfinder.FinalPath)
        // {
        //     GridManager.CellGridTable[path.Coordination]._cellImage.color = randomcolor;
        //     yield return new WaitUntil(()=>GameManager.instance.TurnFinished );
        //     yield return new WaitForSeconds(.1f);
        //     GridManager.SwapTiles(_playerCell.ParentCell,PlayerManager.instance._playerCell._pathfinder.FinalPath[i].Coordination);
        //     i++;
        // }
        // currentAutopilot = null;
        yield return null;
    }
    public void SetPlayerManager(Player_Cell parentCell)
    {
        instance = this;
        _playerCell = parentCell;
        //_mainBackpack = GameObject.Find("Content_EquipmentTab").GetComponent<EquipmentScript>();

        var uicontroller = Instantiate(GraphicSwitchPrefab,_playerCell.playerSpriteObject.transform);
        GraphicSwitch = uicontroller.GetComponent<PlayerEquipmentVisualSwitchScript>();
        
        HealthBar.UpdateBar(parentCell.HealthPoints,parentCell.MaxHealthPoints);
        // init restart values
        Experience = 1;
        Level = 1;
        Strength = 1;
        Inteligence = 1;
        Dexterity = 1;
        Vitality = 1;
        AvailablePoints = 0;

        print("Level 1->2 exp: "+((1)*(15*1*2)));
        print("Level 2->3 exp: "+((2)*(15*2*2)));
        print("Level 3->4 exp: "+((3)*(15*3*2)));
        print("Level 4->5 exp: "+((4)*(15*4*2)));


    }
    public void Reset_QuickSlotToDefault(int quickslotID)
    {
        Debug.Log("reset to default action button id: "+quickslotID);
        Action defaultAction = () => 
        {
            Debug.Log($"uruchomienie [EquipmentScript.AssignItemToActionSlot({quickslotID})]");
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
        int oldGoldvalue = Int32.Parse(GoldCounter_TMP.text);
        Gold = oldGoldvalue + value;
        GoldCounter_TMP.SetText(Gold.ToString());
        NotificationManger.AddValueTo_Gold_Notification(value);
    }
    public void Restart_ClearStatsAndEquipedItems()
    {
        ArmorIMG.enabled = false;
        HelmetIMG.enabled = false;

        _EquipedItems.Reset_WipeOutDataAndImages();

        Level = 1; Gold = 0; Experience = 0; Strength = 1; Inteligence = 1; Dexterity = 1; 
        _EquipedItems.ItemSlots.ForEach(slot=>slot.ITEM = new Chest.ItemPack(0, null));
    }

    public void AddExperience(int value)
    {
        Experience += value;
        ExperienceCounter_TMP.SetText(Experience.ToString());
    }

    public void AddResource(string _resource)
    {
        ResourceType resource = (ResourceType)Enum.Parse(typeof(ResourceType),_resource);
        switch(resource)
        {
            case ResourceType.STR:
                Strength++;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Strength")
                    .First().GetComponentInChildren<TextMeshProUGUI>()
                    .SetText(Strength.ToString());
                break;
                
            case ResourceType.DEX:
                Dexterity++;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Dexterity")
                    .First().GetComponentInChildren<TextMeshProUGUI>()
                    .SetText(Dexterity.ToString());
                break;            
                
            case ResourceType.INT:
                Inteligence++;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Inteligence")
                    .First().GetComponentInChildren<TextMeshProUGUI>()
                    .SetText(Inteligence.ToString());
               break;            
                
            case ResourceType.VIT:
                Vitality++;
                CoreStatButtonsList.Where(b=>b.transform.parent.name == "Vitality")
                    .First().GetComponentInChildren<TextMeshProUGUI>()
                    .SetText(Vitality.ToString());
                break;
        }
        AvailablePoints--;
    }
}
public enum ResourceType
{
    STR,
    INT,
    DEX,
    VIT
    }