using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public partial class DungeonRoomScript
{
    public class RoomGridData
    {
        public Dictionary<string,MonsterBackupData> Backup_Monsters = new Dictionary<string, MonsterBackupData>();
        public Dictionary<string,TreasureBackupData> Backup_Treasures = new Dictionary<string,TreasureBackupData >();
        public Dictionary<string,BombBackupData> Backup_Bombs = new Dictionary<string, BombBackupData>();
        public Dictionary<string,PortalBackupData> Backup_Portals = new Dictionary<string, PortalBackupData>();
        public HashSet<string> WallPositions = new HashSet<string>();
        
        public RoomGridData(
            Dictionary<string, MonsterBackupData> backup_Monsters, 
            Dictionary<string, TreasureBackupData> backup_Treasures, 
            Dictionary<string, BombBackupData> backup_Bombs, 
            Dictionary<string, PortalBackupData> backup_portals, 
            HashSet<string> wallPositions
            )
        {
            Backup_Monsters = backup_Monsters;
            Backup_Treasures = backup_Treasures;
            Backup_Bombs = backup_Bombs;
            WallPositions = wallPositions;
            Backup_Portals = backup_portals;
        }
    }
}
