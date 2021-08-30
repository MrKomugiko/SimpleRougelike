using UnityEngine;

public class PlayerManager
{
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