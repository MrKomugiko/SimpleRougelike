using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class DungeonRoomScript
{
    public class Room
    {
        public int DistanceFromCenter = -1;
        public bool WasVisited = false;
        public string position;
        public String doorsNameCode;
        public RoomGridData DATA = new RoomGridData();
        public Room(string position, string doorsNameCode)
        {
            this.position = position.ToString();
            this.doorsNameCode = doorsNameCode;
            
            foreach(var doorCode in doorsNameCode.ToCharArray().ToList())
            {
                DoorStatesList.Add(doorCode.ToString(),false);
            }
        }
        public Dictionary<string,bool> DoorStatesList = new Dictionary<string, bool>();
        public bool ContainPortal;

        public void SetStateDoorByCode(string code, bool state)
        {
            if(DoorStatesList[code] == true && state == true) return; 
            if(DoorStatesList[code] == false && state == false) return; 


            DoorStatesList[code] = state;
            DungeonManager.SetNeighourRoomsDoorsState(this, new List<Vector2Int>(){directionsDict[code]});
        }
        public void SetStateDoorByVector(Vector2Int direction, bool state)
        {
            string code = directionsDict.Where(v=>v.Value == direction).First().Key;

            if(DoorStatesList[code] == true && state == true) return; 
            if(DoorStatesList[code] == false && state == false) return; 

            DoorStatesList[code] = state;
            DungeonManager.SetNeighourRoomsDoorsState(this, new List<Vector2Int>(){directionsDict[code]});
        }
        public void SetAllDoorsState(bool state = true)
        {
            if(DoorStatesList.Values.Where(v=>v == false).Count() == 0 && state == true) return; 
            if(DoorStatesList.Values.Where(v=>v == true).Count() == 0 && state == false) return; 

            List<Vector2Int> listchangedDoors = new List<Vector2Int>();
            foreach(var doorCode in doorsNameCode.ToCharArray().ToList())
            {
                DoorStatesList[doorCode.ToString()] = state;
                listchangedDoors.Add(directionsDict[doorCode.ToString()]);
            }
            
            DungeonManager.SetNeighourRoomsDoorsState(this, listchangedDoors);
        }
        public List<Room> GetNeighbourRooms()
        {
            List<Room> neigbours = new List<Room>();
            foreach(var doorcode in doorsNameCode)
            {
                neigbours.Add(Dungeon[GameManager.ConvertToVector2Int(position) + directionsDict[doorcode.ToString()]]);
            }
            return neigbours;
        }
    }
}
