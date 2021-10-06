using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Monster_Cell :ICreature
{
    
    private float _healthPoints;
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
    public float HealthPoints 
    { 
        get => _healthPoints; 
        set 
        {
            _healthPoints = value;
            
            if(value>MaxHealthPoints)
                _healthPoints = MaxHealthPoints;
        }
    }
    public event EventHandler<(CellScript parent, int damageTaken, bool criticalHit,bool blockedHit, bool dodgedHit)> OnMonsterTakeDamageEvent;
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
       
        var monsterObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(monsterObject);
    
        if(_data.IsPathfinderRequired)
        {
            monsterObject.AddComponent<Pathfinding>();  
            ConfigurePathfinderComponent();
        }
        CustomEventManager.instance.OnMonsterDieEvent += MonsterDie;
    }
    public void AdjustByMapDificultyLevel(int lvl)
    {
        float atackMultip = .1f;
        float healthMultip = .1f;
        float expMultip = .50f;

        
        Damage = Mathf.RoundToInt(Damage+(Damage*lvl*atackMultip));
        
        MaxHealthPoints = Mathf.RoundToInt(MaxHealthPoints+(MaxHealthPoints*lvl*healthMultip));
        HealthPoints = MaxHealthPoints;

        ExperiencePoints = Mathf.RoundToInt(ExperiencePoints+(ExperiencePoints*lvl*expMultip));
    }
    
    public object SaveAndGetCellProgressData()
    {
      //  "zapisanie aktualnehp hp potworka");
        MonsterBackupData savedValues = new MonsterBackupData(this.MonsterDataID,Mathf.RoundToInt(this._healthPoints));
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
            ParentCell.Trash.Where(t => t.name == (Icon_Sprite.name + "(Clone)")).FirstOrDefault().TryGetComponent<Pathfinding>(out _pathfinder);
            if (_pathfinder == null)
            {
                return;
            }
            _pathfinder._cellData = ParentCell;
        }
        else
            return;

    }
    public void OnClick_MakeAction()
    {
        if(GameManager.instance.CurrentTurnPhase == GameManager.TurnPhase.PlayerMovement)
        {
            if(PlayerManager.instance.MovmentValidator.Attack_Indicators.ContainsKey(ParentCell.CurrentPosition))
            {
                if(PlayerManager.instance.MovmentValidator.Attack_Indicators[ParentCell.CurrentPosition].color == Color.clear)
                {
                    return;
                }
                GameManager.instance.NextTarget = this.OnClick_MakeAction; 
                _pathfinder.FindPath(GameManager.Player_CELL);
                GridManager.CellGridTable[_pathfinder.FinalPath[0].Coordination].MoveTo();
            }
            return;
        }

        if(GameManager.instance.CurrentTurnPhase != GameManager.TurnPhase.PlayerAttack)
        {
            return;
        }

        if(PlayerManager.instance.MovmentValidator.Attack_Indicators.ContainsKey(ParentCell.CurrentPosition))
        {
            if(PlayerManager.instance.MovmentValidator.Attack_Indicators[ParentCell.CurrentPosition].color == Color.clear)
            {
                return;
            }
        }
        else
        {
            return;
        }
        if(GameManager.instance.TurnPhaseBegin == false) return;
          
        Vector2Int direction = GameManager.Player_CELL.CurrentPosition - this.ParentCell.CurrentPosition;

        if(direction.x == 0)  GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
        if(direction.y == 0)  GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";
        PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();


        SkillsManager.SelectedAttackSkill(this);
    }
    public void TakeDamage(float damage, string source, bool _idCritical = false)
    {
        OnMonsterTakeDamageEvent?.Invoke(this,(ParentCell,Int32.Parse(damage.ToString()),_idCritical,false,false));
        HealthPoints -= Mathf.RoundToInt(damage);
        var IMPORTANTCHECKTOTRIGGERGETTER = IsAlive;
        SkillsManager.Hit1ImpactTrigger = false;
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
    {
        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = false;
        }
        this.ParentCell.isWalkable = true;
        
        NodeGrid.UpdateMapObstacleData();
        _pathfinder.FindPath(_targetCell);
        if (_pathfinder.FinalPath.Count > 1)
        {
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
        if (Border != null)
        {
            Border.GetComponent<Image>().color = Color.yellow;
            GameObject.Destroy(Border, .5f);
            Border = null;
        }
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