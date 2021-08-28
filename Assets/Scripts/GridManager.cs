using System;
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
    public static GridManager instance;

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
        if(rng >= 0 && rng <65)
            return TileTypes.grass; // 65%
        if(rng >= 65 && rng <85)
            return TileTypes.wall; // 20%
        if(rng >= 85 && rng <90)
            return TileTypes.bomb; // 5%
        if(rng >= 90 && rng <95)
            return TileTypes.treasure; // 5%
        if(rng >= 95)
            return TileTypes.monster; // 5%
        
        return TileTypes.undefined;

    }
    [ContextMenu("ForceRefillIncorrectAll")]
    public void ForceRefillIncorrectAll()
    {
        Debug.LogError("PERFORMED FORCE REPLACE TILES TO CORRECT POSITION");
        float positionMultipliferX = CellGridTable.First().Value._recTransform.rect.size.x;
        float positionMultipliferY = CellGridTable.First().Value._recTransform.rect.size.y;

        int i = 0;
        foreach(var cell in CellGridTable)
        {   
            i++;
            Vector2 correctPosition = new Vector2(cell.Value.CurrentPosition.x * positionMultipliferX, cell.Value.CurrentPosition.y * positionMultipliferY);
            if((Vector2)cell.Value._recTransform.localPosition != correctPosition)
            {
                cell.Value._recTransform.localPosition = correctPosition;
            }
        }
        Debug.LogError(i);
    }
    public static void FillGaps()
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
    public static void TrySwapTiles(CellScript movedCell, Vector2Int newPosition)
    {
       // sprawdz czy pozycja z któą sie chcesz zamienic jest specialnym / fragile i czy jest aktualnie aktywnym
        if(newPosition == GameManager.Player_CELL.CurrentPosition)
        {
           //Debug.LogWarning("monster nie moze zamienic sie miejscami z graczem");
            return;
        }
        if(CellGridTable[newPosition].SpecialTile != null)
        {
            if(CellGridTable[newPosition].SpecialTile is Monster_Cell){
                //Debug.LogWarning("nie zamieniaj sie miejscem z innym monsterkiem");
                return;
            }
            
            //Debug.LogWarning("proba swapniecia miejsc ze specialnym tilesem");

            if(CellGridTable[newPosition].SpecialTile is IFragile)
            {
                if((CellGridTable[newPosition].SpecialTile as IUsable).IsReadyToUse){
                   // Debug.LogWarning("ten tiles jest delikatny i wybuchnie przy ruchu, = nie zamieniaj miejsc, poprostu go zdetonuj");
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
