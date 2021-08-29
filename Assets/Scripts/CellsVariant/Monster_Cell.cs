using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Monster_Cell :ICreature
{
    
    private int _healthPoints;
    private int _turnsElapsedCounter;
    public Pathfinding _pathfinder;
    
    #region core
    public CellScript ParentCell { get; set; }
    public TileTypes Type { get; private set; } = TileTypes.monster;
    public GameObject Icon_Sprite { get; set; }
    public GameObject Corpse_Sprite { get; private set; }

    #endregion

    #region Monster-specific
    public int ExperiencePoints { get; set; } = 10;
    public int MaxHealthPoints {get; private set;}
    public int Damage {get; private set;}
    string Corpse_Url {get;set;} = "monster_bones";
    public int HealthPoints 
    { 
        get => _healthPoints; 
        set 
        {
            _healthPoints = value;
        }
    }
    public bool IsAlive
    {
        get
        {
            if(HealthPoints > 0)
                return true;
            else
            {
                ChangeIntoTreasureObject(corpse_Url:Corpse_Sprite.name, lootID: lootID);
                return  false;
            }
        }
    }
    public int TurnsRequiredToMakeAction {get; private set;}
    public int lootID { get; private set; }
    public int Level {get;set;}
    
    #endregion

    public int TurnsElapsedCounter {
    get {return _turnsElapsedCounter;} 
    set
    {   
        _turnsElapsedCounter = value;
        if(value >= TurnsRequiredToMakeAction)
        {
            // reset licznika
            _turnsElapsedCounter = 0;
            ISReadyToMakeAction = true;
            return;
        }
        ISReadyToMakeAction = false;
    }
   }
    public bool ISReadyToMakeAction {get; private set;} = false;
    public bool IsHighlighted {get;set;} = false;
    public GameObject Border {get; set;}
    public List<(Action action,string description,ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string description,ActionIcon icon, bool singleAction)>();
    public string Name { get; set; }

    public Monster_Cell(CellScript parent, MonsterData _data)
    {
        this.ParentCell                   =       parent;       
        this.lootID                       =       _data.LootID;
        this.Name                         =       _data.MonsterName;
        this.MaxHealthPoints              =       _data.MaxHealthPoints;
        this.HealthPoints                 =       _data.MaxHealthPoints;
        this.TurnsRequiredToMakeAction    =       _data.Speed;
        this.Damage                       =       _data.Damage;
        this.Type                         =       _data.Type;;
        this.ParentCell.IsWalkable        =       _data.IsWalkable;
        this.Icon_Sprite                  =       _data.Icon_Sprite;
        this.Corpse_Sprite                =       _data.Corpse_Sprite; 
        this.Level                        =       _data.Level;

        AvaiableActions.Add((()=>OnClick_MakeAction(),"Attack", ActionIcon.Sword, true));
        NotificationManger.CreateNewNotificationElement(this);
       
        var monsterObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(monsterObject);
    
        if(_data.IsPathfinderRequired)
        {
            monsterObject.AddComponent<Pathfinding>();  
            ConfigurePathfinderComponent();
        }
    }
    public void ConfigurePathfinderComponent()
    {
        if (_pathfinder == null)
        {
          //  Debug.Log("proba załadowania pathfindera z obiektu monster");
            ParentCell.Trash.Where(t => t.name == (Icon_Sprite.name + "(Clone)")).FirstOrDefault().TryGetComponent<Pathfinding>(out _pathfinder);
            if (_pathfinder == null)
            {
                //Debug.LogError("nieudane ładowanie pathfindera");
                return;
            }
            _pathfinder._cellData = ParentCell;
        }
        else
            return;

    }
    public void OnClick_MakeAction()
    {
        if(GridManager.DistanceCheck(this) == false) {
            // out of range
             NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Info, "Creature is too far away.");
             return;
        }
        int PlayerAttakDamage = 1;

        TakeDamage(PlayerAttakDamage, "Attacked by player");
        NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.PlayerAttack);
        // delay !
        GameManager.instance.StartCoroutine(GameManager.instance.AddTurn());
    }
    public void TakeDamage(int damage, string source)
    {
        HealthPoints -= damage;
     
        if(IsAlive)
            Debug.Log($"Monster HP decerase from [{HealthPoints + damage}] to [{HealthPoints}] by <{source}>");
        else
            Debug.Log("monster died and left bones");

      
    }
    public bool TryAttack(CellScript _target)
    {
        if (Vector2Int.Distance(ParentCell.CurrentPosition, _target.CurrentPosition) > 1.1f)
        {
            return false;
        }

        //Debug.Log($"Monster zaatakował {_target.name}");
        //TODO: rozpisać to , aktualnie na sztywno -10hp
        int currentHP = Int32.Parse((GameManager.instance.HealthCounter_TMP.text.Replace("%", "")));
        currentHP -= 10;
        GameManager.instance.HealthCounter_TMP.SetText(currentHP + " %");

        NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Attack);
        return true;

    }
    public bool TryMove(CellScript _targetCell)
    {

        NodeGrid.UpdateMapObstacleData();
        _pathfinder.FindPath(_targetCell);
        if (_pathfinder.FinalPath.Count > 1)
        {
            Debug.LogError("RUCH STWORKA");
            //Debug.Log($"Monster wykonuje krok w strone celu [ komórki {_targetCell.name} ]");
            GridManager.SwapTiles(ParentCell, _pathfinder.FinalPath[0].Coordination);
            return true;
        }
        return false;

    }
    public void ChangeIntoTreasureObject(string corpse_Url, int lootID)
    {
        ParentCell.Trash.ForEach(t=>GameObject.Destroy(t.gameObject));
        ParentCell.Trash.Clear();

        
        ParentCell.Type = TileTypes.treasure;
        ParentCell.SpecialTile = new Treasure_Cell(ParentCell, GameManager.instance.GetTreasureData(lootID));
        //4. assign LootID related reward to this object
        if (Border != null)
        {
            Border.GetComponent<Image>().color = Color.yellow;
            GameObject.Destroy(Border, .5f);
            Border = null;
        }

        NotificationManger.TriggerActionNotification(ParentCell.SpecialTile as ISelectable,NotificationManger.AlertCategory.Loot);
    }

    public void RemoveBorder()
    {
        IsHighlighted = false;
        if (Border != null)
        {
            GameObject.Destroy(Border.gameObject);
        }
    }
}