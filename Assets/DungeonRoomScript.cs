using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DungeonRoomScript : MonoBehaviour
{
    public static DungeonRoomScript instance;
    private void Awake() {
            instance = this;
        }
    [SerializeField] public List<Sprite> roomsTemplates = new List<Sprite>();

    public Vector2Int Location = Vector2Int.zero;
    public Dictionary<Vector2Int, SpriteRenderer> existingRooms = new Dictionary<Vector2Int, SpriteRenderer>();
    public static Dictionary<string, Vector2Int> directionsDict = new Dictionary<string, Vector2Int>{
        {"W",Vector2Int.up},
        {"D",Vector2Int.right},
        {"S",Vector2Int.down},
        {"A",Vector2Int.left}
    };
   
    public Vector2Int BackupPlayerPosition;
    
    
    [ContextMenu("SpawnFirstRoom")]
    public static void GenerateDungeonRooms()
    {
        // create init center room
        Room MainRoom = new Room(Vector2Int.zero,"WDSA");
        Dungeon.Clear();
        Dungeon.Add(Vector2Int.zero,MainRoom);
        foreach(var dirChar in "WDSA".ToCharArray().ToList())
        {
            CreateRoom(from: MainRoom.position, directionsDict[dirChar.ToString()]);
        }
    }
    public static Dictionary<Vector2Int, Room> Dungeon = new Dictionary<Vector2Int, Room>();
    private static void CreateRoom(Vector2Int from, Vector2Int direction)
    {
//        print("create new room from " + from + " in direction:" + direction);
        var newLocation = from + direction;
        if (Dungeon.ContainsKey(newLocation))
            return;      
        else
        {
            string doorsNameCode = AdjustDorrsBasedOnNeighbours(newLocation);
            var room = new Room(newLocation, doorsNameCode);
            Dungeon.Add(newLocation, room);

            foreach(var dirChar in doorsNameCode.ToCharArray().ToList())
            {
                CreateRoom(from: room.position, directionsDict[dirChar.ToString()]);
            }
        }
    }
    private  static string AdjustDorrsBasedOnNeighbours(Vector2Int location)
    {
        string W = "";
        string D = "";
        string S = "";
        string A = "";

        if (location.x >= -4 && location.x <= 4)
        {
            if (location.y >= -4 && location.y <= 4)
            {
                W = UnityEngine.Random.Range(0, 100) < 40 ? "W" : "";
                D = UnityEngine.Random.Range(0, 100) < 40 ? "D" : "";
                S = UnityEngine.Random.Range(0, 100) < 40 ? "S" : "";
                A = UnityEngine.Random.Range(0, 100) < 40 ? "A" : "";
            }
        }

        if (Dungeon.ContainsKey(location + Vector2Int.up))
        {
            if (Dungeon[location + Vector2Int.up].doorsNameCode.ToCharArray().ToList().Contains((Char.Parse("S"))))
                W = "W";
            else
                W = "";
        }

        if (Dungeon.ContainsKey(location + Vector2Int.right))
        {
            if (Dungeon[location + Vector2Int.right].doorsNameCode.ToCharArray().ToList().Contains((Char.Parse("A"))))
                D = "D";
            else
                D = "";
        }

        if (Dungeon.ContainsKey(location + Vector2Int.down))
        {
            if (Dungeon[location + Vector2Int.down].doorsNameCode.ToCharArray().ToList().Contains((Char.Parse("W"))))
                S = "S";
            else
                S = "";
        }

        if (Dungeon.ContainsKey(location + Vector2Int.left))
        {
            if (Dungeon[location + Vector2Int.left].doorsNameCode.ToCharArray().ToList().Contains((Char.Parse("D"))))
                A = "A";
            else
                A = "";
        }

        return $"{W}{D}{S}{A}";
    }
    public class Room
    {
        public bool WasVisited = false;
        public Vector2Int position;
        public String doorsNameCode;
        public RoomGridData DATA;
        public Room(Vector2Int position, string doorsNameCode)
        {
            this.position = position;
            this.doorsNameCode = doorsNameCode;
            
           // Debug.Log("Room Created: "+position+" ["+doorsNameCode+"]");
        }
    }

    public class RoomGridData
    {
        public Dictionary<Vector2Int,MonsterBackupData> Backup_Monsters = new Dictionary<Vector2Int, MonsterBackupData>();
        public Dictionary<Vector2Int,TreasureBackupData> Backup_Treasures = new Dictionary<Vector2Int,TreasureBackupData >();
        public Dictionary<Vector2Int,BombData> Backup_Bombs = new Dictionary<Vector2Int, BombData>();
        public HashSet<Vector2Int> WallPositions = new HashSet<Vector2Int>();
        public RoomGridData(Dictionary<Vector2Int, MonsterBackupData> backup_Monsters, 
                    Dictionary<Vector2Int, TreasureBackupData> backup_Treasures, 
                    Dictionary<Vector2Int, BombData> backup_Bombs, 
                    HashSet<Vector2Int> wallPositions)
        {
            Backup_Monsters = backup_Monsters;
            Backup_Treasures = backup_Treasures;
            Backup_Bombs = backup_Bombs;
            WallPositions = wallPositions;
        }
    }

    [ContextMenu("Generate full dungeon data")]
    public void GenerateFullData()
    {
        GenerateDungeonRooms();

        string FULLDATA = "";
        string wallsDATA = "";
        foreach(var room in Dungeon.Values)
        {
            string roomData = "";
            int rows = 7;
            wallsDATA += room.position.ToString()+$"[{room.doorsNameCode}]\n";
            roomData+="\n";
            for (int i = 0; i < 49; i++)
            {
                roomData+=" "+GetRandomTypeString();
                rows--;
                if(rows == 0)
                {
                    roomData += "\n";
                    rows = 7;
                }
            }   
          //  Debug.LogError(roomData);
            FULLDATA+=roomData+"\n\n";
        }

        Debug.LogError(FULLDATA);
         Debug.LogError(wallsDATA);
    }
    
    public static string GetRandomTypeString()
    {
        int rng = UnityEngine.Random.Range(0,101);
        
        if(rng >= 0 && rng <75)
            return TileTypes.grass.ToString(); // 55%

        if(rng >= 75 && rng <85)
            return TileTypes.wall.ToString(); // 10%

        if(rng >= 85 && rng <90)
            return TileTypes.bomb.ToString(); // 10%

        if(rng >= 90 && rng <95)
            return TileTypes.treasure.ToString(); // 5%

        if(rng >= 95)
            return TileTypes.monster.ToString(); // 5%

        return TileTypes.undefined.ToString();
    }
    
    
    
    #region  wizualizacja
    // [ContextMenu("SpawnFirstRoom-Visual")]
    // public void SpawnRoomVisual()
    // {
    // foreach (var r in existingRooms)
    // {
    //     Destroy(r.Value.gameObject);
    // }
    // existingRooms.Clear();

    // var room = new GameObject(Location.ToString());
    // room.transform.parent = this.transform;
    // room.transform.localScale = Vector3.one * 150;
    // room.transform.localPosition = new Vector3(0, 0);
    // room.name = Location.ToString();
    // var spriteRenderer = room.AddComponent<SpriteRenderer>();
    // spriteRenderer.sortingOrder = 150;
    // spriteRenderer.sprite = roomsTemplates.Where(n=> n.name == "WDSA_OPEN").First();
    // existingRooms.Add(Vector2Int.zero, spriteRenderer);

    // var avaiablenewroomDirections = room.GetComponent<SpriteRenderer>().sprite.name.Replace("_OPEN", "").ToCharArray();
    // foreach (var dir in avaiablenewroomDirections)
    // {
    //   //  print($"dir check:[ {dir} ]");
    //     CreateNewRoom_OLD(from: Location, directionsDict[dir.ToString()]);
    // }
    // }
    // public void CreateNewRoom_OLD(Vector2Int from, Vector2Int direction)
    // {

    // //    print("create new room from " + from + " in direction:" + direction);
    //     var newLocation = from + direction;
    //     if (existingRooms.ContainsKey(newLocation))
    //     {
    //         return;
    //     }
    //     else
    //     {
    //         var room = new GameObject(newLocation.ToString());
    //         room.transform.parent = this.transform;
    //         room.name = newLocation.ToString();
    //         room.transform.localScale = Vector3.one * 150;
    //         room.transform.localPosition = new Vector3(225 * newLocation.x, +225 * newLocation.y);
    //         var spriteRenderer = room.AddComponent<SpriteRenderer>();
    //         spriteRenderer.sortingOrder = 150;
    //         spriteRenderer.sprite = ModifeNeighboursDungeonSpritesBasedOnAvaiableEnters_OLD(newLocation);
    //         existingRooms.Add(newLocation, spriteRenderer);

    //         var avaiablenewroomDirections = room.GetComponent<SpriteRenderer>().sprite.name.Replace("_OPEN", "").ToCharArray();
    //         foreach (var dir in avaiablenewroomDirections)
    //         {
    //             CreateNewRoom_OLD(from: newLocation, directionsDict[dir.ToString()]);
    //         }
    //     }
    // }
    // private Sprite ModifeNeighboursDungeonSpritesBasedOnAvaiableEnters_OLD(Vector2Int location)
    // {
    //     string W = "";
    //     string D = "";
    //     string S = "";
    //     string A = "";

    //     if (location.x >= -4 && location.x <= 4)
    //     {
    //         if (location.y >= -4 && location.y <= 4)
    //         {
    //             W = UnityEngine.Random.Range(0, 100) < 40 ? "W" : "";
    //             D = UnityEngine.Random.Range(0, 100) < 40 ? "D" : "";
    //             S = UnityEngine.Random.Range(0, 100) < 40 ? "S" : "";
    //             A = UnityEngine.Random.Range(0, 100) < 40 ? "A" : "";
    //         }
    //     }

    //     if (existingRooms.ContainsKey(location + Vector2Int.up))
    //     {
    //       //  print("jest sąsiad u góry" + existingRooms[location + Vector2Int.up].sprite.name.Replace("_OPEN", ""));

    //         if (existingRooms[location + Vector2Int.up].sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("S"))))
    //             W = "W";
    //         else
    //             W = "";
    //     }

    //     if (existingRooms.ContainsKey(location + Vector2Int.right))
    //     {
    //      //   print("jest sąsiad po prawej" + existingRooms[location + Vector2Int.right].sprite.name.Replace("_OPEN", ""));

    //         if (existingRooms[location + Vector2Int.right].sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("A"))))
    //             D = "D";
    //         else
    //             D = "";
    //     }

    //     if (existingRooms.ContainsKey(location + Vector2Int.down))
    //     {
    //     //    print("jest sąsiad na dole" + existingRooms[location + Vector2Int.down].sprite.name.Replace("_OPEN", ""));

    //         if (existingRooms[location + Vector2Int.down].sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("W"))))
    //             S = "S";
    //         else
    //             S = "";
    //     }

    //     if (existingRooms.ContainsKey(location + Vector2Int.left))
    //     {
    //       //  print("jest sąsiad po lewej" + existingRooms[location + Vector2Int.left].sprite.name.Replace("_OPEN", ""));

    //         if (existingRooms[location + Vector2Int.left].sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("D"))))
    //             A = "A";
    //         else
    //             A = "";
    //     }

    //     var spriteName = $"{W}{D}{S}{A}_OPEN";
    //    // print(" MUST HAVE => " + spriteName);
    //     return roomsTemplates.Where(s => s.name == spriteName).First();
    // }
    
    #endregion
}
