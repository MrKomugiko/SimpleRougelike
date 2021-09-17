using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] Button W_BTN, D_BTN, S_BTN, A_BTN;
    public static DungeonManager instance;
    private void Awake() {
        instance = this;
    }
    [SerializeField] GameObject DungeonSelectionWindow;
    public int maxDungeonStage = 0;
    public int recentDungeonStage = 0;
    /* DungeonData dungeondata */
    /* private DungeonData ConfigureDungeonLevel(configurationdata)
    {

        return dungeondata;
    } 
    */
    [SerializeField] private GameObject DungeonCanvas;
    public void OpenDungeon(/* dungeondata */ )
    {
        DungeonSelectionWindow.SetActive(false);
        DungeonCanvas.SetActive(true);
        GridManager.instance.CreateEmptyGrid();
        GridManager.instance.RandomizeDataOnGrid(); // TODO:
        GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4,4));

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.CurrentTurnNumber = 1;
        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        StartCoroutine(GameManager.instance.AddTurn());
    }

    public void DungeonClearAndGoToCamp()
    {
        // poodpinanie zalozonych itemkow w quickslocie gracza
        PlayerManager.instance._mainBackpack.ItemSlots.ForEach(slot=> 
            {
                if(slot.IsInQuickSlot)
                {
                    slot.RemoveFromQuickSlot((int)slot.AssignedToQuickSlot);
                }
            }
        );

        Debug.Log("DUNGEON CLEAR AND BACK TO CAMP");
      //  recentDungeonStage = dungeonData.stage;
        recentDungeonStage++; // tymczasowe

        GameManager.instance._chestLootScript.Clear();
        if(NotificationManger.instance != null)
        {
            NotificationManger.instance.NotificationList.ForEach(n=>Destroy(n.gameObject.transform.parent.gameObject));
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
        
        maxDungeonStage = recentDungeonStage>maxDungeonStage?recentDungeonStage:maxDungeonStage;
    }    

    [SerializeField] Image RoomWallsSprite; 
    Vector2Int CurrentLocation;
    [ContextMenu("generate new dung")]
    public void GenerateAndEnterDungeon()
    {
        Debug.Log("generate dungeon rooms");
        DungeonRoomScript.GenerateDungeonRooms();
        OpenDungeon();
        CurrentLocation = Vector2Int.zero;

        RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t=> t.name == DungeonRoomScript.Dungeon[Vector2Int.zero].doorsNameCode+"_OPEN").First();
    }
    [ContextMenu("move up")]
    public void MoveNExtRoom_Up()
    {
        var vector = Vector2Int.up;
        if(DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation+vector))
        {
            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t=> t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode+"_OPEN").First();
            GridManager.instance.ResetGridToDefault();
            GridManager.instance.RandomizeDataOnGrid(); 
            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4,0));
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation );
        }
    }
       [ContextMenu("move right")]
    public void MoveNExtRoom_Right()
    {
        var vector = Vector2Int.right;
        if(DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation+vector))
        {
            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t=> t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode+"_OPEN").First();
            GridManager.instance.ResetGridToDefault();
            GridManager.instance.RandomizeDataOnGrid(); 
            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(0,4));
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation );
        }
    }
       [ContextMenu("move Down")]
    public void MoveNExtRoom_Down()
    {
        var vector = Vector2Int.down;
        if(DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation+vector))
        {
            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t=> t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode+"_OPEN").First();
            GridManager.instance.ResetGridToDefault();
            GridManager.instance.RandomizeDataOnGrid(); 
            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(4,8));
            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation );
        }
    }


    [ContextMenu("move Left")]
    public void MoveNExtRoomLeftP()
    {
        var vector = Vector2Int.left;
        if(DungeonRoomScript.Dungeon.ContainsKey(CurrentLocation+vector))
        {
            CurrentLocation += vector;
            RoomWallsSprite.sprite = DungeonRoomScript.instance.roomsTemplates.Where(t=> t.name == DungeonRoomScript.Dungeon[CurrentLocation].doorsNameCode+"_OPEN").First();
            GridManager.instance.ResetGridToDefault();
            GridManager.instance.RandomizeDataOnGrid(); 
            GameManager.instance.Init_PlacePlayerOnGrid(new Vector2Int(8,4));

            RestartTurnRoutine();
            ConfigureNextRoomButtons(newLocation: CurrentLocation );
        }
    }

    private void RemoveRoomGrid()
    {
        GameManager.instance._chestLootScript.Clear();
        if(NotificationManger.instance != null)
        {
            NotificationManger.instance.NotificationList.ForEach(n=>Destroy(n.gameObject.transform.parent.gameObject));
            NotificationManger.instance.NotificationList.Clear();
        }
        foreach (var cell in GridManager.CellGridTable)
        {
            Destroy(cell.Value.gameObject);
        }
        GridManager.destroyedTilesPool.Clear();
        GridManager.CellGridTable.Clear();
    }
    private void RestartTurnRoutine()
    {
        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.CurrentTurnNumber = 1;
        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        StartCoroutine(GameManager.instance.AddTurn());
    }

    private void ConfigureNextRoomButtons(Vector2Int newLocation)
    {
       var availableExits = DungeonRoomScript.Dungeon[newLocation].doorsNameCode.ToCharArray().ToList();

        W_BTN.gameObject.SetActive(false);
        S_BTN.gameObject.SetActive(false);
        A_BTN.gameObject.SetActive(false);
        D_BTN.gameObject.SetActive(false);

       foreach(var exitDoor in availableExits  )
       {
           if(exitDoor == Char.Parse("W"))
                W_BTN.gameObject.SetActive(true);

            if(exitDoor == Char.Parse("S"))
                S_BTN.gameObject.SetActive(true);

            if(exitDoor == Char.Parse("A"))
                A_BTN.gameObject.SetActive(true);

            if(exitDoor == Char.Parse("D"))
                D_BTN.gameObject.SetActive(true);
       }
    }
}
