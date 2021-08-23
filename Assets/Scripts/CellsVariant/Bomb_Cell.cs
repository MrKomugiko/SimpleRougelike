using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb_Cell : ISpecialTile, IFragile, ITaskable
{
    #region core
    public CellScript ParentCell {get; private set;}
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public string Icon_Url {get;set;}

    #endregion

    #region Bomb-specific
    public string Effect_Url {get;set;}
    public bool Active {get;set;} = true;
    public bool IsReadyToUse => ((GameManager.instance.CurrentTurnNumber-_spawnTurnNumber) > TurnsRequiredToActivate);
    public int TurnsRequiredToActivate {get; private set;}
    private int _spawnTurnNumber;
    internal TickScript TickCounter {get;set;}    
    public List<CellScript> CellsToDestroy = new List<CellScript>();
    #endregion


    

    public Bomb_Cell(CellScript parent, string name, string effect_Url, string icon_Url, int turnsRequiredToActivate )
    {
        _spawnTurnNumber = GameManager.instance.CurrentTurnNumber;

        this.ParentCell = parent;
        this.Type = TileTypes.bomb;
        this.Name = name;

        this.Effect_Url = effect_Url;
        this.Icon_Url = icon_Url;
        this.TurnsRequiredToActivate = turnsRequiredToActivate;

        Debug.Log("pomyslnie utworzono Bombę, dla pola skojarzonego z pozycją "+ ParentCell.CurrentPosition);  

        var ticker = GameManager.instance.InstantiateTicker(this);
        ParentCell.Trash.Add(ticker);      
        this.TickCounter = ticker.GetComponentInChildren<TickScript>();
        this.TickCounter.parent = ParentCell;
//
        parent.IsWalkable = true;
    }
    public void OnClick_MakeAction()
    {        
        if(TaskManager.TaskManagerIsOn == false)
        {
            Use();
        }
        else
        {
            AddActionToQUEUE();
        }
    }
    public void Use()
    {
          if(Active == false) return;
                Active = false;
            Debug.Log("EXPLODE !");
            
            AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);

            foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
            {   
                Debug.Log(cell.CurrentPosition);    
                Debug.Log(Effect_Url);

                cell.AddEffectImage(imageUrl: Effect_Url);
                if(GridManager.CellGridTable[cell.CurrentPosition].SpecialTile is ICreature)
                {
                   (GridManager.CellGridTable[cell.CurrentPosition].SpecialTile as ICreature).TakeDamage(1, "Bomb Explosion");
                } 
                if(cell.SpecialTile is Bomb_Cell)
                {
                   (cell.SpecialTile as IUsable).Use();
                } 
            }
            
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
        // TODO: need to be executed after before explosion ends make chain
       Debug.Log("DETONATD ON MOVE");
        if(Active == false) return;
                Active = false;
            Debug.Log("EXPLODE !");
            
            AddCellsToDestroyList(nextPosition, direction);

            foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
            {   
                Debug.Log(cell.CurrentPosition);    
                Debug.Log(Effect_Url);

                cell.AddEffectImage(imageUrl: Effect_Url);
                if(cell.SpecialTile is ICreature)
                {
                   (cell.SpecialTile as ICreature).TakeDamage(1, "Bomb Explosion");
                } 
                if(cell.SpecialTile is Bomb_Cell)
                {
                   (cell.SpecialTile as IUsable).Use();
                } 
            }
            
            GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);
       // TODO: mark as next to explode ater before
    }
    public void AddActionToQUEUE()
    {
        TaskManager.AddToActionQueue(
            $"Manual detonate Bomb",
            () =>
            {
                if(Active == false) return (false,"bomba jest juz nieaktywna");

                Active = false;
                Debug.Log("EXPLODE !");
                
                AddCellsToDestroyList(ParentCell.CurrentPosition, Vector2Int.zero);

                foreach(var cell in CellsToDestroy.Where(cell=> cell != null))
                {   
                    Debug.Log(cell.CurrentPosition);    
                    Debug.Log(Effect_Url);

                    cell.AddEffectImage(imageUrl: Effect_Url);
                    if(GridManager.CellGridTable[cell.CurrentPosition].SpecialTile is ICreature) 
                    {
                        (GridManager.CellGridTable[cell.CurrentPosition].SpecialTile as ICreature).TakeDamage(1,"Bomb explosion");
                    }
                }
                
                GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);

                return (true,"succes");
            }
        );
    }
}
