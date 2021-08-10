using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector2Int StartingPlayerPosition;
    [SerializeField] GridManager _gridManager;
    public List<GameObject> specialEffectList = new List<GameObject>();
    public static CellScript Player;

    public static GameManager instance;
    private void Awake() {
        if(instance == null)
            instance = this;
        else
            Destroy(this);
    }
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

       GridManager.CellGridTable[randomPosition].SpecialTile = 
        new Bomb_Cellcs(
            GridManager.CellGridTable[randomPosition], 
            randomPosition, 
            TileTypes.bomb, 
            name: "Mina przeciwpiechotna",
            effect: "bomb_explosion_image",
            icon: "bomb_icon"
        );
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
