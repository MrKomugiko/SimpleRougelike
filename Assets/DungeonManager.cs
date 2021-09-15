using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class DungeonManager : MonoBehaviour
{
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
}
