using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb_Cellcs : ISpecialTile, IFragile
{
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }

    public CellScript ParentCell {get; private set;}
    public string Effect_Url {get;set;}
    public string Icon_Url {get;set;}

    public bool Active {get;set;} = true;

    public bool IsReadyToUse => ((GameManager.instance.CurrentTurnNumber-_spawnTurnNumber) > _turnsRequiredToActivate);


    private int _turnsRequiredToActivate = 5;
    private int _spawnTurnNumber;

    internal TickScript TickCounter {get;set;}
    

    public Bomb_Cellcs(CellScript parent, string name, string effect_Url, string icon_Url )
    {
        _spawnTurnNumber = GameManager.instance.CurrentTurnNumber;

        this.ParentCell = parent;
        this.Type = TileTypes.bomb;
        this.Name = name;

        this.Effect_Url = effect_Url;
        this.Icon_Url = icon_Url;

        // Debug.Log("pomyslnie utworzono Bombę, dla pola skojarzonego z pozycją "+ ParentCell.CurrentPosition);  

        var ticker = GameManager.instance.InstantiateTicker(this);
        ParentCell.Trash.Add(ticker);      
        this.TickCounter = ticker.GetComponentInChildren<TickScript>();
        this.TickCounter.parent = ParentCell;
        Debug.Log(ticker.name);
    }
    public List<CellScript> CellsToDestroy = new List<CellScript>();
    public void MakeAction()
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
            GridManager.CellGridTable[cell.CurrentPosition].DamagedTimes ++;
        }
        
        GameManager.instance.Countdown_SendToGraveyard(0.5f, CellsToDestroy);
    }

    public void MakeAction(Vector2Int _position, Vector2Int _fromDirection)
    {
        if(Active == false) return;
            Active = false;
        Debug.Log("EXPLODE !");

        AddCellsToDestroyList(_position, _fromDirection);

        foreach (var cell in CellsToDestroy.Where(cell=> cell != null))
        {
         //   Debug.Log(cell.CurrentPosition);
        //    Debug.Log(Effect_Url);

            cell.AddEffectImage(imageUrl: Effect_Url);
            GridManager.CellGridTable[cell.CurrentPosition].DamagedTimes ++;
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

    void IFragile.DetonateOnMove(Vector2Int nextPosition, Vector2Int direction)
    {
        // TODO: need to be executed after before explosion ends make chain
       Debug.Log("DETONATD ON MOVE");
       MakeAction(nextPosition, direction);
       // TODO: mark as next to explode ater before
    }
}
