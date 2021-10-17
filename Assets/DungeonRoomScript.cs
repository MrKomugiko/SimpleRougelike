using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public partial class DungeonRoomScript : MonoBehaviour
{
    [SerializeField] GameObject ImageColorIndicator;
    public static DungeonRoomScript instance;
    private void Awake() {
            instance = this;
        }
    [SerializeField] public List<Sprite> roomsTemplates = new List<Sprite>();

    public Vector2Int Location = Vector2Int.zero;
    public Dictionary<Vector2Int, (SpriteRenderer, int)> existingRooms = new Dictionary<Vector2Int, (SpriteRenderer, int)>();
    public static Dictionary<string, Vector2Int> directionsDict = new Dictionary<string, Vector2Int>{
        {"W",Vector2Int.up},
        {"D",Vector2Int.right},
        {"S",Vector2Int.down},
        {"A",Vector2Int.left}
    };
    public static Dictionary<Vector2Int, Room> Dungeon = new Dictionary<Vector2Int, Room>();
    public static void GenerateDungeonRooms()
    {
        Room MainRoom = new Room(Vector2Int.zero.ToString(),"WDSA");
        Dungeon.Clear();
        Dungeon.Add(Vector2Int.zero,MainRoom);
        foreach(var dirChar in "WDSA".ToCharArray().ToList())
        {
            CreateRoom(from: GameManager.ConvertToVector2Int(MainRoom.position), directionsDict[dirChar.ToString()]);
        }
        Dungeon[Vector2Int.zero].DistanceFromCenter = DungeonManager.instance.maxDungeonTraveledDistance;
       
        var parentloc = Vector2Int.zero;

        var avaiablenewroomDirections = Dungeon[Vector2Int.zero].doorsNameCode.ToCharArray();
        foreach (var dir in avaiablenewroomDirections)
        {
            UpdateDistance(fromposition: parentloc, currentloc:parentloc+directionsDict[dir.ToString()]);
        }
    }
    private static void UpdateDistance(Vector2Int fromposition, Vector2Int currentloc)
    {
        if(Dungeon[currentloc].DistanceFromCenter != -1)
        {
            if(Dungeon[currentloc].DistanceFromCenter > Dungeon[fromposition].DistanceFromCenter)
            {
                Dungeon[currentloc].DistanceFromCenter =Dungeon[fromposition].DistanceFromCenter+1;
                var avaiablenewroomDirections = Dungeon[currentloc].doorsNameCode.ToCharArray();
                foreach (var dir in avaiablenewroomDirections)
                {
                    UpdateDistance(fromposition:currentloc, currentloc:currentloc+directionsDict[dir.ToString()]);
                }
            }
            else
            {
                return;
            }
        }
        if(Dungeon[currentloc].DistanceFromCenter == -1)
        {
            Dungeon[currentloc].DistanceFromCenter = (Dungeon[fromposition].DistanceFromCenter+1);
            var avaiablenewroomDirections = Dungeon[currentloc].doorsNameCode.ToCharArray();
            foreach (var dir in avaiablenewroomDirections)
            {
                UpdateDistance(fromposition:currentloc, currentloc:currentloc+directionsDict[dir.ToString()]);
            }
        }
    }
   
    private static void CreateRoom(Vector2Int from, Vector2Int direction)
    {
        var newLocation = from + direction;
        if (Dungeon.ContainsKey(newLocation))
            return;      
        else
        {
            string doorsNameCode = AdjustDorrsBasedOnNeighbours(newLocation);
            var room = new Room(newLocation.ToString(), doorsNameCode);
            Dungeon.Add(newLocation, room);

            foreach(var dirChar in doorsNameCode.ToCharArray().ToList())
            {
                CreateRoom(from: GameManager.ConvertToVector2Int(room.position), directionsDict[dirChar.ToString()]);
            }
        }
    }
    private  static string AdjustDorrsBasedOnNeighbours(Vector2Int location)
    {
        string W = "";
        string D = "";
        string S = "";
        string A = "";

        if (location.x >= -7 && location.x <= 7)
        {
            if (location.y >= -7 && location.y <= 7)
            {
                W = UnityEngine.Random.Range(0, 100) < 50 ? "W" : "";
                D = UnityEngine.Random.Range(0, 100) < 50 ? "D" : "";
                S = UnityEngine.Random.Range(0, 100) < 50 ? "S" : "";
                A = UnityEngine.Random.Range(0, 100) < 50 ? "A" : "";
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
    public class JSONDUNGEONDATACLASS
    {
        public int TotalRoomsCount;
        public int ExploredRoomCount;
        public string PlayerRoomPosition;
        public string PlayerDungeonLocation;
        public Dictionary<string, Room> data = new Dictionary<string, Room>();

        public JSONDUNGEONDATACLASS(string playerRoomPosition, string playerDungeonLocation, Dictionary<Vector2Int, Room> _dungeonData)
        {
            if(_dungeonData == null)
            {
                _dungeonData = new Dictionary<Vector2Int, Room>();
            }

            PlayerRoomPosition = playerRoomPosition;
            PlayerDungeonLocation = playerDungeonLocation;
            Debug.Log("_dungeondata"+_dungeonData.Count());
            data = ConvertDatatoStringKeys(_dungeonData);
            if(data.Count>0)
            {
                TotalRoomsCount = data.Count;
                ExploredRoomCount = data.Count(d=>d.Value.WasVisited);
            }
        }

        public Dictionary<Vector2Int, Room> ConvertDatatoVectorKeys(Dictionary<string, Room> _data)
        {
            Dictionary<Vector2Int, Room> _dungeon = new Dictionary<Vector2Int, Room>();
            foreach(var room in _data)
            {
                string[]temp=room.Key.ToString().Substring(1,room.Key.ToString().Length-2).Split(',');
                Vector2Int newkey = new Vector2Int(Int32.Parse(temp[0]),Int32.Parse(temp[1]));
                _dungeon.Add(newkey,room.Value);
            }
            return _dungeon;
        }

        public Dictionary<string, Room> ConvertDatatoStringKeys(Dictionary<Vector2Int, Room> _data)
        {
            Dictionary<string, Room> _dungeon = new Dictionary<string, Room>();
            foreach(var room in _data)
            {
                _dungeon.Add(room.Key.ToString(),room.Value);
            }
            return _dungeon;
        }
    }
    

    [Obsolete] public void GenerateFullData()
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
            FULLDATA+=roomData+"\n\n";
        }

        Debug.LogError(FULLDATA);
        Debug.LogError(wallsDATA);
    }
    [Obsolete] public static string GetRandomTypeString()
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
    [ContextMenu("SpawnFirstRoom-Visual")]
    public void SpawnRoomVisual()
    {
    foreach (var r in existingRooms)
    {
        Destroy(r.Value.Item1.gameObject);
    }
    existingRooms.Clear();

    var room = new GameObject(Location.ToString());
    room.transform.parent = this.transform;
    room.transform.localScale = Vector3.one * 150;
    room.transform.localPosition = new Vector3(0, 0);
    room.name = Location.ToString();
    var spriteRenderer = room.AddComponent<SpriteRenderer>();
    spriteRenderer.sortingOrder = 150;
    spriteRenderer.sprite = roomsTemplates.Where(n=> n.name == "WDSA_OPEN").First();
    existingRooms.Add(Vector2Int.zero, (spriteRenderer, 0));

    var avaiablenewroomDirections = room.GetComponent<SpriteRenderer>().sprite.name.Replace("_OPEN", "").ToCharArray();
    foreach (var dir in avaiablenewroomDirections)
    {
      //  print($"dir check:[ {dir} ]");
        CreateNewRoom_OLD(from: Location, directionsDict[dir.ToString()]);
    }
    }
    public void CreateNewRoom_OLD(Vector2Int from, Vector2Int direction)
    {
    //    print("create new room from " + from + " in direction:" + direction);
        var newLocation = from + direction;
        if (existingRooms.ContainsKey(newLocation))
        {
            return;
        }
        else
        {
            var room = new GameObject(newLocation.ToString());
            room.transform.parent = this.transform;
            room.name = newLocation.ToString();
            room.transform.localScale = Vector3.one * 150;
            room.transform.localPosition = new Vector3(225 * newLocation.x, +225 * newLocation.y);
            var spriteRenderer = room.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 150;
            spriteRenderer.sprite = ModifeNeighboursDungeonSpritesBasedOnAvaiableEnters_OLD(newLocation);
            int distance = existingRooms[from].Item2 +1;
            existingRooms.Add(newLocation, (spriteRenderer,distance));

            var avaiablenewroomDirections = room.GetComponent<SpriteRenderer>().sprite.name.Replace("_OPEN", "").ToCharArray();
            foreach (var dir in avaiablenewroomDirections)
            {
                CreateNewRoom_OLD(from: newLocation, directionsDict[dir.ToString()]);
            }
        }
    }
    private Sprite ModifeNeighboursDungeonSpritesBasedOnAvaiableEnters_OLD(Vector2Int location)
    {
        string W = "";
        string D = "";
        string S = "";
        string A = "";

        if (location.x >= -9 && location.x <= 9)
        {
            if (location.y >= -9 && location.y <= 9)
            {
                W = UnityEngine.Random.Range(0, 100) < 40 ? "W" : "";
                D = UnityEngine.Random.Range(0, 100) < 40 ? "D" : "";
                S = UnityEngine.Random.Range(0, 100) < 40 ? "S" : "";
                A = UnityEngine.Random.Range(0, 100) < 40 ? "A" : "";
            }
        }

        if (existingRooms.ContainsKey(location + Vector2Int.up))
        {
            if (existingRooms[location + Vector2Int.up].Item1.sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("S"))))
                W = "W";
            else
                W = "";
        }

        if (existingRooms.ContainsKey(location + Vector2Int.right))
        {
            if (existingRooms[location + Vector2Int.right].Item1.sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("A"))))
                D = "D";
            else
                D = "";
        }

        if (existingRooms.ContainsKey(location + Vector2Int.down))
        {
            if (existingRooms[location + Vector2Int.down].Item1.sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("W"))))
                S = "S";
            else
                S = "";
        }

        if (existingRooms.ContainsKey(location + Vector2Int.left))
        {
            if (existingRooms[location + Vector2Int.left].Item1.sprite.name.Replace("_OPEN", "").ToCharArray().ToList().Contains((Char.Parse("D"))))
                A = "A";
            else
                A = "";
        }

        var spriteName = $"{W}{D}{S}{A}_OPEN";
        return roomsTemplates.Where(s => s.name == spriteName).First();
    }
    Dictionary<Vector2Int,int> globalDistanceDict = new Dictionary<Vector2Int, int>();
    Dictionary<Vector2Int,int> distanceDir = new Dictionary<Vector2Int, int>();        
    [ContextMenu("visualise color images - distance")]
    public void VisualiseColorDistance_OLD()
    {
        distanceDir = new Dictionary<Vector2Int, int>();  
        distanceDir.Add(Vector2Int.zero,0);
        var parentloc = Vector2Int.zero;

        var avaiablenewroomDirections = existingRooms[Vector2Int.zero].Item1.sprite.name.Replace("_OPEN", "").ToCharArray();
        foreach (var dir in avaiablenewroomDirections)
        {
            UpdateDistance_OLD(fromposition: parentloc, currentloc:parentloc+directionsDict[dir.ToString()]);
        }

        foreach (var room in distanceDir)
        {
            int longestdistance = distanceDir.OrderByDescending(v=>v.Value).First().Value;
            var x = Instantiate(ImageColorIndicator,existingRooms[room.Key].Item1.gameObject.transform);
            float progressfillcolor = ((float)room.Value / (float)longestdistance);
            x.GetComponent<Image>().color = Color32.Lerp(Color.white,Color.red,progressfillcolor);
        }
    }
    private void UpdateDistance_OLD(Vector2Int fromposition, Vector2Int currentloc)
    {
        if(distanceDir.ContainsKey(currentloc))
        {
            if(distanceDir[currentloc] > distanceDir[fromposition])
            {
                distanceDir[currentloc] = distanceDir[fromposition]+1;
                 var avaiablenewroomDirections = existingRooms[currentloc].Item1.sprite.name.Replace("_OPEN", "").ToCharArray();
                foreach (var dir in avaiablenewroomDirections)
                {
                    UpdateDistance_OLD(fromposition:currentloc, currentloc:currentloc+directionsDict[dir.ToString()]);
                }
            }
            else
            {
                return;
            }
        }
        if(distanceDir.ContainsKey(currentloc) == false)
        {
            distanceDir.Add(currentloc,(distanceDir[fromposition]+1));
            var avaiablenewroomDirections = existingRooms[currentloc].Item1.sprite.name.Replace("_OPEN", "").ToCharArray();
            foreach (var dir in avaiablenewroomDirections)
            {
                UpdateDistance_OLD(fromposition:currentloc, currentloc:currentloc+directionsDict[dir.ToString()]);
            }
        }
    }

    #endregion
}
