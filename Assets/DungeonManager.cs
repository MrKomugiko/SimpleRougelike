using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class DungeonManager : MonoBehaviour
{
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
        GridManager.instance.RandomizeDataOnGrid();

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;

        GameManager.instance.CurrentTurnNumber = 1;
        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        StartCoroutine(GameManager.instance.AddTurn());
    }

    public void DungeonClearedSaveAndBackToCamp()
    {
      //  recentDungeonStage = dungeonData.stage;
        recentDungeonStage++; // tymczasowe

        GameManager.instance._chestLootScript.Clear();
        NotificationManger.instance.NotificationList.ForEach(n=>Destroy(n.gameObject.transform.parent.gameObject));
        NotificationManger.instance.NotificationList.Clear();
        foreach (var cell in GridManager.CellGridTable)
        {
            Destroy(cell.Value.gameObject);
        }
        GridManager.destroyedTilesPool.Clear();
        GridManager.CellGridTable.Clear();
        GameObject.Find("BottomSection").GetComponent<AnimateWindowScript>().HideTabWindow();
        DungeonCanvas.SetActive(false);

        maxDungeonStage = recentDungeonStage>maxDungeonStage?recentDungeonStage:maxDungeonStage;
    }
}
