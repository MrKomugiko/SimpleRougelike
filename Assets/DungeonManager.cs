using System;
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
    public int maxDungeonTraveledDistance = 0;
    public int currentDungeonDistance = 0;

    [SerializeField] private GameObject DungeonCanvas;
    public void OpenDungeon()
    {
        currentDungeonDistance = DungeonManager.instance.maxDungeonTraveledDistance;

        DungeonSelectionWindow.SetActive(false);
        DungeonCanvas.SetActive(true);
        GridManager.instance.CreateEmptyGrid();
        GridManager.instance.RandomizeDataOnGrid(); // TODO:
        GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4, 4));

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        PlayerManager.instance.RegenerateFullStamina();
        
        StartCoroutine(GameManager.instance.AddTurn());
    }
    
    public void DungeonClearAndGoToCamp()
    {
        PlayerManager.instance._mainBackpack.ItemSlots.ForEach(slot =>
            {
                if (slot.IsInQuickSlot)
                {
                    slot.RemoveFromQuickSlot((int)slot.AssignedToQuickSlot);
                }
            }
        );
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

        maxDungeonTraveledDistance = currentDungeonDistance > maxDungeonTraveledDistance ? currentDungeonDistance : maxDungeonTraveledDistance;
    }
    [SerializeField] Image RoomWallsSprite;
    public Vector2Int CurrentLocation;
    public void MakeCurrentMapBackup(Room _room)
    {
        Dictionary<Vector2Int, MonsterBackupData> _backup_Monsters = new Dictionary<Vector2Int, MonsterBackupData>();
        Dictionary<Vector2Int, TreasureBackupData> _backup_Treasures = new Dictionary<Vector2Int, TreasureBackupData>();
        Dictionary<Vector2Int, BombData> _backup_Bombs = new Dictionary<Vector2Int, BombData>();
        HashSet<Vector2Int> _wallPositions = new HashSet<Vector2Int>();
        
        foreach (var cell in GridManager.CellGridTable)
        {
            if (cell.Value.SpecialTile is Monster_Cell)
            {
                var monster = (cell.Value.SpecialTile as Monster_Cell);
                _backup_Monsters.Add(cell.Key, (monster.SaveAndGetCellProgressData()) as MonsterBackupData);
                continue;
            }

            if (cell.Value.SpecialTile is Treasure_Cell)
            {
                var treasure = (cell.Value.SpecialTile as Treasure_Cell);

                _backup_Treasures.Add(cell.Key, (treasure.SaveAndGetCellProgressData()) as TreasureBackupData);
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
                    var monsterbackup =_room.DATA.Backup_Monsters[cell.Key];
                    GridManager.CellGridTable[cell.Key].SpecialTile = new Monster_Cell(parent: cell.Value, GameManager.instance.GetMonsterData(monsterbackup.MonsterDataID),monsterbackup);
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.monster;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.Backup_Treasures.ContainsKey(cell.Key))
                {
                    var treasurebackup =_room.DATA.Backup_Treasures[cell.Key];
                    GridManager.CellGridTable[cell.Key].SpecialTile = new Treasure_Cell(parent: cell.Value, GameManager.instance.GetTreasureData(treasurebackup.TreasureDataID),treasurebackup);
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

        GridManager.CellGridTable[new Vector2Int(0, 4)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(0, 4)].isWalkable = true;

        GridManager.CellGridTable[new Vector2Int(8, 4)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(8, 4)].isWalkable = true;

        GridManager.CellGridTable[new Vector2Int(4, 0)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(4, 0)].isWalkable = true;

        GridManager.CellGridTable[new Vector2Int(4, 8)].AssignType(TileTypes.grass);
        GridManager.CellGridTable[new Vector2Int(4, 8)].isWalkable = true;
    }

    public static void SetNeighourRoomsDoorsState(Room changesFromRoom, List<Vector2Int> listchangedDoors)
    {
        foreach(var changeddoordir in listchangedDoors)
        {
            DungeonRoomScript.Dungeon[changesFromRoom.position+changeddoordir].SetStateDoorByVector( (-changeddoordir), true);
        }
    }
    public void GenerateAndEnterDungeon()
    {
        DungeonRoomScript.GenerateDungeonRooms();
        OpenDungeon();
        CurrentLocation = Vector2Int.zero;

        RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[Vector2Int.zero].doorsNameCode + "_OPEN").First();
        DungeonRoomScript.Dungeon[Vector2Int.zero].WasVisited = true;

        currentDungeonDistance = DungeonRoomScript.Dungeon[Vector2Int.zero].DistanceFromCenter;
        maxDungeonTraveledDistance = currentDungeonDistance > maxDungeonTraveledDistance ? currentDungeonDistance : maxDungeonTraveledDistance;

        foreach(var monsterCell in GridManager.CellGridTable.Where(c=>c.Value.SpecialTile is Monster_Cell))
        {
            (monsterCell.Value.SpecialTile as Monster_Cell).AdjustByMapDificultyLevel(Dungeon[Vector2Int.zero].DistanceFromCenter);
        }
    }
    public void MoveNExtRoom_Up()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.up;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(4,0),newLocationCoord, moveFromDirection: vector);
    }
    public void MoveNExtRoom_Right()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.right;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(0,4),newLocationCoord, moveFromDirection: vector);
    }
    public void MoveNExtRoom_Down()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.down;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(4,8),newLocationCoord, moveFromDirection: vector);
    }
    public void MoveNExtRoomLeftP()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.left;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(8, 4),newLocationCoord, moveFromDirection: vector);
    }
    private void ManageRoomDorsAndPlayerSpawn(Vector2Int playerPosition, Vector2Int newLocationCoord, Vector2Int moveFromDirection)
    {
        if (DungeonRoomScript.Dungeon.ContainsKey(newLocationCoord))
        {             
            currentDungeonDistance = DungeonRoomScript.Dungeon[newLocationCoord].DistanceFromCenter;
            maxDungeonTraveledDistance = currentDungeonDistance > maxDungeonTraveledDistance ? currentDungeonDistance : maxDungeonTraveledDistance;

            CurrentLocation = newLocationCoord;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t => t.name == DungeonRoomScript.Dungeon[newLocationCoord].doorsNameCode + "_OPEN").First();
            if(DungeonRoomScript.Dungeon[newLocationCoord].WasVisited == false)
            {
                GridManager.instance.ResetGridToDefault();
                GridManager.instance.RandomizeDataOnGrid();
            }
            else
            {
                GridManager.instance.ResetGridToDefault();
                LoadGridForRoomData(DungeonRoomScript.Dungeon[newLocationCoord]);
            }

            foreach(var monsterCell in GridManager.CellGridTable.Where(c=>c.Value.SpecialTile is Monster_Cell))
            {
                (monsterCell.Value.SpecialTile as Monster_Cell).AdjustByMapDificultyLevel(Dungeon[newLocationCoord].DistanceFromCenter);
            }

            GameManager.instance.teleporsEmergencySwitchUsed = false;
            GameManager.instance.Init_PlacePlayerOnGrid(playerPosition);
            PlayerManager.instance.MovmentValidator.HighlightValidMoveGrid(_restrictedByStaminavalue:true);
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: newLocationCoord);

            DungeonRoomScript.Dungeon[newLocationCoord].WasVisited = true;
            ShowMinimap();
        }


    }
    [SerializeField] public List<GameObject> telerpotButtons = new List<GameObject>();
    public void EnableTeleportButton(string direction)
    {
        var teleport = telerpotButtons.Where(n=>n.name == direction).First();
        teleport.transform.Find("Teleport_ON").GetComponent<SpriteRenderer>().enabled = true;
        teleport.GetComponent<Button>().interactable = true;
        teleport.GetComponent<Image>().raycastTarget = true;       
    }

    private void DissableTeleportButtons()
    {
        foreach(var teleport in telerpotButtons)
        {
            teleport.transform.Find("Teleport_ON").GetComponent<SpriteRenderer>().enabled = false;
            teleport.GetComponent<Button>().interactable = false;
            teleport.GetComponent<Image>().raycastTarget = false;
        }
    }
    public void RestartTurnRoutine()
    {
        GameManager.instance.StopAllCoroutines();
        StopAllCoroutines();

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        StartCoroutine(GameManager.instance.AddTurn());
        
    }
    private void ConfigureNextRoomButtons(Vector2Int newLocation)
    {
        DissableTeleportButtons(); // reset ustawienie wszystkich na OFF
        DungeonRoomScript.Dungeon[newLocation].DoorStatesList.Where(d=>d.Value == true).ToList().ForEach(d=>EnableTeleportButton(d.Key));

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


    public List<Sprite> minimapimageList = new List<Sprite>();
    public Transform MinimapSpawnContainer;
    public GameObject MinimapTilePrefab;
    private Dictionary<Vector2Int,Image> minimapTiles = new Dictionary<Vector2Int,Image>();

    public void ShowMinimap()
    {
        minimapTiles.Values.ToList().ForEach(c=>Destroy(c.gameObject));
        minimapTiles.Clear();

        int size = 55;
        Color32 defaultColor = new Color32(164,130,176,255);
        foreach(var room in Dungeon)
        {
            if(room.Value.WasVisited)
            {
                AddMinimapTile(size, room.Key, doorsExitsCode:room.Value.doorsNameCode);

                foreach(var directionExit in room.Value.doorsNameCode.ToCharArray())
                {
                    if(directionExit == Char.Parse("W"))
                    {
                        var vectorToSpottedRoom = Vector2Int.up;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                        AddMinimapTile(size, positiotn, markAsRevealed:true);
                    }

                    if (directionExit == Char.Parse("D"))
                    {
                        var vectorToSpottedRoom = Vector2Int.right;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                       AddMinimapTile(size, positiotn, markAsRevealed:true);
                    }

                    if(directionExit == Char.Parse("S"))
                    {
                        var vectorToSpottedRoom = Vector2Int.down;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                        AddMinimapTile(size, positiotn, markAsRevealed:true);
                    }

                    if(directionExit == Char.Parse("A"))
                    {
                        var vectorToSpottedRoom = Vector2Int.left;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                        AddMinimapTile(size, positiotn, markAsRevealed:true);
                    }
                }
            }
        }


        foreach(var tile in minimapTiles)
        {
            tile.Value.transform.localPosition = new Vector3((tile.Key.x-CurrentLocation.x) * size, (tile.Key.y-CurrentLocation.y) * size, 0);
        }
        MinimapSpawnContainer.parent.transform.GetComponent<RectTransform>().localPosition = new Vector3(244.74f,-247.55f,0);

        void AddMinimapTile(int size, Vector2Int roomLocation, string doorsExitsCode = "", bool markAsRevealed = false)
        {
            Image room_minimap = null;
            if(markAsRevealed)
            {
                if(minimapTiles.ContainsKey(roomLocation))
                {
                    return;
                }
                room_minimap= Instantiate(MinimapTilePrefab, MinimapSpawnContainer).GetComponent<Image>();
                room_minimap.name = roomLocation.ToString();

                room_minimap.enabled = false;
                room_minimap.transform.GetChild(0).GetComponent<Image>().enabled = true;
            }
            else
            {                
                if(minimapTiles.ContainsKey(roomLocation))
                {
                    minimapTiles[roomLocation].enabled = true;
                    minimapTiles[roomLocation].sprite = minimapimageList.Where(img=>img.name == doorsExitsCode+"_OPEN").First();
                    minimapTiles[roomLocation].color = roomLocation==CurrentLocation?Color.green:defaultColor;
                    minimapTiles[roomLocation].transform.GetChild(0).GetComponent<Image>().enabled = false;
                    return;
                }
                
                room_minimap = Instantiate(MinimapTilePrefab, MinimapSpawnContainer).GetComponent<Image>();;
                room_minimap.name = roomLocation.ToString();
                
                room_minimap.enabled = true;
                room_minimap.sprite = minimapimageList.Where(img=>img.name == doorsExitsCode+"_OPEN").First();
                room_minimap.color = roomLocation==CurrentLocation?Color.green:defaultColor;
                room_minimap.transform.GetChild(0).GetComponent<Image>().enabled = false;
            }
             minimapTiles.Add(roomLocation,room_minimap);
        }
    }

    public void ColorRevealedMinimap()
    {
        int longestdistance = Dungeon.ToList().OrderByDescending(v=>v.Value.DistanceFromCenter).First().Value.DistanceFromCenter;
        foreach(var tile in minimapTiles)
        {
            if(Dungeon[tile.Key].WasVisited)
            {
                float progressfillcolor = ((float)Dungeon[tile.Key].DistanceFromCenter / (float)longestdistance);
                tile.Value.color = Color32.Lerp(Color.white,Color.red,progressfillcolor);
            }
        }
    }
}