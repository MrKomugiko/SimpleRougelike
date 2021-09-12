using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Chest;

[Serializable] public class PlayerProgressModel
{
    public DateTime CreatedDate;
    public DateTime LastVisitedDate;

    // ----------------------------- CORE  ------------------------
    public string NickName = "Nick";
    public int Level = 1;
    public int Experience = 1;
    public int MaxHealth=25;
    public int CurrentHealth = 25;
    public int Power = 100;

    // ----------------------------- MAIN STATS  ------------------
    public int Strength = 1;
    public int Inteligence = 1;
    public int Dexterity = 1;
    public int Vitality = 1;

    // ----------------------------- CURRECIES  -------------------
    public int Gold = 0;
    public int Cristals = 1;

   // ----------------------------- LOCATION AND PROGRESS  --------
    public string CurrentLocation = "Home";
    public int HighestDungeonStage = 0;
    public int RecentDungeonStage = 0;

    // ----------------------------- EQUIPMENT --------------------
    List<ItemPack> EquipedItems = new List<ItemPack>();
    List<ItemPack> BagpackItems = new List<ItemPack>();

    public PlayerProgressModel(string _nickname)
    {
        Debug.Log("generated default hero named: "+_nickname);
        NickName = _nickname;
        CreatedDate = DateTime.Now;
        LastVisitedDate = CreatedDate;
    }

    // ----------------------------- ARCHIVMENTS PROGRES ----------
    // ----------------------------- UNLOCKED THINGS --------------
    // ----------------------------- ITEMS IN QUICKSLOT -----------

}
