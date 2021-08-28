using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject _cellPrefab;
    [SerializeField] public Vector2Int _gridSize;


    public static Queue<Action> CurrentExplosionQueue = new Queue<Action>();
    public static List<CellScript> destroyedTilesPool = new List<CellScript>();
    public static Dictionary<Vector2Int,CellScript> CellGridTable = new Dictionary<Vector2Int, CellScript>();
    public static GridManager instance;


    internal List<(ICreature creature, int damage)> DamageMap = new List<(ICreature creature, int damage)>();
    internal List<CellScript> DestroyedCells = new List<CellScript>();
    internal List<CellScript> BombDetonatedByChainReaction = new List<CellScript>();


    private void Awake() {
        instance = this;
    }
    public void Start()
    {   
        for(int x = 0; x<_gridSize.x; x++)
        {
            for(int y = 0; y<_gridSize.y; y++)
            {
                var newCell = Instantiate(_cellPrefab,this.transform,false).GetComponent<CellScript>();
                newCell.SetCell(new Vector2Int(x,y), false);
                if(newCell.CurrentPosition == GameManager.instance.StartingPlayerPosition) 
                    newCell.AssignType(TileTypes.player);
                else
                    newCell.AssignType(GetRandomType());
                    
                CellGridTable.Add(new Vector2Int(x,y),newCell);
            }   
        }
    }
    
    public static TileTypes GetRandomType()
    {
        int rng = UnityEngine.Random.Range(0,101);
        if(rng >= 0 && rng <60)
            return TileTypes.grass; // 55%

        if(rng >= 60 && rng <80)
            return TileTypes.wall; // 10%

        if(rng >= 80 && rng <90)
            return TileTypes.bomb; // 10%
            
        if(rng >= 90 && rng <99)
            return TileTypes.treasure; // 5%

        if(rng >= 99)
            return TileTypes.monster; // 5%
        
        return TileTypes.undefined;

    }
    // [ContextMenu("ForceRefillIncorrectAll")]
    // public void ForceRefillIncorrectAll()
    // {
    //     Debug.LogError("PERFORMED FORCE REPLACE TILES TO CORRECT POSITION");
    //     float positionMultipliferX = CellGridTable.First().Value._recTransform.rect.size.x;
    //     float positionMultipliferY = CellGridTable.First().Value._recTransform.rect.size.y;

    //     int i = 0;
    //     foreach(var cell in CellGridTable)
    //     {   
    //         i++;
    //         Vector2 correctPosition = new Vector2(cell.Value.CurrentPosition.x * positionMultipliferX, cell.Value.CurrentPosition.y * positionMultipliferY);
    //         if((Vector2)cell.Value._recTransform.localPosition != correctPosition)
    //         {
    //             cell.Value._recTransform.localPosition = correctPosition;
    //         }
    //     }
    //     Debug.LogError(i);
    // }
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
        CellGridTable[positionToFill].AssignType(GetRandomType());      
    }
    public static void SendToGraveyard(Vector2Int cellPosition)
    {
        if(CellGridTable.Where(cell=>cell.Key == cellPosition).FirstOrDefault().Value == null)
            return;
        // 0# zwolnienei miejsca w ktore gracz przechodzi
        destroyedTilesPool.Add(CellGridTable[cellPosition]);
        
        CellGridTable[cellPosition].SetCell(new Vector2Int(-1, -1),false);
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
        print("execute destroying once");
        if(DestroyedCells.Count == 0) return;
        var copy_destroyedCelld = DestroyedCells.ToList();
        DestroyedCells.Clear();
        
        StartCoroutine(GameManager.instance.routine_SendToGraveyard(0.5f,copy_destroyedCelld));
    }


    
    internal void CascadeExploding()
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
        
        StartCoroutine(GameManager.instance.routine_SendToGraveyard(.55f,anotherDamagedCells));
    }








    public static void TrySwapTiles(CellScript movedCell, Vector2Int newPosition)
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
}
