using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
        GameManager.instance.attackSelectorPopup.ClearCenteredNode();
        GameManager.instance.attackSelectorPopup.gameObject.SetActive(false);

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

    internal void TeleportTo(Room from, Room to)
    {
        MakeCurrentMapBackup(from);
        Vector2Int newLocationCoord = GameManager.ConvertToVector2Int(to.position);
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(4, 3),newLocationCoord);
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
        Debug.Log("Aktualizowanie danych pokoju: "+_room.position);
        Dictionary<string, MonsterBackupData> _backup_Monsters = new Dictionary<string, MonsterBackupData>();
        Dictionary<string, TreasureBackupData> _backup_Treasures = new Dictionary<string, TreasureBackupData>();
        Dictionary<string, PortalBackupData> _backup_Portals = new Dictionary<string, PortalBackupData>();
        Dictionary<string, BombBackupData> _backup_Bombs = new Dictionary<string, BombBackupData>();
        HashSet<string> _wallPositions = new HashSet<string>();
        
        foreach (var cell in GridManager.CellGridTable)
        {
            if (cell.Value.SpecialTile is Monster_Cell)
            {
                var monster = (cell.Value.SpecialTile as Monster_Cell);
                _backup_Monsters.Add(cell.Key.ToString(), (monster.SaveAndGetCellProgressData()) as MonsterBackupData);
                continue;
            }

            if (cell.Value.SpecialTile is Treasure_Cell)
            {
                var treasure = (cell.Value.SpecialTile as Treasure_Cell);

                _backup_Treasures.Add(cell.Key.ToString(), (treasure.SaveAndGetCellProgressData()) as TreasureBackupData);
                continue;
            }

            if (cell.Value.SpecialTile is Portal_Cell)
            {
                var portal = (cell.Value.SpecialTile as Portal_Cell);

                _backup_Portals.Add(_room.position.ToString(), (portal.SaveAndGetCellProgressData()) as PortalBackupData);
                continue;
            }

            if (cell.Value.SpecialTile is Bomb_Cell)
            {
                 var bomb = (cell.Value.SpecialTile as Bomb_Cell);

                _backup_Bombs.Add(cell.Key.ToString(),(bomb.SaveAndGetCellProgressData()) as BombBackupData);
                continue;
            }

            if (cell.Value.Type == TileTypes.wall)
            {
                _wallPositions.Add(cell.Key.ToString());
                continue;
            }
        }
        RoomGridData _roomBackupData = new RoomGridData(_backup_Monsters, _backup_Treasures, _backup_Bombs, _backup_Portals, _wallPositions);
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
                if (_room.DATA.Backup_Monsters.ContainsKey(cell.Key.ToString()))
                {
                    var monsterbackup =_room.DATA.Backup_Monsters[cell.Key.ToString()];
                    GridManager.CellGridTable[cell.Key].SpecialTile = new Monster_Cell(parent: cell.Value, GameManager.instance.GetMonsterData(monsterbackup.MonsterDataID),monsterbackup);
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.monster;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.Backup_Treasures.ContainsKey(cell.Key.ToString()))
                {
                    var treasurebackup =_room.DATA.Backup_Treasures[cell.Key.ToString()];
                    treasurebackup.LoadDataFromBackup();

                    GridManager.CellGridTable[cell.Key].SpecialTile = new Treasure_Cell(parent: cell.Value, GameManager.instance.GetTreasureData(treasurebackup.TreasureDataID), treasurebackup);
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.treasure;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.Backup_Bombs.ContainsKey(cell.Key.ToString()))
                {
                    var bombbackup =_room.DATA.Backup_Bombs[cell.Key.ToString()];

                    GridManager.CellGridTable[cell.Key].SpecialTile = new Bomb_Cell(parent: cell.Value, GameManager.instance.GetBombData(bombbackup.BombId));
                    GridManager.CellGridTable[cell.Key].Type = TileTypes.bomb;
                    cell.Value.SetCell(cell.Key, false);
                }

                if (_room.DATA.WallPositions.Contains(cell.Key.ToString()))
                {
                    cell.Value.AssignType(TileTypes.wall);
                    cell.Value.SetCell(cell.Key, false);
                }
            }
        }
        
        if (_room.DATA.Backup_Portals.ContainsKey(_room.position.ToString()))
        {
            Debug.Log("Spawn portalu");
            var portalbackup =_room.DATA.Backup_Portals[_room.position.ToString()];
            PlaceAndConfigurePortal(GameManager.ConvertToVector2Int(_room.position), portalbackup);     
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
            DungeonRoomScript.Dungeon[GameManager.ConvertToVector2Int(changesFromRoom.position)+changeddoordir].SetStateDoorByVector( (-changeddoordir), true);
        }
    }
    public void GenerateAndEnterDungeon()
    {
        DungeonRoomScript.GenerateDungeonRooms();

        RandomizePortalsConnections(connectionsNumber: 3);
        
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

    [ContextMenu("ZrzutDungeonaZDanymi")]
    public void GenerateFullDungeonBackupData()
    {
        JSONDUNGEONDATACLASS DungDataBackup = new JSONDUNGEONDATACLASS(
                PlayerManager.instance._playerCell.ParentCell.CurrentPosition.ToString(), 
                DungeonManager.instance.CurrentLocation.ToString(), 
                DungeonRoomScript.Dungeon);

        string JSONresult = JsonConvert.SerializeObject(DungDataBackup);
        Directory.CreateDirectory(Application.persistentDataPath + $"/DUNGEONS_DATA");
        File.WriteAllText(Application.persistentDataPath + $"/DUNGEONS_DATA/DUNGEON_1.json", JSONresult);

    }

    [ContextMenu("LoadDungeonFromFile")]
    public void LoadDungeonFromFile()
    {
        string JSONresult = File.ReadAllText(Application.persistentDataPath + $"/DUNGEONS_DATA/DUNGEON_1.json");
        JSONDUNGEONDATACLASS DungDataBackup = JsonConvert.DeserializeObject<JSONDUNGEONDATACLASS>(JSONresult);  
        Debug.Log("data count "+DungDataBackup.data.Count);
        DungeonRoomScript.Dungeon = DungDataBackup.ConvertDatatoVectorKeys(DungDataBackup.data);

        string[]temp=DungDataBackup.PlayerDungeonLocation.ToString().Substring(1,DungDataBackup.PlayerDungeonLocation.ToString().Length-2).Split(',');
        CurrentLocation = new Vector2Int(Int32.Parse(temp[0]),Int32.Parse(temp[1]));
        // -------------------------------------------------------------------------------------------
        
        GameManager.instance.attackSelectorPopup.ClearCenteredNode();
        GameManager.instance.attackSelectorPopup.gameObject.SetActive(false);

        currentDungeonDistance = DungeonManager.instance.maxDungeonTraveledDistance;

        DungeonSelectionWindow.SetActive(false);
        DungeonCanvas.SetActive(true);

        GridManager.instance.CreateEmptyGrid();
        //GridManager.instance.RandomizeDataOnGrid(); // TODO:

        temp=DungDataBackup.PlayerRoomPosition.ToString().Substring(1,DungDataBackup.PlayerRoomPosition.ToString().Length-2).Split(',');
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(Int32.Parse(temp[0]),Int32.Parse(temp[1])),CurrentLocation);

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        PlayerManager.instance.RegenerateFullStamina();
        
        StartCoroutine(GameManager.instance.AddTurn());
        
        // ----------------------------------------------------------------------------------------------
        

        RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates
            .Where(t => t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode + "_OPEN")
            .First();

        currentDungeonDistance = DungeonRoomScript.Dungeon[CurrentLocation].DistanceFromCenter;
        maxDungeonTraveledDistance = currentDungeonDistance > maxDungeonTraveledDistance ? currentDungeonDistance : maxDungeonTraveledDistance;

        foreach(var monsterCell in GridManager.CellGridTable.Where(c=>c.Value.SpecialTile is Monster_Cell))
        {
            (monsterCell.Value.SpecialTile as Monster_Cell).AdjustByMapDificultyLevel(Dungeon[Vector2Int.zero].DistanceFromCenter);
        }

        ShowMinimap();
    }

    Dictionary<Room,Room> portalConnectionDict = new Dictionary<Room, Room>();
    private void RandomizePortalsConnections(int connectionsNumber)
    {
        portalConnectionDict.Clear();
        
        while(portalConnectionDict.Count-1 <= connectionsNumber)
        {
            
            Room randomLocationStart = DungeonRoomScript.Dungeon.ElementAt(UnityEngine.Random.Range(0,DungeonRoomScript.Dungeon.Count-1)).Value;
            Room randomLocationEnd = DungeonRoomScript.Dungeon.ElementAt(UnityEngine.Random.Range(0,DungeonRoomScript.Dungeon.Count-1)).Value;

            if(portalConnectionDict.ContainsKey(randomLocationStart) || portalConnectionDict.ContainsKey(randomLocationEnd))
            {
                continue;
            }

            if(portalConnectionDict.Where(r=>r.Value.position == randomLocationStart.position).Any() || portalConnectionDict.Where(r=>r.Value.position == randomLocationEnd.position).Any())
            {
                continue;
            }

            Debug.Log($"room {randomLocationStart.position} <====> room {randomLocationEnd.position}");

            randomLocationStart.ContainPortal = true;
            randomLocationStart.DATA.Backup_Portals.Add(randomLocationStart.position.ToString(), new PortalBackupData(1,"open",randomLocationStart.position.ToString(),randomLocationEnd.position.ToString()));

            randomLocationEnd.ContainPortal = true;
            randomLocationEnd.DATA.Backup_Portals.Add(randomLocationEnd.position.ToString(), new PortalBackupData(1,"open",randomLocationEnd.position.ToString(),randomLocationStart.position.ToString()));

            portalConnectionDict.Add(randomLocationStart,randomLocationEnd);            
        }
    }

    public void MoveNExtRoom_Up()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.up;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(4,0),newLocationCoord);
    }
    public void MoveNExtRoom_Right()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.right;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(0,4),newLocationCoord);
    }
    public void MoveNExtRoom_Down()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.down;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(4,8),newLocationCoord);
    }
    public void MoveNExtRoomLeftP()
    {
        MakeCurrentMapBackup(DungeonRoomScript.Dungeon[CurrentLocation]);
        var vector = Vector2Int.left;
        Vector2Int newLocationCoord = CurrentLocation + vector;
        ManageRoomDorsAndPlayerSpawn(playerPosition:new Vector2Int(8, 4),newLocationCoord);
    }
    private void ManageRoomDorsAndPlayerSpawn(Vector2Int playerPosition, Vector2Int newLocationCoord)
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
                PlaceAndConfigurePortal(newLocationCoord);
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
    private void PlaceAndConfigurePortal(Vector2Int roomLocation, PortalBackupData portalDataFromBackup = null)
    {   
        Vector2Int center = new Vector2Int(4,4);
        var cell = GridManager.CellGridTable[center];
        if(portalDataFromBackup != null)
        {
            // załądowanie info o teleporcie z pliku
            var startingRoom = Dungeon[GameManager.ConvertToVector2Int(portalDataFromBackup.StartLocation)];
            var endingRoom = Dungeon[GameManager.ConvertToVector2Int(portalDataFromBackup.EndLocation)];

            if(portalConnectionDict.ContainsKey(startingRoom) == false && portalConnectionDict.ContainsKey(endingRoom) == false)
            {
                portalConnectionDict.Add(startingRoom,endingRoom);
            }
        }
        var portal = portalConnectionDict.Where(k=>k.Key.position == roomLocation.ToString() || k.Value.position == roomLocation.ToString()).FirstOrDefault();

        Debug.Log(portal.Key.ToString()+"portal do położenia, spawn na środku pokoju: "+roomLocation.ToString());
        if(portal.Key != null)
        {
            GridManager.CellGridTable[center].SpecialTile = new Portal_Cell(parentCell: cell, GameManager.instance.GetPortalData(0),portal.Key,portal.Value);
            GridManager.CellGridTable[center].Type = TileTypes.portal;
            cell.SetCell(center, false);
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
                AddMinimapTile(room.Key, doorsExitsCode:room.Value.doorsNameCode);

                foreach(var directionExit in room.Value.doorsNameCode.ToCharArray())
                {
                    if(directionExit == Char.Parse("W"))
                    {
                        var vectorToSpottedRoom = Vector2Int.up;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                        AddMinimapTile(positiotn, markAsRevealed:true);
                    }

                    if (directionExit == Char.Parse("D"))
                    {
                        var vectorToSpottedRoom = Vector2Int.right;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                       AddMinimapTile(positiotn, markAsRevealed:true);
                    }

                    if(directionExit == Char.Parse("S"))
                    {
                        var vectorToSpottedRoom = Vector2Int.down;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                        AddMinimapTile(positiotn, markAsRevealed:true);
                    }

                    if(directionExit == Char.Parse("A"))
                    {
                        var vectorToSpottedRoom = Vector2Int.left;
                        Vector2Int positiotn = room.Key + vectorToSpottedRoom;
                        AddMinimapTile(positiotn, markAsRevealed:true);
                    }
                }
            }
            if(room.Value.ContainPortal)
            {
                AddMinimapTile(room.Key, markAsRevealed:room.Value.WasVisited);
                minimapTiles[room.Key].GetComponent<Image>().enabled = room.Value.WasVisited;
                minimapTiles[room.Key].transform.GetChild(0).GetComponent<Image>().enabled = false;
                minimapTiles[room.Key].transform.GetChild(1).GetComponent<Image>().enabled = true;
            }
        }

        foreach(var tile in minimapTiles)
        {
            tile.Value.transform.localPosition = new Vector3((tile.Key.x-CurrentLocation.x) * size, (tile.Key.y-CurrentLocation.y) * size, 0);
        }
        MinimapSpawnContainer.parent.transform.GetComponent<RectTransform>().localPosition = new Vector3(244.74f,-247.55f,0);

        void AddMinimapTile(Vector2Int roomLocation, string doorsExitsCode = "", bool markAsRevealed = false)
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
                    minimapTiles[roomLocation].sprite = minimapimageList.Where(img=>img.name == doorsExitsCode+"_OPEN").FirstOrDefault();
                    minimapTiles[roomLocation].color = roomLocation==CurrentLocation?Color.green:defaultColor;
                    minimapTiles[roomLocation].transform.GetChild(0).GetComponent<Image>().enabled = false;
                    return;
                }

                room_minimap = Instantiate(MinimapTilePrefab, MinimapSpawnContainer).GetComponent<Image>();
                room_minimap.name = roomLocation.ToString();
                
                room_minimap.enabled = true;
                room_minimap.sprite = minimapimageList.Where(img=>img.name == doorsExitsCode+"_OPEN").FirstOrDefault();
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