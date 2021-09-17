using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject _cellPrefab;
    [SerializeField] public Vector2Int _gridSize;
    public static List<CellScript> destroyedTilesPool = new List<CellScript>();
    public static Dictionary<Vector2Int,CellScript> CellGridTable = new Dictionary<Vector2Int, CellScript>();
    public static GridManager instance;
    internal List<(ILivingThing creature, int damage)> DamageMap = new List<(ILivingThing creature, int damage)>();
    internal List<CellScript> DestroyedCells = new List<CellScript>();
    internal List<CellScript> BombDetonatedByChainReaction = new List<CellScript>();

    private void Awake() {
        instance = this;
    }
    public void CreateEmptyGrid()
    {   
        for(int x = 0; x<_gridSize.x; x++)
        {
            for(int y = 0; y<_gridSize.y; y++)
            {
                var newCell = Instantiate(_cellPrefab,this.transform,false).GetComponent<CellScript>();
                newCell.SetCell(new Vector2Int(x,y), false);
                CellGridTable.Add(new Vector2Int(x,y),newCell);
            }   
        }
    }
    [ContextMenu("RESET")]
    public void ResetGridToDefault()
    {
        Debug.Log("reset");
        foreach(var cell in CellGridTable)
        {
            cell.Value.SpecialTile = null;
            cell.Value.SetCell(cell.Key,false);
        }
    }
    public void RandomizeDataOnGrid()
    {
        foreach(var cell in CellGridTable)
        {
            if(cell.Key.x==0 || cell.Key.x==_gridSize.x-1||cell.Key.y==0 || cell.Key.y==_gridSize.y-1 )
            {
                cell.Value.AssignType(TileTypes.grass);
                cell.Value.isWalkable = false;
            }
            else
                cell.Value.AssignType(GetRandomType());
        }
        
        CellGridTable[new Vector2Int(0,4)].AssignType(TileTypes.grass);
        CellGridTable[new Vector2Int(0,4)].isWalkable = true;

        CellGridTable[new Vector2Int(8,4)].AssignType(TileTypes.grass);
        CellGridTable[new Vector2Int(8,4)].isWalkable = true;

        CellGridTable[new Vector2Int(4,0)].AssignType(TileTypes.grass);
        CellGridTable[new Vector2Int(4,0)].isWalkable = true;

        CellGridTable[new Vector2Int(4,8)].AssignType(TileTypes.grass);
        CellGridTable[new Vector2Int(4,8)].isWalkable = true;        
        // wyczysc mape z pustych skrzynek
        foreach(var cell in CellGridTable.Values)
        {
            if(cell.SpecialTile is Treasure_Cell)
            {
                (cell.SpecialTile as Treasure_Cell).RemoveFromMapIfChesIsEmpty();
            }
        }

        
    }
    public static TileTypes GetRandomType()
    {
        int rng = UnityEngine.Random.Range(0,101);
        
        if(rng >= 0 && rng <75)
            return TileTypes.grass; // 55%

        if(rng >= 75 && rng <85)
            return TileTypes.wall; // 10%

        if(rng >= 85 && rng <90)
            return TileTypes.bomb; // 10%

        if(rng >= 90 && rng <95)
            return TileTypes.treasure; // 5%

        if(rng >= 95)
            return TileTypes.monster; // 5%

        return TileTypes.undefined;
    }
    public static void FillGaps()
    {      
        if(destroyedTilesPool.Count() == 0) 
        {
            GridManager.instance.CascadeExploding();         
            return;
        }
      var globalFillDirection = new Vector2Int(0,-1); // (Z gory do dołu)
      foreach(var tile in GridManager.CellGridTable.OrderByDescending(cell=>cell.Key.y).Where(t=>t.Value == null))
      {
          if(CellGridTable.ContainsKey(tile.Key-globalFillDirection))         
            {
                CascadeMoveTo(CellGridTable[tile.Key-globalFillDirection],tile.Key);
                continue;
            }
            else
            {
                var positionToFill = tile.Key;
                for (int i = 1; ; i++)
                {
                    Vector2Int nextPosition = tile.Key + i * globalFillDirection;
                    if (CellGridTable.ContainsKey(nextPosition))
                    {
                        if (CellGridTable[nextPosition] == null)
                        {
                            positionToFill = nextPosition;
                            continue;
                        }
                    }
                    break;
                }
                AddNewCellIntoGridField(positionToFill);
                break;
            }
        }
      FillGaps();
  }
    public static void AddNewCellIntoGridField(Vector2Int positionToFill)
    {
        foreach(var pool in destroyedTilesPool.Where(t=>t.Trash.Count>0))
        {
            pool.Trash.ForEach(t=>{Destroy(t.gameObject);});
            pool.Trash.Clear();
        }
        CellGridTable[positionToFill] = destroyedTilesPool.First();
        destroyedTilesPool.Remove(destroyedTilesPool.First());
        CellGridTable[positionToFill].SetCell(positionToFill,false);
        if(positionToFill == new Vector2Int(0,4))
        {
            CellGridTable[new Vector2Int(0,4)].AssignType(TileTypes.grass);
            CellGridTable[new Vector2Int(0,4)].isWalkable = true;
            return;
        }

        if(positionToFill == new Vector2Int(8,4))
        {
            CellGridTable[new Vector2Int(8,4)].AssignType(TileTypes.grass);
            CellGridTable[new Vector2Int(8,4)].isWalkable = true;
            return;
        }
        if(positionToFill == new Vector2Int(4,0))
        {
            CellGridTable[new Vector2Int(4,0)].AssignType(TileTypes.grass);
            CellGridTable[new Vector2Int(4,0)].isWalkable = true;
            return;
        }
        if(positionToFill == new Vector2Int(4,8))
        {
            CellGridTable[new Vector2Int(4,8)].AssignType(TileTypes.grass);
            CellGridTable[new Vector2Int(4,8)].isWalkable = true;
            return;
        }

        if(positionToFill.x==0 || positionToFill.x==GridManager.instance._gridSize.x-1||positionToFill.y==0 || positionToFill.y==GridManager.instance._gridSize.y-1 )
        {
                CellGridTable[positionToFill].AssignType(TileTypes.grass);
                CellGridTable[positionToFill].isWalkable = false;
        }
        else
        {
            CellGridTable[positionToFill].SetCell(positionToFill,false);
            CellGridTable[positionToFill].AssignType(GetRandomType());      
        }
 
    }
    public static void SendToGraveyard(Vector2Int cellPosition)
    {
        if(CellGridTable.Where(cell=>cell.Key == cellPosition).FirstOrDefault().Value == null)
            return;
        destroyedTilesPool.Add(CellGridTable[cellPosition]);
        CellGridTable[cellPosition].SetCell(new Vector2Int(-3, -3),false);
        CellGridTable[cellPosition].SpecialTile = null;
        CellGridTable[cellPosition] = null;
    }
    public static void CascadeMoveTo(CellScript movedCell, Vector2Int positionToMove)
    {
        if(movedCell.CurrentPosition == positionToMove) return;
        Vector2Int moveDirection = positionToMove - movedCell.CurrentPosition;
        SendToGraveyard(positionToMove);
        for (int i = 0; ; i++)
        {
            var newPosition = positionToMove - i * moveDirection;
            var oldPosition = positionToMove - i * moveDirection - moveDirection;;
            if (CellGridTable.ContainsKey(oldPosition))
            {
                CellGridTable[newPosition] = CellGridTable[oldPosition];
                CellGridTable[newPosition].SetCell(newPosition);
                CellGridTable[newPosition].Type = CellGridTable[newPosition].Type;
            }
            else
            {
                AddNewCellIntoGridField(newPosition);
                break;
            }
        }
    }
    internal void ExecuteExplodes()
    {
        if(DestroyedCells.Count == 0) return;
        var copy_destroyedCelld = DestroyedCells.ToList();
        DestroyedCells.Clear();
        StartCoroutine(GameManager.instance.routine_SendToGraveyard(0.4f,copy_destroyedCelld));
    }
    public void CascadeExploding()
    {
        var copy_BombDetonatedByChainReaction = BombDetonatedByChainReaction.ToList();
        if(copy_BombDetonatedByChainReaction.Count == 0) return;

        BombDetonatedByChainReaction.Clear();
        var anotherDamagedCells = new List<CellScript>();
        foreach(var lateBomb in copy_BombDetonatedByChainReaction)
        {
            if(lateBomb == null) continue;
            if(lateBomb.SpecialTile == null || lateBomb.SpecialTile is Bomb_Cell == false) continue;
            anotherDamagedCells.AddRange((lateBomb.SpecialTile as Bomb_Cell).GetDestroyedCellsFromCascadeContinueExploding());
        }
        if(anotherDamagedCells.Count == 0) return;;
        StartCoroutine(GameManager.instance.routine_SendToGraveyard(0.4f,anotherDamagedCells));
    }
    public static void SwapTiles(CellScript movedCell, Vector2Int newPosition)
    {
        if(newPosition == GameManager.Player_CELL.CurrentPosition)
            return;
        if(CellGridTable[newPosition].SpecialTile != null)
        {
            if(CellGridTable[newPosition].SpecialTile is Monster_Cell)
                return;
            if(CellGridTable[newPosition].SpecialTile is IFragile)
            {
                if((CellGridTable[newPosition].SpecialTile as IUsable).IsReadyToUse)
                {
                    (CellGridTable[newPosition].SpecialTile as IFragile).Use();
                    return;
                }
            }
        }
        var oldPosition = movedCell.CurrentPosition;
        var temp = CellGridTable[newPosition];
        CellGridTable[newPosition] = CellGridTable[oldPosition];
        CellGridTable[oldPosition] = temp;

        CellGridTable[newPosition].SetCell(newPosition);
        CellGridTable[oldPosition].SetCell(oldPosition);
    }
    public static bool DistanceCheck(ISpecialTile cell)    
    {
        if (Vector3Int.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition, (Vector3Int)cell.ParentCell.CurrentPosition) < 1.1f)
        {
            return true;
        }
        return false;
    }
}
