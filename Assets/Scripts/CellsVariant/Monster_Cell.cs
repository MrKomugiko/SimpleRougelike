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

    private int MonsterDataID;

    public TileTypes Type { get; set; } = TileTypes.monster;
    public GameObject Icon_Sprite { get; set; }
    public GameObject Corpse_Sprite { get; private set; }

    #endregion

    #region Monster-specific
    public int ExperiencePoints { get; set; } = 10;
    public int MaxHealthPoints {get; private set;}
    public float Damage {get; private set;}
    string Corpse_Url {get;set;} = "monster_bones";
    public int HealthPoints 
    { 
        get => _healthPoints; 
        set 
        {
            OnMonsterTakeDamageEvent?.Invoke(this,(ParentCell,value-_healthPoints));
            _healthPoints = value;
            
            if(value>MaxHealthPoints)
                _healthPoints = MaxHealthPoints;
        }
    }
    public event EventHandler<(CellScript ParticleSystemEmissionType, int damageTaken)> OnMonsterTakeDamageEvent;
    public event EventHandler<string> OnMonsterDieEvent;
    public bool IsAlive
    {
        get
        {
            if(HealthPoints > 0)
                return true;
            else
            {
                OnMonsterDieEvent?.Invoke(this,Name+" died.");
                PlayerManager.instance.AddExperience(ExperiencePoints);
                ChangeIntoTreasureObject(_data: lootID);
                return  false;
            }
        }
    }
   
    public int TurnsRequiredToMakeAction {get; private set;}
    public TreasureData lootID { get; private set; }
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

    public MonsterBackupData MonsterData_Backup_DATA;
    public Monster_Cell(CellScript parent, MonsterData _data, MonsterBackupData restoredData = null)
    {
        this.ParentCell                   =       parent;      
        this.MonsterDataID = _data.ID; 
        this.lootID                       =       _data.LootData;
        this.Name                         =       _data.MonsterName;
        this.MaxHealthPoints              =       _data.MaxHealthPoints;

        this.HealthPoints                 =       restoredData==null?_data.CurrentHealthPoints:restoredData.HealthPoints;
        this.TurnsRequiredToMakeAction    =       _data.Speed;
        this.Damage                       =       _data.Damage;
        this.Type                         =       _data.Type;;
        this.ParentCell.IsWalkable        =       _data.IsWalkable;
        this.Icon_Sprite                  =       _data.Icon_Sprite;
        this.Corpse_Sprite                =       _data.Corpse_Sprite; 
        this.Level                        =       _data.Level;

        CustomEventManager.instance.RegisterMonsterInEventManager(this);

        AvaiableActions.Add((()=>OnClick_MakeAction(),"Attack", ActionIcon.Sword, true));
        NotificationManger.CreateNewNotificationElement(this);
       
        var monsterObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(monsterObject);
    
        if(_data.IsPathfinderRequired)
        {
            monsterObject.AddComponent<Pathfinding>();  
            ConfigurePathfinderComponent();
        }
        CustomEventManager.instance.OnMonsterDieEvent += MonsterDie;
    }
    
    public object SaveAndGetCellProgressData()
    {
        Debug.Log("zapisanie aktualnehp hp potworka");
        MonsterBackupData savedValues = new MonsterBackupData(this.MonsterDataID,this._healthPoints);
        return savedValues;

    }

    private void MonsterDie(object sender, string e)
    {
        Debug.LogError(e);
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
        if(GameManager.instance.CurrentTurnPhase != GameManager.TurnPhase.PlayerAttack)
        {
            Debug.Log($"trwa innna tura({GameManager.instance.CurrentTurnPhase.ToString()}) niż tura ataku");
            return;
        }
        if(GameManager.instance.TurnPhaseBegin == false) return;
                // Debug.Log("Gracz kliknął na siebie samego");
        Vector2Int direction = GameManager.Player_CELL.CurrentPosition - this.ParentCell.CurrentPosition;

//        Debug.Log(direction);
        if(direction.x == 0)
            GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
        
        if(direction.y == 0)
            GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";
        PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();

        if(GridManager.DistanceCheck(this) == false) {
            // out of range
             NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Info, "Creature is too far away.");
             return;
        }
        TakeDamage((GameManager.Player_CELL.SpecialTile as Player_Cell).Damage, "Attacked by player");
        PlayerManager.instance.CumulativeStageDamageTaken += (GameManager.Player_CELL.SpecialTile as Player_Cell).Damage;
        NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.PlayerAttack);
        // delay !

        PlayerManager.instance.StartCoroutine(PlayerManager.instance.PerformRegularAttackAnimation(PlayerManager.instance._playerCell.ParentCell,this.ParentCell,GameManager.instance.attackAnimationFrames));

        GameManager.instance.PlayerAttacked = true;        
    }
    public void TakeDamage(float damage, string source)
    {
    
        HealthPoints -= Mathf.RoundToInt(damage);
     
        if(IsAlive)
            Debug.Log($"Monster HP decerase from [{HealthPoints + damage}] to [{HealthPoints}] by <{source}>");    
    }
    public bool TryAttack(CellScript _target)
    {
        if (Vector2Int.Distance(ParentCell.CurrentPosition, _target.CurrentPosition) > 1.1f)
        {
            return false;
        }

        (_target.SpecialTile as ILivingThing).TakeDamage(Damage,Name);
        NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Attack);
        return true;
    }
    public bool TryMove(CellScript _targetCell)
    {;
        NodeGrid.UpdateMapObstacleData();
        _pathfinder.FindPath(_targetCell);
        if (_pathfinder.FinalPath.Count > 1)
        {
           // Debug.LogError("RUCH STWORKA");
            //Debug.Log($"Monster wykonuje krok w strone celu [ komórki {_targetCell.name} ]");
            GridManager.SwapTiles(ParentCell, _pathfinder.FinalPath[0].Coordination);
            return true;
        }
        return false;
    }
    public void ChangeIntoTreasureObject(TreasureData _data)
    {
        PlayerManager.instance.CumulativeMonsterKilled++;
        
        ParentCell.Trash.ForEach(t=>GameObject.Destroy(t.gameObject));
        ParentCell.Trash.Clear();

        ParentCell.Type = TileTypes.treasure;
        ParentCell.IsWalkable = true;
        ParentCell.SpecialTile = new Treasure_Cell(ParentCell, _data);
;       (ParentCell.SpecialTile as Treasure_Cell).RemoveFromMapIfChesIsEmpty();
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