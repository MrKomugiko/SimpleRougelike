using UnityEngine;

public class PlayerManager
{
    
    static public int Level = 1;
    static public int Strength = 1;
    static public int Inteligence = 1;
    static public int Dexterity = 1;
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public static PlayerManager PlayerInstance;
    public PlayerManager(Player_Cell parentCell)
    {
        _playerCell = parentCell;
        _mainBackpack = GameObject.Find("Content_EquipmentTab").GetComponent<EquipmentScript>();
        PlayerInstance = this;
    }
}