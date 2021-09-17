using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Bomb_Cell : ISpecialTile, IFragile, IUsable, ISelectable
{

    public GameObject Border { get; set; }
    public bool IsHighlighted { get; set; }
    public CellScript ParentCell {get; private set;}
    public TileTypes Type { get; set; } 
    public string Name { get; set; }
    public GameObject Icon_Sprite {get;set;}
    public List<(Action action, string description, ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string description, ActionIcon icon, bool singleAction)>();
    private bool isReady = false;
    public bool IsReadyToUse {
        get 
        {     
            if(RestrictedByTimer)
            {   
                if(GameManager.instance.CurrentTurnNumber-_spawnTurnNumber > TurnsRequiredToActivate)
                {
                    isReady = true;   
                    ParentCell.Trash.First(t=>t.name.Contains(Icon_Sprite.name)).GetComponentInChildren<SpriteRenderer>().color = Color.magenta;
                    return true;
                }
                
                return false;
            }
            else
            {
                /// jezeli nie ma licznika ZAWSZE bedzie gotowa do uzycia
                return true;
            }
        } 
    }
    public bool IsUsed {get; set;} = false;
    public GameObject Effect_Sprite {get; private set;}
    public int TurnsRequiredToActivate;
    private int _spawnTurnNumber;
    public TickScript TickCounter;
    public int BombDamage;
    private ExplosionPatterns ExplosionPattern;
    private Directions Direction;
    public int BombID;
    private bool RestrictedByTimer;
    private int Size;  
    public List<CellScript> CellsToDestroy = new List<CellScript>();
    public List<Vector2Int> ExplosionVectors = new List<Vector2Int>();
    internal bool IsImpactAreaHighlihted = false;
    public BombData BombData_Backup_DATA;
    public Bomb_Cell(CellScript parent, BombData _data)
    {

        BombData_Backup_DATA = _data;
        this.ParentCell                   =       parent;       
        this.ParentCell.IsWalkable        =       _data.IsWalkable;
        this.Type                         =       _data.Type;;
        this.Name                         =       _data.BombName;

        this.BombID                       =       _data.ID;
        this.Effect_Sprite                =       _data.Exploasion_Sprite;
        this.Icon_Sprite                  =       _data.Icon_Sprite;
        this.RestrictedByTimer            =       _data.RestrictedByTimer;
        this.TurnsRequiredToActivate      =       _data.TicksToTurnOn;
        this.BombDamage                   =       _data.Damage;
        this.ExplosionPattern             =       _data.ExplosionPattern;
        this.Direction                    =       _data.Direction;
        this.Size                         =       _data.Size;

        var bombObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(bombObject);

        ExplosionVectors = _data.GetExplosionVectors(this.ExplosionPattern);

        _spawnTurnNumber = GameManager.instance.CurrentTurnNumber;
        if(RestrictedByTimer)
        {
            var ticker = GameManager.instance.InstantiateTicker(this);
            ParentCell.Trash.Add(ticker);      
            this.TickCounter = ticker.GetComponentInChildren<TickScript>();
            this.TickCounter.parent = ParentCell;
        }

        AvaiableActions.Add((()=>Use(),"Detonate", ActionIcon.Bomb, true));
        AvaiableActions.Add((()=>SwitchHighlightImpactArea(),"Show Inpact Area",ActionIcon.Flag, false));
        // AvaiableActions.Add((null,"WIP: Disarm",ActionIcon.Delete));

      //  NotificationManger.CreateNewNotificationElement(this);
    }
    
    public void OnClick_MakeAction()
    {   
        Debug.Log("bomb click");
        if(GameManager.instance.TurnPhaseBegin == false) return;
        Vector2Int direction = GameManager.Player_CELL.CurrentPosition - this.ParentCell.CurrentPosition;

//        Debug.Log(direction);
        if(direction.x == 0)
            GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
        
        if(direction.y == 0)
            GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";
            
        PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();

        if(IsReadyToUse == true) 
        {
            if(GameManager.instance.CurrentTurnPhase == GameManager.TurnPhase.PlayerMovement)
            {
                Use();    
            }
        }
        else
        {
            if(GameManager.instance.CurrentTurnPhase == GameManager.TurnPhase.PlayerMovement)
            {
                ParentCell.MoveTo();
            }       
            Debug.Log("to nie tura ruchu");

        }   

    }
    public void Use()
    {
        if(IsImpactAreaHighlihted) SwitchHighlightImpactArea();
        if(IsReadyToUse == false) return;
        if(IsUsed == true) return;

        IsUsed = true;
       // Debug.LogError("WYBUCH BOMBY !");
        AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);
        foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
        {   
            cell.AddEffectImage(sprite: Effect_Sprite);
            if(cell.SpecialTile is ILivingThing)
            {
                (cell.SpecialTile as ILivingThing).TakeDamage(BombDamage, "Bomb Explosion");
                NotificationManger.TriggerActionNotification
                (
                    INVOKER:this,
                    NotificationManger.AlertCategory.ExplosionDamage,
                    TARGET_BaseCEll: cell.SpecialTile 
                );
             //   GridManager.instance.DamageMap.Add((cell.SpecialTile as ILivingThing, BombDamage));
                continue;
            } 
            if(cell.SpecialTile is Bomb_Cell)
            {
                if((cell.SpecialTile as IUsable).IsUsed ==false)
                {
                    Debug.LogError("bomba do odstrzału => "+cell.CurrentPosition);
                    (cell.SpecialTile as IUsable).Use();
                }
            } 
            GridManager.instance.DestroyedCells.Add(cell);
        }
        GridManager.instance.ExecuteExplodes();
        RemoveBorder();
    }

    public List<CellScript> GetDestroyedCellsFromCascadeContinueExploding()
    {
        //Debug.LogError("GetDestroyedCellsFromCascadeContinueExploding");
        List<CellScript> result = new List<CellScript>();

        if(IsImpactAreaHighlihted)
        {   
            SwitchHighlightImpactArea();
        }
        if(IsUsed == true) 
            return result;
        
        IsUsed = true;
        AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);

        foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
        {   
            cell.AddEffectImage(sprite: Effect_Sprite);
            if(cell.SpecialTile is ILivingThing)
            {
                (cell.SpecialTile as ILivingThing).TakeDamage(BombDamage, "Bomb Explosion");
                NotificationManger.TriggerActionNotification(
                    INVOKER:this, 
                    CATEGORY: NotificationManger.AlertCategory.ExplosionDamage,
                    TARGET_BaseCEll:cell.SpecialTile
                );
                GridManager.instance.DamageMap.Add((cell.SpecialTile as ILivingThing, BombDamage));
                continue;
            } 
            if(cell.SpecialTile is Bomb_Cell)
            {
                if((cell.SpecialTile as IUsable).IsUsed ==false)
                {
                    Debug.LogError("bomba do odstrzału => "+cell.CurrentPosition);
                    result.AddRange(GetDestroyedCellsFromCascadeContinueExploding());
                }
            } 
            result.Add(cell);
        }

        return result;
    }
    private void AddCellsToDestroyList(Vector2Int nextPosition, Vector2Int direction)
    {
        foreach(var vector in ExplosionVectors)
        {
            var position = vector+direction+nextPosition;
            if (GridManager.CellGridTable.ContainsKey(position) && !CellsToDestroy.Contains(GridManager.CellGridTable[position]))
                CellsToDestroy.Add(GridManager.CellGridTable[position]);
        }
    }
    public void ActionOnMove(Vector2Int nextPosition, Vector2Int direction)
    {
        if(isReady == false) return;
        if(IsUsed) return;
        IsUsed = true;
        isReady = false;

 /////////////////////////////////////////////////   AddCellsToDestroyList(nextPosition, Vector2Int.zero);
 /////////////////////////////////////////////////   int bombCounter = 0;
 /////////////////////////////////////////////////   foreach (var cell in CellsToDestroy.Where(cell => cell != null))
 /////////////////////////////////////////////////   {
 /////////////////////////////////////////////////       cell.AddEffectImage(sprite: Effect_Sprite);
 /////////////////////////////////////////////////       if (cell.SpecialTile is ICreature)
 /////////////////////////////////////////////////       {
 /////////////////////////////////////////////////           (cell.SpecialTile as ICreature).TakeDamage(BombDamage, "Bomb Explosion");
 /////////////////////////////////////////////////           NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.ExplosionDamage);
 /////////////////////////////////////////////////       }
 /////////////////////////////////////////////////       if (cell.SpecialTile is Bomb_Cell)
 /////////////////////////////////////////////////       {
 /////////////////////////////////////////////////           if((cell.SpecialTile as IUsable).IsUsed)
 /////////////////////////////////////////////////           {
 /////////////////////////////////////////////////               bombCounter++;
 /////////////////////////////////////////////////               (cell.SpecialTile as IUsable).Use();
 /////////////////////////////////////////////////               GridManager.instance.ListBombsWaitingForDetonate.Add(ParentCell);
 /////////////////////////////////////////////////                       
 /////////////////////////////////////////////////               Debug.LogWarning($"OnMove -> DODANO EKSPLOZJE DO KOLEJKI {cell.CurrentPosition} / {cell.name}");
 /////////////////////////////////////////////////           }
 /////////////////////////////////////////////////       }
 /////////////////////////////////////////////////   }
 /////////////////////////////////////////////////
 /////////////////////////////////////////////////   Debug.LogError("bomb counters = "+bombCounter);
 /////////////////////////////////////////////////   // if (bombCounter == 1)
 /////////////////////////////////////////////////   // {
 /////////////////////////////////////////////////   //     Debug.LogWarning("START!");
 /////////////////////////////////////////////////   //     GridManager.instance.RunExploding();
 /////////////////////////////////////////////////   // }
 /////////////////////////////////////////////////   RemoveBorder();
 /////////////////////////////////////////////////
 /////////////////////////////////////////////////   GridManager.CurrentExplosionQueue.Enqueue(()=>GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy));
    }  
    public void RemoveBorder()
    {
        IsHighlighted = false;
        if (Border != null)
        {
            GameObject.Destroy(Border.gameObject);
        }
    }
    private List<GameObject> HighlihtedArea = new List<GameObject>();


    public void SwitchHighlightImpactArea()
    {
        Debug.Log($"switch impact area range from {IsImpactAreaHighlihted} to {!IsImpactAreaHighlihted}");
        if(IsImpactAreaHighlihted == false)
        {
            IsImpactAreaHighlihted = true;
            List<Vector2Int> ImpactAreaPositions = new List<Vector2Int>();
            foreach(var vector in ExplosionVectors)
            {
                if(GridManager.CellGridTable.ContainsKey(ParentCell.CurrentPosition + vector))
                {
                    ImpactAreaPositions.Add(ParentCell.CurrentPosition + vector);
                }
            }
            HighlihtedArea.AddRange(NotificationManger.HighlightAreaWithTemporaryBorders(ImpactAreaPositions, Color.magenta));

            GameObject.Find("ExitButton").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("ExitButton").GetComponent<Button>().onClick.AddListener(()=>NotificationManger.RemoveTemporaryBordersObjectsFromArea(HighlihtedArea));
            return;
   
        }
        if(IsImpactAreaHighlihted == true)
        {
            NotificationManger.RemoveTemporaryBordersObjectsFromArea(HighlihtedArea);
            IsImpactAreaHighlihted = false;
            return;
        }
    }
}
