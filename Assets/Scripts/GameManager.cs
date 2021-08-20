using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TurnCounter_TMP;
    [SerializeField] private TextMeshProUGUI GoldCounter_TMP;
    [SerializeField] private TextMeshProUGUI ExperienceCounter_TMP;
    [SerializeField] private Vector2Int StartingPlayerPosition;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject TickCounterPrefab;

    public List<GameObject> specialEffectList = new List<GameObject>();
    public List<Sprite> SpritesList = new List<Sprite>();
    public List<GameObject> ExtrasPrefabList = new List<GameObject>();

    public static CellScript Player_CELL;
    public static List<CellScript> DamagedCells = new List<CellScript>();
    public static GameManager instance;

    public bool WybuchWTrakcieWykonywania = false;
    private int _currentTurnNumber = 0;
    public int CurrentTurnNumber
    {
        get => _currentTurnNumber;
        set
        {
            _currentTurnNumber = value;
            foreach (var tile in GridManager.CellGridTable.Where(cell => cell.Value.SpecialTile != null).Select(c => c.Value))
            {
                IncrementTickCounterOnBombCells(tile);

                if (tile.SpecialTile is IFragile == false) continue;

                ActivateSpecialTileIfIsReady(tile);
            }

        }
    }

    private void FixedUpdate() {
     //   NodeGrid.UpdateMapObstacleData();
    }
    public void AddTurn()
    {
        _currentTurnNumber = Int32.Parse(TurnCounter_TMP.text);
        TurnCounter_TMP.SetText((CurrentTurnNumber += 1).ToString());
        print("dodanie tury");
    }
    public void AddGold(int value)
    {
        int currentTurnnumber = Int32.Parse(GoldCounter_TMP.text);
        GoldCounter_TMP.SetText((currentTurnnumber += value).ToString());
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void SpawnBomb()
    {
        int x = UnityEngine.Random.Range(0, _gridManager._gridSize.x);
        int y = UnityEngine.Random.Range(0, _gridManager._gridSize.y);

        Vector2Int randomPosition = new Vector2Int(x, y);
        print($"spawn bomby na losową pozycje: {randomPosition}.");

        GridManager.CellGridTable[randomPosition].SpecialTile =
         new Bomb_Cell(
             GridManager.CellGridTable[randomPosition],
             name: "Mina przeciwpiechotna",
             effect_Url: "bomb_explosion_image",
             icon_Url: "bomb_icon"
         );
    }
    public GameObject InstantiateTicker(Bomb_Cell bomb_Cellcs)
    {
        return Instantiate(TickCounterPrefab, bomb_Cellcs.ParentCell.transform);
    }
    public void Countdown_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
        foreach (var damagedCell in GameManager.DamagedCells)
        {
            if (cellsToDestroy.Contains(damagedCell))
            {
                cellsToDestroy.Remove(damagedCell);
            }
        }
        StartCoroutine(routine_SendToGraveyard(time, cellsToDestroy));
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    private void Start()
    {

        Init_PlacePlayerOnGrid();
    }
    // private void MoveAllMonstersToPlayerDirection()
    // {
    //   //  Grid.PopulateGridData();
    //     var temp = GridManager.CellGridTable.Where(cell => cell.Value.Type == TileTypes.monster).ToList();
    //     int liczbaMonsterkow = temp.Count;

    //         foreach (var monster in temp)
    //         {
    //             // 1 wygeneruj dla potwora ścieżke do pozycji gracza
    //             Pathfinding pathfinder;
    //             if (monster.Value.TryGetComponent<Pathfinding>(out pathfinder))
    //             {
    //                 monster.Value.gameObject.AddComponent<Pathfinding>();
    //             }
    //             pathfinder = monster.Value.GetComponentInChildren<Pathfinding>();

    //            // pathfinder.end_pos = Player_CELL.CurrentPosition;
    //             //pathfinder.FindPath();

    //             //GridManager.CascadeMoveTo(monster.Value,(pathfinder.FinalPath[0].Coordination));
    //             if (!(pathfinder.FinalPath.Count > 0)) continue;

    //             // if (GridManager.CellGridTable[pathfinder.FinalPath[0].Coordination].Type != TileTypes.monster)
    //             // {
    //                 if (GridManager.CellGridTable[pathfinder.FinalPath[0].Coordination].Type == TileTypes.player)
    //                 {
    //                     print("PLAYER WAS HITTED BY MONSTER");
    //                     continue;
    //                 }
    //                 GridManager.SwapTiles(movedCell: monster.Value, newPosition: pathfinder.FinalPath[0].Coordination);
    //             // }
    //         }
        
    // }
    private void Init_PlacePlayerOnGrid()
    {
        Player_CELL = GridManager.CellGridTable[StartingPlayerPosition];
        Player_CELL.Trash.ForEach(t => Destroy(t.gameObject));
        Player_CELL.Trash.Clear();
        Player_CELL.AssignType(TileTypes.player);
    }
    private IEnumerator routine_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
        // TODO: jakos to uprościc
        DamagedCells.AddRange(cellsToDestroy);
        yield return new WaitWhile(() => WybuchWTrakcieWykonywania);
        WybuchWTrakcieWykonywania = true;
        yield return new WaitForSeconds(time);

        foreach (var cell in cellsToDestroy.Where(c => c != null).ToList())
        {
            if (cell.Type == TileTypes.player) continue;

            GridManager.SendToGraveyard(cell.CurrentPosition);
            cell.SpecialTile = null;
        }

        cellsToDestroy.Where(c => c != null).ToList().ForEach(cell => cell.Trash.ForEach(trash => Destroy(trash.gameObject)));
        cellsToDestroy.Where(c => c != null).ToList().ForEach(cell =>
        {
            cell.SpecialTile = null;
            cell.Trash.Clear();
        }
            );
        cellsToDestroy.ForEach(cell => DamagedCells.Remove(cell));

        
        WybuchWTrakcieWykonywania = false;
        if (DamagedCells.Count == 0)
        {
            GridManager.FillGaps();
        }
        
        yield return null;
    }
    private static void IncrementTickCounterOnBombCells(CellScript tile)
    {
        if (tile.SpecialTile is Bomb_Cell)
        {
            if ((tile.SpecialTile as Bomb_Cell).TickCounter != null)
            {
                print("tick");
                (tile.SpecialTile as Bomb_Cell).TickCounter.AddTick(1);
            }
        }
    }
    private static void ActivateSpecialTileIfIsReady(CellScript tile)
    {
        if (tile.SpecialTile.IsReadyToUse == true)
        {
            tile.Trash.ForEach(trash =>
            {
                if (trash != null)
                {
                    var trash_SR = trash.GetComponent<SpriteRenderer>();
                    if (trash_SR != null)
                    {
                        if (trash_SR.name.Contains("icon"))
                        {
                            trash_SR.color = Color.red;
                        }
                    }
                }
            });
        }
    }
    [ContextMenu("Start simulation")]
    public void StartSimulation()
    {
        StartCoroutine(SimulateGameplay());
    }
    private IEnumerator SimulateGameplay()
    {

        for (; ; )
        {
            if (GridManager.CellGridTable.Where(cell => cell.Value.Type != TileTypes.wall && cell.Value.Type != TileTypes.player).Count() > 0)
            {
                foreach (var tile in GridManager.CellGridTable.Where(cell => cell.Value.Type != TileTypes.wall && cell.Value.Type != TileTypes.player))
                {
                    print($"click [{tile.Value.CurrentPosition}]");
                    tile.Value._button.onClick.Invoke();
                    break;
                }
                yield return new WaitForSeconds(.5f);
            }
            else
            {
                print("no more avaiable moves");
                break;
            }
        }

        yield return null;

    }
}