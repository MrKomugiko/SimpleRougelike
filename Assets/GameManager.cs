using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector2Int StartingPlayerPosition;
    [SerializeField] GridManager _gridManager;

    public static CellScript Player;

    private void Start() {
        PlacePlayerOnGrid();
    }
    private void PlacePlayerOnGrid()
    {
      GridManager.CellGridTable[StartingPlayerPosition].AssignType(TileTypes.player);
      Player = GridManager.CellGridTable[StartingPlayerPosition];
    }
    
    public void SpawnBomb()
    {
        int x = UnityEngine.Random.Range(0,_gridManager._gridSize.x);
        int y = UnityEngine.Random.Range(0,_gridManager._gridSize.y);

        Vector2Int randomPosition = new Vector2Int(x,y);
        print($"spawn bomby na losową pozycje: {randomPosition}.");
    }
}

public enum TileTypes
{
    undefined = 0,
    player,
    wall,
    grass,
    bomb,
    treasure,
    monster
}
