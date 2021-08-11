using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TurnCounter_TMP;
    [SerializeField] TextMeshProUGUI GoldCounter_TMP;
    [SerializeField] TextMeshProUGUI ExperienceCounter_TMP;
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

  

    public delegate void TestDelegate(Vector2Int position); // This defines what type of method you're going to call.
    
    public void Countdown_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
        StartCoroutine(routine_SendToGraveyard(time,cellsToDestroy));
    }
    private IEnumerator routine_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
        yield return new WaitForSeconds(time);

        foreach(var cell in cellsToDestroy)
        {
            if(cell.Type== TileTypes.player) continue;

            GridManager.SendToGraveyard(cell.CurrentPosition);
            cell.SpecialTile = null;
        }

        cellsToDestroy.ForEach(cell => cell.Trash.ForEach(trash => Destroy(trash.gameObject)));
        cellsToDestroy.ForEach(cell => cell.Trash.Clear());
        
        _gridManager.FillGaps();
        yield return null;
    }

    public void AddTurn()
    {
        int currentTurnnumber = Int32.Parse(TurnCounter_TMP.text);
        TurnCounter_TMP.SetText((currentTurnnumber++).ToString());
    }
    public void AddGold(int value)
    {
        int currentTurnnumber = Int32.Parse(GoldCounter_TMP.text);
        GoldCounter_TMP.SetText((currentTurnnumber+=value).ToString());
    }

    public List<Sprite> SpritesList = new List<Sprite>();
    public List<GameObject> ExtrasPrefabList = new List<GameObject>();

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
