using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb_Cell : ISpecialTile, IFragile, IUsable, ISelectable
{

    #region SELECTABLE : VARIABLES
        public GameObject Border { get; set; }
        public bool IsHighlighted { get; set; }
    #endregion
    #region SPECIALTILE : VARIABLES
    public CellScript ParentCell {get; private set;}
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public GameObject Icon_Sprite {get;set;}
    public List<(Action action, string description, ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string description, ActionIcon icon, bool singleAction)>();
    
    #endregion
    #region USABLE : VATIABLES
    public bool IsReadyToUse {
        get 
        {     
            if(RestrictedByTimer)
            {
                return (GameManager.instance.CurrentTurnNumber-_spawnTurnNumber) > TurnsRequiredToActivate;
            }
            else
            {
                /// jezeli nie ma licznika ZAWSZE bedzie gotowa do uzycia
                return true;
            }
        } 
    }
    public bool IsUsed {get; private set;} = false;
    public GameObject Effect_Sprite {get; private set;}
    #endregion
    #region BOMB-SPECIFIC : VARIABLE
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
    #endregion;
 
    public Bomb_Cell(CellScript parent, BombData _data)
    {
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

        NotificationManger.CreateNewNotificationElement(this);
    }
    
    public void OnClick_MakeAction()
    {        
        if(IsReadyToUse == false) return;
        {
            Use();
        }
    }
    public void Use()
    {
        if(IsImpactAreaHighlihted)
        {   
            // turn off
            SwitchHighlightImpactArea();
        }
        if(IsUsed == true) 
            return;

        IsUsed = true;

        AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);

        foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
        {   
            cell.AddEffectImage(sprite: Effect_Sprite);
            if(cell.SpecialTile is ICreature)
            {
                (cell.SpecialTile as ICreature).TakeDamage(BombDamage, "Bomb Explosion");
                NotificationManger.TriggerActionNotification(cell.SpecialTile as ISelectable, NotificationManger.AlertCategory.ExplosionDamage);
            } 
            if(cell.SpecialTile is Bomb_Cell)
            {
                // (cell.SpecialTile as IUsable).Use(); // TODO: Laskadowe niszczenie sie bomb jest wyłączone dla testow
            } 
        }
        
        RemoveBorder();
        GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);
        
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
        if(IsUsed) return;
        IsUsed = true;

        AddCellsToDestroyList(nextPosition, direction);

        foreach (var cell in CellsToDestroy.Where(cell => cell != null))
        {
            cell.AddEffectImage(sprite: Effect_Sprite);
            if (cell.SpecialTile is ICreature)
            {
                (cell.SpecialTile as ICreature).TakeDamage(1, "Bomb Explosion");
                NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.ExplosionDamage);
            }
            if (cell.SpecialTile is Bomb_Cell)
            {
                (cell.SpecialTile as IUsable).Use();
            }
        }

        RemoveBorder();
        GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);
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
