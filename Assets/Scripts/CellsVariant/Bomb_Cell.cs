using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb_Cell : ISpecialTile, IFragile, ISelectable
{
    #region core
    public CellScript ParentCell {get; private set;}
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public string Icon_Url {get;set;}

    #endregion

    #region Bomb-specific
    public string Effect_Url {get;set;}
    public bool IsReadyToUse => ((GameManager.instance.CurrentTurnNumber-_spawnTurnNumber) > TurnsRequiredToActivate);
    public int TurnsRequiredToActivate {get; private set;}
    private int _spawnTurnNumber;
    public TickScript TickCounter {get;set;}
    public GameObject Border { get; set; }
    public bool IsHighlighted { get; set; }
    public int DAMAGE {get; private set;} = 1;

    public List<CellScript> CellsToDestroy = new List<CellScript>();
    #endregion


      public List<(Action action, string description, ActionIcon icon)> AvaiableActions { get; private set;} = new List<(Action action, string description, ActionIcon icon)>();

    public Bomb_Cell(CellScript parent, string name, string effect_Url, string icon_Url, int turnsRequiredToActivate )
    {
        _spawnTurnNumber = GameManager.instance.CurrentTurnNumber;

        this.ParentCell = parent;
        this.Type = TileTypes.bomb;
        this.Name = name;

        this.Effect_Url = effect_Url;
        this.Icon_Url = icon_Url;
        this.TurnsRequiredToActivate = turnsRequiredToActivate;

        //Debug.Log("pomyslnie utworzono Bombę, dla pola skojarzonego z pozycją "+ ParentCell.CurrentPosition);  

        var ticker = GameManager.instance.InstantiateTicker(this);
        ParentCell.Trash.Add(ticker);      
        this.TickCounter = ticker.GetComponentInChildren<TickScript>();
        this.TickCounter.parent = ParentCell;
//
        parent.IsWalkable = true;

        AvaiableActions.Add((()=>Use(),"Detonate", ActionIcon.Bomb));
        AvaiableActions.Add((null,"WIP: Reset Timmer",ActionIcon.Timer));
        AvaiableActions.Add((null,"WIP: Disarm",ActionIcon.Delete));

        NotificationManger.CreateNewNotificationElement(this);
    }
    public void OnClick_MakeAction()
    {        
        if(IsReadyToUse == false) return;
        {
            Use();
        }
    }
    public bool IsUsed {get; private set;} = false;
    public void Use()
    {
            if(IsUsed == true) 
            {
                //Debug.Log("Already used");
                return;
            }
            IsUsed = true;

            //Debug.Log("EXPLODEEEEEEEEEEEEEEE !");
          

            AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);

            foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
            {   
               // Debug.Log(cell.CurrentPosition);    
               // Debug.Log(Effect_Url);

                cell.AddEffectImage(imageUrl: Effect_Url);
                if(cell.SpecialTile is ICreature)
                {
                   (cell.SpecialTile as ICreature).TakeDamage(DAMAGE, "Bomb Explosion");
                   NotificationManger.TriggerActionNotification(cell.SpecialTile as ISelectable, NotificationManger.AlertCategory.ExplosionDamage);
                } 
                if(cell.SpecialTile is Bomb_Cell)
                {
                   (cell.SpecialTile as IUsable).Use();
                } 
            }
            
            RemoveBorder();
            
            GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);
    }
    private void AddCellsToDestroyList(Vector2Int nextPosition, Vector2Int direction)
    {
        if (GridManager.CellGridTable.ContainsKey(nextPosition+2*direction) && !CellsToDestroy.Contains(GridManager.CellGridTable[nextPosition+2*direction]))
            CellsToDestroy.Add(GridManager.CellGridTable[nextPosition+2*direction]);

        if (GridManager.CellGridTable.ContainsKey(nextPosition + Vector2Int.up) && !CellsToDestroy.Contains(GridManager.CellGridTable[nextPosition+ Vector2Int.up]))
            CellsToDestroy.Add(GridManager.CellGridTable[nextPosition + Vector2Int.up]);

        if (GridManager.CellGridTable.ContainsKey(nextPosition + Vector2Int.down) && !CellsToDestroy.Contains(GridManager.CellGridTable[nextPosition+ Vector2Int.down]))
            CellsToDestroy.Add(GridManager.CellGridTable[nextPosition + Vector2Int.down]);

        if (GridManager.CellGridTable.ContainsKey(nextPosition + Vector2Int.left) && !CellsToDestroy.Contains(GridManager.CellGridTable[nextPosition+ Vector2Int.left]))
            CellsToDestroy.Add(GridManager.CellGridTable[nextPosition + Vector2Int.left]);

        if (GridManager.CellGridTable.ContainsKey(nextPosition + Vector2Int.right) && !CellsToDestroy.Contains(GridManager.CellGridTable[nextPosition+ Vector2Int.right]))
            CellsToDestroy.Add(GridManager.CellGridTable[nextPosition + Vector2Int.right]);
    }
    public void ActionOnMove(Vector2Int nextPosition, Vector2Int direction)
    {
        if(IsUsed) return;
        IsUsed = true;

        AddCellsToDestroyList(nextPosition, direction);

        foreach (var cell in CellsToDestroy.Where(cell => cell != null))
        {
            cell.AddEffectImage(imageUrl: Effect_Url);
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

    private void RemoveBorder()
    {
        IsHighlighted = false;
        if (Border != null)
        {
            GameObject.Destroy(Border.gameObject);
        }
    }

    // public void AddActionToQUEUE()
    // {
    //     TaskManager.AddToActionQueue(
    //         $"Manual detonate Bomb",
    //         () =>
    //         {
    //             if(IsUsed)  return (false,"bomba jest juz nieaktywna");
    //             if(IsReadyToUse == false) return (false,"bomba nie jest jeszcze gotowa nieaktywna");
    //             IsUsed = true; 
    //             IsHighlighted = false;
    //             Debug.Log("EXPLODE !");
                
    //             AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);

    //             foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
    //             {   
    //                 //Debug.Log(cell.CurrentPosition);    
    //                 //Debug.Log(Effect_Url);

    //                 cell.AddEffectImage(imageUrl: Effect_Url);
    //                 if(GridManager.CellGridTable[cell.CurrentPosition].SpecialTile is ICreature) 
    //                 {
    //                     (GridManager.CellGridTable[cell.CurrentPosition].SpecialTile as ICreature).TakeDamage(1,"Bomb explosion");
    //                 }
    //             }
                
    //             GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);

    //             return (true,"succes");
    //         }
    //     );
    // }
    

}
