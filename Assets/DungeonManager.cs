using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static DungeonRoomScript;
using static GameManager;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] Button W_BTN, D_BTN, S_BTN, A_BTN;
    public static DungeonManager instance;
    private void Awake()
    {
        instance = this;
    }
    [SerializeField] GameObject DungeonSelectionWindow;
    public int maxDungeonStage = 0;
    public int recentDungeonStage = 0;
    /* DungeonData dungeondata */
    /* private DungeonData ConfigureDungeonLevel(configurationdata)
    {

        return dungeondata;
    } 
    */
    [SerializeField] private GameObject DungeonCanvas;
    public void OpenDungeon(/* dungeondata */ )
    {
        DungeonSelectionWindow.SetActive(false);
        DungeonCanvas.SetActive(true);
        GridManager.instance.CreateEmptyGrid();
        GridManager.instance.RandomizeDataOnGrid(); // TODO:
        GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4, 4));

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.CurrentTurnNumber = 1;
        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        StartCoroutine(GameManager.instance.AddTurn());
    }

    public void DungeonClearAndGoToCamp()
    {
        // poodpinanie zalozonych itemkow w quickslocie gracza
        PlayerManager.instance._mainBackpack.ItemSlots.ForEach(slot =>
            {
                if (slot.IsInQuickSlot)
                {
                    slot.RemoveFromQuickSlot((int)slot.AssignedToQuickSlot);
                }
            }
        );

        Debug.Log("DUNGEON CLEAR AND BACK TO CAMP");
        //  recentDungeonStage = dungeonData.stage;
        recentDungeonStage++; // tymczasowe

        GameManager.instance._chestLootScript.Clear();
        if (NotificationManger.instance != null)
        {
            NotificationManger.instance.NotificationList.ForEach(n => Destroy(n.gameObject.transform.parent.gameObject));
            NotificationManger.instance.NotificationList.Clear();
        }
        foreach (var cell in GridManager.CellGridTable)
        {
            Destroy(cell.Value.gameObject);
        }
        GridManager.destroyedTilesPool.Clear();
        GridManager.CellGridTable.Clear();
        MenuScript.instance.CampCanvas.SetActive(true);
        GameObject.Find("BottomSection").GetComponent<AnimateWindowScript>().HideTabWindow();
        DungeonCanvas.SetActive(false);

        maxDungeonStage = recentDungeonStage > maxDungeonStage ? recentDungeonStage : maxDungeonStage;
    }

    [SerializeField] Image RoomWallsSprite;
    Vector2Int CurrentLocation;


    [ContextMenu("1. create current map backup")]
    public void MakeCurrentMapBackup(Room _room)
    {
        
        Debug.LogWarning("zrzut danych dla pokoju: "+_room.position);
        Dictionary<Vector2Int, MonsterData> _backup_Monsters = new Dictionary<Vector2Int, MonsterData>();
        Dictionary<Vector2Int, TreasureData> _backup_Treasures = new Dictionary<Vector2Int, TreasureData>();
        Dictionary<Vector2Int, BombData> _backup_Bombs = new Dictionary<Vector2Int, BombData>();
        HashSet<Vector2Int> _wallPositions = new HashSet<Vector2Int>();
        
        foreach (var cell in GridManager.CellGridTable)
        {
            if (cell.Value.SpecialTile is Monster_Cell)
            {
                _backup_Monsters.Add(cell.Key, (cell.Value.SpecialTile as Monster_Cell).MonsterData_Backup_DATA);
                continue;
            }

            if (cell.Value.SpecialTile is Treasure_Cell)
            {
                _backup_Treasures.Add(cell.Key, (cell.Value.SpecialTile as Treasure_Cell).TreasureData_Backup_DATA);
                continue;
            }

            if (cell.Value.SpecialTile is Bomb_Cell)
            {
                _backup_Bombs.Add(cell.Key, (cell.Value.SpecialTile as Bomb_Cell).BombData_Backup_DATA);
                continue;
            }

            if (cell.Value.Type == TileTypes.wall)
            {
                _wallPositions.Add(cell.Key);
                continue;
            }
        }
        RoomGridData _roomBackupData = new RoomGridData(_backup_Monsters, _backup_Treasures, _backup_Bombs, _wallPositions);
        _room.DATA = _roomBackupData;
    }
    [ContextMenu("2. Regenerate map from backup")]
    public void LoadGridForRoomData(Room _room)
    {
        foreach (var cell in GridManager.CellGridTable)
        {
            cell.Value.SpecialTile = null;

            if (cell.Key.x == 0 || cell.Key.x == GridManager.instance._gridSize.x - 1 || cell.Key.y == 0 || cell.Key.y == GridManager.instance._gridSize.y - 1)
            {
                cell.Value.AssignType(TileTypes.grass);
                cell.Value.SetCell(cell.Key, false);
                cell.Value.isWalkable = false;
            }
            else
            {
                if (_room.DATA.Backup_Monsters.ContainsKey(cell.Key))
                {
                    GridManager.CellGridTable[cell.Key].SpecialTile = new Monster_Cell(parent: cell.Value, _room.DATA.Backup_Monsters[cell.Key]);
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.monster;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.Backup_Treasures.ContainsKey(cell.Key))
                {
                    GridManager.CellGridTable[cell.Key].SpecialTile = new Treasure_Cell(parent: cell.Value, _room.DATA.Backup_Treasures[cell.Key]);
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.treasure;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.Backup_Bombs.ContainsKey(cell.Key))
                {
                    GridManager.CellGridTable[cell.Key].SpecialTile = new Bomb_Cell(parent: cell.Value, _room.DATA.Backup_Bombs[cell.Key]);
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.bomb;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.WallPositions.Contains(cell.Key))
                {
                    cell.Value.AssignType(TileTypes.wall);
                    cell.Value.SetCell(cell.Key, false);
                }
            }
        }

        // map border => invicible walls  excepted doors entrace
        GridManager.CellGridTable[new Vector2Int(0, 4)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(0, 4)].isWalkable = true;

        GridManager.CellGridTable[new Vector2Int(8, 4)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(8, 4)].isWalkable = true;

        GridManager.CellGridTable[new Vector2Int(4, 0)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(4, 0)].isWalkable = true;

        GridManager.CellGridTable[new Vector2Int(4, 8)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(4, 8)].isWalkable = true;

        // Player spawn
      //  GameManager.instance.Init_PlacePlayerOnGrid(BackupPlayerPosition);
        // PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();

        // start routine
        //DungeonManager.instance.RestartTurnRoutine();
    }

    [ContextMenu("generate new dung")]
    public void GenerateAndEnterDungeon()
    {
        Debug.Log("generate dungeon rooms");
        DungeonRoomScript.GenerateDungeonRooms();
        OpenDungeon();
        CurrentLocation = Vector2Int.zero;

        RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[Vector2Int.zero].doorsNameCode + "_OPEN").First();
        DungeonRoomScript.Dungeon[Vector2Int.zero].WasVisited = true;
    }
    [ContextMenu("move up")]
    public void MoveNExtRoom_Up()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
    
        var vector = Vector2Int.up;
         Debug.LogWarning("Move up from:"+CurrentLocation.ToString()+" to "+(CurrentLocation+vector));
        if (DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation + vector))
        {

            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode + "_OPEN").First();
            if(DungeonRoomScript.Dungeon[CurrentLocation].WasVisited == false)
            {
                // PIERWSZE WEJSCIE DO TEGO POKOJU W DUNGEONIE
                GridManager.instance.ResetGridToDefault();
                GridManager.instance.RandomizeDataOnGrid();
            }
            else
            {
                // KOLEJNE WEJSCIE DO TEGO SAMEGO POKOJU, RAZ JUZ ODWIEDZONEGO
                GridManager.instance.ResetGridToDefault();
                LoadGridForRoomData(DungeonRoomScript.Dungeon[CurrentLocation]);
            }

            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4, 0));
            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation);
            DungeonRoomScript.Dungeon[CurrentLocation].WasVisited = true;
        }
    }
    [ContextMenu("move right")]
    public void MoveNExtRoom_Right()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);

        var vector = Vector2Int.right;
         Debug.LogWarning("Move right from:"+CurrentLocation.ToString()+" to "+(CurrentLocation+vector));
        if (DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation + vector))
        {

            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode + "_OPEN").First();
            if(DungeonRoomScript.Dungeon[CurrentLocation].WasVisited == false)
            {
                // PIERWSZE WEJSCIE DO TEGO POKOJU W DUNGEONIE
                GridManager.instance.ResetGridToDefault();
                GridManager.instance.RandomizeDataOnGrid();
            }
            else
            {
                // KOLEJNE WEJSCIE DO TEGO SAMEGO POKOJU, RAZ JUZ ODWIEDZONEGO
                GridManager.instance.ResetGridToDefault();
                LoadGridForRoomData(DungeonRoomScript.Dungeon[CurrentLocation]);
            }

            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(0, 4));
            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation);
            DungeonRoomScript.Dungeon[CurrentLocation].WasVisited = true;
        }
    }
    [ContextMenu("move Down")]
    public void MoveNExtRoom_Down()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);

        var vector = Vector2Int.down;
         Debug.LogWarning("Move down from:"+CurrentLocation.ToString()+" to "+(CurrentLocation+vector));
        if (DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation + vector))
        {

            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode + "_OPEN").First();
            if(DungeonRoomScript.Dungeon[CurrentLocation].WasVisited == false)
            {
                // PIERWSZE WEJSCIE DO TEGO POKOJU W DUNGEONIE
                GridManager.instance.ResetGridToDefault();
                GridManager.instance.RandomizeDataOnGrid();
            }
            else
            {
                // KOLEJNE WEJSCIE DO TEGO SAMEGO POKOJU, RAZ JUZ ODWIEDZONEGO
                 GridManager.instance.ResetGridToDefault();
                LoadGridForRoomData(DungeonRoomScript.Dungeon[CurrentLocation]);
            }

            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4, 8));
            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation);
            DungeonRoomScript.Dungeon[CurrentLocation].WasVisited = true;
        }
    }
    [ContextMenu("move Left")]
    public void MoveNExtRoomLeftP()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);

        var vector = Vector2Int.left;
        Debug.LogWarning("Move left from:"+CurrentLocation.ToString()+" to "+(CurrentLocation+vector));
        if (DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation + vector))
        {

            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode + "_OPEN").First();
            if(DungeonRoomScript.Dungeon[CurrentLocation].WasVisited == false)
            {
                // PIERWSZE WEJSCIE DO TEGO POKOJU W DUNGEONIE
                GridManager.instance.ResetGridToDefault();
                GridManager.instance.RandomizeDataOnGrid();
            }
            else
            {
                // KOLEJNE WEJSCIE DO TEGO SAMEGO POKOJU, RAZ JUZ ODWIEDZONEGO
                GridManager.instance.ResetGridToDefault();
                LoadGridForRoomData(DungeonRoomScript.Dungeon[CurrentLocation]);
            }

            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(8, 4));
            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation);
            DungeonRoomScript.Dungeon[CurrentLocation].WasVisited = true;
        }
    }

    // private void RemoveRoomGrid()
    // {
    //     GameManager.instance._chestLootScript.Clear();
    //     if (NotificationManger.instance != null)
    //     {
    //         NotificationManger.instance.NotificationList.ForEach(n => Destroy(n.gameObject.transform.parent.gameObject));
    //         NotificationManger.instance.NotificationList.Clear();
    //     }
    //     foreach (var cell in GridManager.CellGridTable)
    //     {
    //         Destroy(cell.Value.gameObject);
    //     }
    //     GridManager.destroyedTilesPool.Clear();
    //     GridManager.CellGridTable.Clear();
    // }
    public void RestartTurnRoutine()
    {
        GameManager.instance.StopAllCoroutines();
        StopAllCoroutines();

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.CurrentTurnNumber = 1;
        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        StartCoroutine(GameManager.instance.AddTurn());
    }
    private void ConfigureNextRoomButtons(Vector2Int newLocation)
    {
        var availableExits = DungeonRoomScript.Dungeon[newLocation].doorsNameCode.ToCharArray().ToList();

        W_BTN.gameObject.SetActive(false);
        S_BTN.gameObject.SetActive(false);
        A_BTN.gameObject.SetActive(false);
        D_BTN.gameObject.SetActive(false);

        foreach (var exitDoor in availableExits)
        {
            if (exitDoor == Char.Parse("W"))
                W_BTN.gameObject.SetActive(true);

            if (exitDoor == Char.Parse("S"))
                S_BTN.gameObject.SetActive(true);

            if (exitDoor == Char.Parse("A"))
                A_BTN.gameObject.SetActive(true);

            if (exitDoor == Char.Parse("D"))
                D_BTN.gameObject.SetActive(true);
        }
    }
}
