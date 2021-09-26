using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int CurrentStageLevel = 1;
    public int CurrentStageFloor = 1;
    [SerializeField] GameObject ClearStageWindow;
    [SerializeField] public TextMeshProUGUI TurnInfo_TMP;
    [SerializeField] public TextMeshProUGUI TurnCounter_TMP;    
    [SerializeField] public Vector2Int StartingPlayerPosition;
    [SerializeField] private GameObject TickCounterPrefab;
    [SerializeField] public GameObject SelectionBorderPrefab;
    public static CellScript Player_CELL;
    public static List<CellScript> DamagedCells = new List<CellScript>();
    public static GameManager instance;
    [SerializeField] public GameObject GameOverScreen;
   [SerializeField] public GameObject ContentLootWindow;

    [SerializeField] private bool wybuchWTrakcieWykonywania = false;
    private int _currentTurnNumber = 0;
    public TurnPhase CurrentTurnPhase = TurnPhase.StartGame;
    public int CurrentTurnNumber
    {
        get => _currentTurnNumber;
        set
        {
            _currentTurnNumber = value;
            foreach (var tile in GridManager.CellGridTable.Where(cell => cell.Value.SpecialTile != null).Select(c => c.Value))
            {
                IncrementTickCounterOnBombCells(tile);

                if (tile.SpecialTile is IFragile) {
                    if ((tile.SpecialTile as IUsable).IsReadyToUse) 
                    {
                        ActivateSpecialTileIfIsReady(tile);
                    }
                }
                if(tile.SpecialTile is Bomb_Cell)
                {
                    if((tile.SpecialTile as Bomb_Cell).IsImpactAreaHighlihted)
                    {
                        var existingnotification = NotificationManger.instance.NotificationList.Where(c=>(c.BaseCell == tile)).FirstOrDefault();
                        if(existingnotification != null)
                        {
                            var x = existingnotification.PossibleActions.GetComponent<ActionSwitchController>();
                            if(tile.SpecialTile is Player_Cell == false)
                                x.Refresh(tile.SpecialTile);
                        }
                        (tile.SpecialTile as Bomb_Cell).SwitchHighlightImpactArea();
                    }
                }
            }
        }
    }
    public bool teleporsEmergencySwitchUsed;
    public bool TurnPhaseBegin = true;
    [SerializeField] private List<Image> TurnImageIndicators = new List<Image>();
    public IEnumerator AddTurn()
    {   
        if(CurrentTurnPhase == TurnPhase.PlayerMovement && PlayerMoved == false)
        {
            //----------------------------------------------------------------------------------
            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();
            if(tempCurrentCreatureList.Count == 0)
            {
                // nie ma potworów na mapie =>uaktywnij teleporty
                DungeonRoomScript.Dungeon[DungeonManager.instance.CurrentLocation].SetAllDoorsState(true);
                foreach(var roomDoor in DungeonRoomScript.Dungeon[DungeonManager.instance.CurrentLocation].DoorStatesList)
                {
                    if(roomDoor.Value == true)
                    {
                        DungeonManager.instance.EnableTeleportButton(roomDoor.Key);
                    }
                }
            }

           // Debug.LogWarning("Player move - start");

            TurnImageIndicators[0].color = Color.yellow;
            TurnImageIndicators[1].color = Color.red;
            TurnImageIndicators[2].color = Color.red;
            TurnImageIndicators[3].color = Color.red;

            TurnPhaseBegin = true;
            instance.TurnInfo_TMP.SetText("PLAYER MOVE TURN BEGIN");

            _currentTurnNumber = Int32.Parse(TurnCounter_TMP.text);
            TurnCounter_TMP.SetText((CurrentTurnNumber += 1).ToString());

            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
            // if(teleporsEmergencySwitchUsed == false)
            // {
                if(PlayerManager.instance.MovmentValidator.validMovePosiitonsCounter == 1)
                {
                    // zablokowane miejsce na ruch, tylko atak bezposrednio niech sie aktywuje
                    //Debug.Log("skip player movement turn = "+PlayerManager.instance.MovmentValidator.validMovePosiitonsCounter);
                    PlayerMoved = true;
                }
            // }
            yield return new WaitUntil(()=>PlayerMoved && PlayerManager.instance.playerCurrentlyMoving==false);
            instance.TurnInfo_TMP.SetText("PLAYER MOVE TURN ENDED");
            yield return new WaitForSeconds(turnPhaseDelay);
           
            PlayerMoved = false;
            TurnPhaseBegin = false;
            CurrentTurnPhase = TurnPhase.PlayerAttack;

            yield return new WaitForSeconds(.05f);
            StartCoroutine(AddTurn());
            yield break;
        }
       
        if(CurrentTurnPhase == TurnPhase.PlayerAttack && PlayerAttacked == false)
        {
            //Debug.LogWarning("Player attack - start");

            TurnPhaseBegin = true;

           int _monstersInRange = PlayerManager.instance.MovmentValidator.ShowValidAttackGrid();
           if(_monstersInRange == 0)
           {
               //Debug.Log("no monsters to attack");
            //    if(teleporsEmergencySwitchUsed == false)
            //    {
            //         PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
            //         if(PlayerManager.instance.MovmentValidator.validMovePosiitonsCounter == 1)
            //         {
            //             foreach(var teleport in telerpotButtons)
            //             {
            //                 Debug.Log("włączenie teleportu:"+teleport.name);
            //                 teleport.transform.Find("Teleport_ON").GetComponent<SpriteRenderer>().enabled = true;
            //                 teleport.GetComponent<Button>().interactable = true;
            //                 teleport.GetComponent<Image>().raycastTarget = true;          
            //             }
            //             teleporsEmergencySwitchUsed = true;
            //             Debug.LogError("Awaryjne odbezpieczenie teleportow na mapie spowodowane napotkaniem blokady zaraz po pojawieniu sie na mapie");
            //         }
            //    }

                PlayerManager.instance.MovmentValidator.HideGrid();
                CurrentTurnPhase = TurnPhase.MonsterMovement;
                // TurnImageIndicators[1].color = Color.green;
                // TurnImageIndicators[2].color = Color.yellow;
                PlayerAttacked = false;
                TurnPhaseBegin = false;
               // Debug.LogWarning("Player attack - end - no monsters to attack");
                StartCoroutine(AddTurn());
                yield break;
           }
            yield return new WaitWhile(()=>PlayerManager.instance.AtackAnimationInProgress);
            yield return new WaitUntil(()=>GameManager.instance.PlayerAttacked);
            instance.TurnInfo_TMP.SetText("PLAYER ATTACK TURN ENDED");

            yield return new WaitForSeconds(turnPhaseDelay);
            CurrentTurnPhase = TurnPhase.MonsterMovement;
            // TurnImageIndicators[1].color = Color.green;
            // TurnImageIndicators[2].color = Color.yellow;
            PlayerManager.instance.MovmentValidator.HideGrid();
            PlayerAttacked = false;
            TurnPhaseBegin = false;
          //  Debug.LogWarning("Player attack - end");
            yield return new WaitForSeconds(.05f);
            StartCoroutine(AddTurn());
            yield break;
        }

        if(CurrentTurnPhase == TurnPhase.MonsterMovement && MonstersMoved == false)
        {
           // Debug.LogWarning("Monster move - start");
            TurnPhaseBegin = true;

            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();

            foreach (var creature in tempCurrentCreatureList)
            {
                creature.TurnsElapsedCounter ++;

                if(creature.ISReadyToMakeAction == false) continue;

                if(creature.TryMove(GameManager.Player_CELL))
                {
                    NotificationManger.TriggerActionNotification(creature, NotificationManger.AlertCategory.Info, "Moved.");
                    // yield return new WaitForSeconds(.05f);
                    countMonsterMoveThisTurn++;
                    continue;
                }
            }

            if(tempCurrentCreatureList.Count == 0)
            {
                // nie ma potworów na mapie =>uaktywnij teleporty
                // 1. zapisanie stanu teleportow
               // Debug.Log("Room cleared!");
                DungeonRoomScript.Dungeon[DungeonManager.instance.CurrentLocation].SetAllDoorsState(true);
            }

            yield return new WaitUntil(()=>tempCurrentCreatureList.Where(m=>m.ParentCell.IsCurrentlyMoving).Count()==0);
            MonstersMoved = true;

            yield return new WaitUntil(()=>GameManager.instance.MonstersMoved);
            instance.TurnInfo_TMP.SetText("MONSTER MOVEMENT TURN ENDED");

            MonstersMoved = false;
            TurnPhaseBegin = false;
            CurrentTurnPhase = TurnPhase.MonsterAttack;
            // TurnImageIndicators[2].color = Color.green;
            // TurnImageIndicators[3].color = Color.yellow;
            //Debug.LogWarning("Monster move - end");

            if(tempCurrentCreatureList.Count > 0) yield return new WaitForSeconds(.05f);
            StartCoroutine(AddTurn());
            yield break;
        }

        if(CurrentTurnPhase == TurnPhase.MonsterAttack && MonsterAttack == false)
        {
           // Debug.LogWarning("Monster attack - start");
            TurnPhaseBegin = true;

            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();

            foreach (var creature in tempCurrentCreatureList)
            {
              //  creature.TurnsElapsedCounter ++;

                if(creature.ISReadyToMakeAction == false) continue;

                if(creature.TryAttack(GameManager.Player_CELL))
                {
                  //  yield return new WaitForSeconds(.05f);
                   PlayerManager.instance.StartCoroutine(
                       PlayerManager.instance.PerformRegularAttackAnimation(
                           creature.ParentCell,
                           PlayerManager.instance._playerCell.ParentCell,
                           attackAnimationFrames));
                           countMonsterAttackThisTurn++;
                    continue;   
                }

                 NotificationManger.TriggerActionNotification(creature, NotificationManger.AlertCategory.Info, "Waiting for turn.");
            }
            MonsterAttack = true;

            yield return new WaitWhile(()=>PlayerManager.instance.AtackAnimationInProgress);
            yield return new WaitUntil(()=>GameManager.instance.MonsterAttack);
            instance.TurnInfo_TMP.SetText("MONSTER ATTACKS TURN ENDED");

            MonsterAttack = false;
            TurnPhaseBegin = false;

            // TurnImageIndicators[3].color = Color.green;
            CurrentTurnPhase = TurnPhase.PlayerMovement;
          //  Debug.LogWarning("Monster attack - end");
            StartCoroutine(AddTurn());
            yield break;
        }
       
        if(CurrentTurnPhase == TurnPhase.MapClear)
        {
            TurnPhaseBegin = true;
            GameManager.instance.ClearStageWindow.SetActive(true);
            PlayerManager.instance.SavePlayerData();
            //print("end of loop");
            yield break;
        }
    }
    [SerializeField] TextMeshProUGUI DELETECONSOLELOGS;
    [SerializeField] TextMeshProUGUI DELETECONSOLE_BTNTEXT;
    public void BACKFROMCLEARINGGAMEDATA()
    {
        DELETECONSOLE_BTNTEXT.SetText("Clear game data");
        DELETECONSOLELOGS.SetText("");
    }
    public void CLEARGAMEDATAFROMDEVICE()
    {
        var files = Directory.GetFiles(Application.persistentDataPath);
        if(DELETECONSOLE_BTNTEXT.text == "Clear game data")
        {
            string logs = "";
            foreach(var file in files)
            {
                logs += file.ToString().Replace(Application.persistentDataPath,"...")+"\n";
            }
            DELETECONSOLELOGS.SetText(logs);

            DELETECONSOLE_BTNTEXT.SetText("Tap to confirm");
            return;
        }

        if(DELETECONSOLE_BTNTEXT.text == "Tap to confirm")
        {
            Directory.CreateDirectory(Application.persistentDataPath + $"/TrashFiles");
            int count = Directory.GetFiles(Application.persistentDataPath+"/TrashFiles").Count();
            foreach(var file in files)
            {
                count++;
                File.Move(file, Application.persistentDataPath + $"/TrashFiles/_EMPTY_{count}.json");
            }
            DELETECONSOLE_BTNTEXT.SetText("Clear game data");
            DELETECONSOLELOGS.SetText("");

            HeroDataController.instance.CreateEmptyHeroCards();
            HeroDataController.instance.LoadHeroesDataFromDevice();
        }
    }
    public void EndTurnLoopShowClearDungeonWindow()
    {
        //Debug.Log("Map cleared");
        TurnPhaseBegin = false;
        CurrentTurnPhase = TurnPhase.MapClear;
        StartCoroutine(AddTurn());
    }

       public void CloseClearWindowBackToDungeon()
    {
       // Debug.Log("back to dungeon");
        TurnPhaseBegin = false;
        PlayerMoved = false;
        PlayerAttacked = false;
        CurrentTurnPhase = TurnPhase.PlayerMovement;
        StartCoroutine(AddTurn());
    }

    public float turnPhaseDelay = 0f;
    public int attackAnimationFrames = 16;   

    [SerializeField] public ChestLootWindowScript _chestLootScript;
    // [SerializeField] public EquipmentScript _playerBackpackequipmentScript;
    public enum TurnPhase
    {
        StartGame,
        MapClear,
        Lose,
        PlayerMovement,
        PlayerAttack,
        MonsterMovement,
        MonsterAttack
    }
       public bool WybuchWTrakcieWykonywania { get => wybuchWTrakcieWykonywania; set {

        wybuchWTrakcieWykonywania = value; 
        }
    }
    public GameObject InstantiateTicker(Bomb_Cell bomb_Cellcs)
    {
        return Instantiate(TickCounterPrefab, bomb_Cellcs.ParentCell.transform);
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void Init_PlacePlayerOnGrid(Vector2Int playerStatingPositon)
    {
        Player_CELL = GridManager.CellGridTable[playerStatingPositon];
        Player_CELL.Trash.ForEach(t => Destroy(t.gameObject));
        Player_CELL.Trash.Clear();
        Player_CELL.AssignType(TileTypes.player);
        Player_CELL.IsWalkable = true;
    }
    public IEnumerator routine_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
        DamagedCells.AddRange(cellsToDestroy);
        yield return new WaitWhile(() => WybuchWTrakcieWykonywania);
        WybuchWTrakcieWykonywania = true;
        yield return new WaitForSeconds(time);

        var tempList = cellsToDestroy.Where(c => c != null).ToList();
        foreach (var cell in tempList)
        {
            if (cell.SpecialTile is Player_Cell) 
            {
               // print("gracz oberawał");
                cellsToDestroy.Remove(cell);
                DamagedCells.Remove(cell);
                continue; //TODO: wyodrębnoć klase player, dodać/zmienic IEnemy na coś uniwersalnego ? IEntity ? zawierac bedzie hp, exp , funkcja ataku obranonu nvm
            }

            if(cell.SpecialTile is ICreature) 
            {
                var enemy =  (cell.SpecialTile as ICreature);
                if(enemy.IsAlive)
                {
                    cellsToDestroy.Remove(cell);
                    DamagedCells.Remove(cell);
                    continue;
                }
                else
                {
                    // print("monster died and should leave his bones on this cell");
                    //TODO: POZMIANA STWORKA NA ZWŁOKI/drop, do tego jakas infomacja ze zmarło mu sie xd
                    cellsToDestroy.Remove(cell);
                    DamagedCells.Remove(cell);
                    continue;
                }
            }
            GridManager.SendToGraveyard(cell.CurrentPosition);  
        }
        
        cellsToDestroy.Where(c => c != null).ToList().ForEach(cell => cell.Trash.ForEach(trash => Destroy(trash.gameObject)));
        cellsToDestroy.Where(c => c != null).ToList().ForEach(cell =>
                {
                    cell.SpecialTile = null;
                    cell.Trash.Clear();
                }
            );
        cellsToDestroy.ForEach(cell => DamagedCells.Remove(cell));

        WybuchWTrakcieWykonywania = false;
     
        GridManager.FillGaps();
        if(GameManager.instance.CurrentTurnPhase == TurnPhase.PlayerMovement)
        {
            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
        }
        else if (GameManager.instance.CurrentTurnPhase == TurnPhase.PlayerAttack)
        {
            PlayerManager.instance.MovmentValidator.ShowValidAttackGrid();
        }
        yield return null;
    }


    private static void IncrementTickCounterOnBombCells(CellScript tile)
    {
        if (tile.SpecialTile is Bomb_Cell)
        {
            if ((tile.SpecialTile as Bomb_Cell).TickCounter != null)
            {
                (tile.SpecialTile as Bomb_Cell).TickCounter.AddTick(1);
            }
        }
    }
    private static void ActivateSpecialTileIfIsReady(CellScript tile)
    {
        if ((tile.SpecialTile as IUsable).IsReadyToUse == true)
        {
            tile.Trash.ForEach(trash =>
            {
                if (trash != null)
                {
                    var trash_SR = trash.GetComponent<SpriteRenderer>();
                    if (trash_SR != null)
                    {
                        if (trash_SR.name.Contains("icon"))
                        {
                            trash_SR.color = Color.red;
                        }
                    }
                }
            });
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------
    [SerializeField] private List<MonsterData> MonsterVariants = new List<MonsterData>();
    [SerializeField] private List<TreasureData> TreasureVariants = new List<TreasureData>();
    [SerializeField] private List<BombData> BombVariants = new List<BombData>();
    [SerializeField] public GameObject WALLSPRITE;
    internal static string LastPlayerDirection;

    public bool MonstersMoved;
    public bool PlayerAttacked;
    public bool PlayerMoved;
    public bool MonsterAttack;
    public bool SwapTilesAsDefault = true;
    public bool SlideAsDefault = false;
    private int countMonsterMoveThisTurn;
    private int countMonsterAttackThisTurn;
    public bool MovingRequestTriggered = false;
    [SerializeField] public PlayerProgressModel PLAYER_PROGRESS_DATA;

    internal MonsterData GetMonsterData(int MonsterID = -1)
    {
        if(MonsterID == -1)
        {
            int randomIndex = 0;
            while(true)
            {
                randomIndex = UnityEngine.Random.Range(0,MonsterVariants.Count);
                if(MonsterVariants[randomIndex].ID != 666)
                break;
            }
            return MonsterVariants[randomIndex];
        }
        else
            return MonsterVariants.Where(m=>m.ID == MonsterID).First();
    }
    internal TreasureData GetTreasureData(int TreasureID = -1)
    {
        if(TreasureID == -1)
        {
            var randomIndex = UnityEngine.Random.Range(0,TreasureVariants.Count);
            return TreasureVariants[randomIndex];
        }
        else
            return TreasureVariants.Where(m=>m.ID == TreasureID).First();
    }
    internal BombData GetBombData(int BombID = -1)
    {
        if(BombID == -1)
        {
            var randomIndex = UnityEngine.Random.Range(0,BombVariants.Count);
            return BombVariants[randomIndex];
        }
        else
            return BombVariants.Where(m=>m.ID == BombID).First();
    }

    public void GameOver()
    {
       // Debug.Log("GameOver, create new account sorry xd");

       // Debug.Log("wyczyszczenie danych mapy dunga i wyjscie do camp'u");
        DungeonManager.instance.DungeonClearAndGoToCamp();

       // Debug.Log("zapisanie danych gracza");
        PlayerManager.instance.SavePlayerData();

       // Debug.Log("wyjscie do menu głownego");
        MenuScript.instance.KickToMenuAfterDeath();
        
    }
}