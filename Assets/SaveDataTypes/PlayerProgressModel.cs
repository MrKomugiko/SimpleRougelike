using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Chest;
using static EquipmentScript;

[Serializable] public class PlayerProgressModel
{
    public int SlotID;
    public bool isDead = false;
    public bool isDeleted = false;
    //-------------------------------------------------------------
    public DateTime CreatedDate;
    public DateTime LastVisitedDate;

    // ----------------------------- CORE  ------------------------
    public string NickName = "Nick";
    public int Level = 1;
    public int Experience = 1;
    public int MaxHealth=25;
    public int CurrentHealth = 25;
    public int Power = 100;
    public float BaseDamage = 1;

    // ----------------------------- MAIN STATS  ------------------
    public int Strength = 1;
    public int Inteligence = 1;
    public int Dexterity = 1;
    public int Vitality = 1;

    // ----------------------------- CURRECIES  -------------------
    public int Gold = 0;
    public int Cristals = 0;

   // ----------------------------- LOCATION AND PROGRESS  --------
    public string CurrentLocation = "Home";
    public int HighestDungeonStage = 0;
    public int RecentDungeonStage = 0;
    public int MoveRange = 2;
    public int AttackRange = 1;
    public int AvailablePoints = 0;

    // ----------------------------- EQUIPMENT --------------------
   public List<ItemBackupData> EquipedItems = new List<ItemBackupData>();
   public List<ItemBackupData> BagpackItems = new List<ItemBackupData>();

    public PlayerProgressModel(string _nickname,int _slotId)
    {
        NickName = _nickname;
        SlotID = _slotId;
        CreatedDate = DateTime.Now;
        LastVisitedDate = CreatedDate;

       // Debug.Log("Created PPMData with nick: "+_nickname);
    }

    // ----------------------------- ARCHIVMENTS PROGRES ----------
    // ----------------------------- UNLOCKED THINGS --------------
    // ----------------------------- ITEMS IN QUICKSLOT -----------

    public ItemBackupData ItemAssignedToAuicslot_0;
    public ItemBackupData ItemAssignedToAuicslot_1;
    public ItemBackupData ItemAssignedToAuicslot_2;
    public ItemBackupData ItemAssignedToAuicslot_3;
    public ItemBackupData ItemAssignedToAuicslot_4;


}
