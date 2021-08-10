using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject _cellPrefab;
    [SerializeField] public Vector2Int _gridSize;

    public static List<CellScript> destroyedTilesPool = new List<CellScript>();
    public static Dictionary<Vector2Int,CellScript> CellGridTable = new Dictionary<Vector2Int, CellScript>();

    public void PlaceBombAtRandomPosition()
    {

    }

    void Awake()
    {   
        for(int x = 0; x<_gridSize.x; x++)
        {
            for(int y = 0; y<_gridSize.y; y++)
            {
                var newCell = Instantiate(_cellPrefab,this.transform,false).GetComponent<CellScript>();
                newCell.SetCell(new Vector2Int(x,y), false);
                newCell.AssignType(GetRandomType());
                CellGridTable.Add(new Vector2Int(x,y),newCell);
            }   
        }
    }
    public static TileTypes GetRandomType()
    {
        int rng = UnityEngine.Random.Range(0,100);
        if(rng > 0 && rng <60)
            return TileTypes.grass; // 60%
        if(rng >= 60 && rng <90)
            return TileTypes.wall; // 30%
        if(rng >= 90 && rng <95)
            return TileTypes.treasure; // 5%
        if(rng >= 95)
            return TileTypes.monster; // 5%
        
        return TileTypes.undefined;

    }
    public void FillGaps()
    {
      if(destroyedTilesPool.Count() == 0) return;
      var globalFillDirection = new Vector2Int(0,-1); // (Z gory do dołu)
      
      foreach(var tile in GridManager.CellGridTable.OrderByDescending(cell=>cell.Key.y).Where(t=>t.Value == null))
      {
            // 1. sprawdz czy nad tilesem jest cos co mozna ruszyc w doł
            //  jezeli nie, wstaw tu losowego tilesa i usun wpis z listy

            if(CellGridTable.ContainsKey(tile.Key-globalFillDirection))         
            {
              // print("mozna cos zrzuci na dół");
                // kaskadowe zrzucanie klockow z gory na dol
                CascadeMoveTo(CellGridTable[tile.Key-globalFillDirection],tile.Key);
                continue;
            }
            else
            {
                var positionToFill = tile.Key;
                // sprawdzam czy pod pustym miejscem jest koljen puste miejsce i jak głeboko zaczyna sie dziura 
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
        CellGridTable[positionToFill] = destroyedTilesPool.First();
        destroyedTilesPool.Remove(destroyedTilesPool.First());
        CellGridTable[positionToFill].SetCell(positionToFill,false);
        CellGridTable[positionToFill].AssignType(GetRandomType());
    }
    public static void SendToGraveyard(Vector2Int positionToMove)
    {
        if(CellGridTable.Where(cell=>cell.Key == positionToMove).FirstOrDefault().Value == null)
            return;
        // 0# zwolnienei miejsca w ktore gracz przechodzi
        destroyedTilesPool.Add(CellGridTable[positionToMove]);
        CellGridTable[positionToMove].SetCell(new Vector2Int(-1, -1),false);
        CellGridTable[positionToMove] = null;
    }
    public static void CascadeMoveTo(CellScript movedCell, Vector2Int positionToMove)
    {
        // BLOCK MOVE PLAYER IN PLAYER POSIIOTN 
        if(movedCell.CurrentPosition == positionToMove) return;
        
        Vector2Int moveDirection = positionToMove - movedCell.CurrentPosition;
        //print("player move in direction :" + moveDirection.ToString());

        SendToGraveyard(positionToMove);

        for (int i = 0; ; i++)
        {
            var newPosition = positionToMove - i * moveDirection;
            var oldPosition = positionToMove - i * moveDirection - moveDirection;;

            // 1# przeniesienie gracza na puste juz miejsce "positionToMove"
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
}
