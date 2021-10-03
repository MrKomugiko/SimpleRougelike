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
    [SerializeField] public SkillsManager _SkillsManager;
    [SerializeField] GameObject ClearStageWindow;
    [SerializeField] private GameObject TickCounterPrefab;
    [SerializeField] public GameObject SelectionBorderPrefab;
    public static CellScript Player_CELL;
    public static List<CellScript> DamagedCells = new List<CellScript>();
    public static GameManager instance;
    [SerializeField] public GameObject GameOverScreen;
   [SerializeField] public GameObject ContentLootWindow;

    [SerializeField] private bool wybuchWTrakcieWykonywania = false;
    private int _currentTurnNumber = 0;
    private int TurnCounter;
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
    internal Action NextTarget;
    internal Action NextMoveLocation;

    [SerializeField] SelectionPopupController attackSelectorPopup;
    public IEnumerator AddTurn()
    {      
        if(CurrentTurnPhase == TurnPhase.PlayerMovement && PlayerMoved == false)
        {
            roomIsCleared = false;
            if(roomIsCleared == false)
            {
                PlayerManager.instance.RegenerateResourcesAtTurnStart();
                PlayerManager.instance.StaminaConsumeEnabled = true;
            }
            
            PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();
            PlayerManager.instance.MovmentValidator.SpawnMarksOnGrid();
            PlayerManager.instance.MovmentValidator.HighlightValidMoveGrid(true);  

             if(PlayerManager.instance.MovmentValidator.validMovePosiitonsCounter == 1)
            {
                PlayerManager.instance.MovmentValidator.HighlightValidMoveGrid(true);
                Debug.Log("tylko 1 pole na ruch, opcja wyboru pozycji jest pominięta");
                PlayerMoved = true;
                TurnPhaseBegin = true;
                roomIsCleared = false;
                CurrentTurnPhase = TurnPhase.PlayerAttack;
                PlayerManager.instance.MovmentValidator.HideMoveGrid();
                yield return new WaitForSeconds(.05f);
                StartCoroutine(AddTurn());
                yield break;
            }
            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();
            if(tempCurrentCreatureList.Count == 0)
            {
                roomIsCleared = true;
                DungeonRoomScript.Dungeon[DungeonManager.instance.CurrentLocation].SetAllDoorsState(true);
                foreach(var roomDoor in DungeonRoomScript.Dungeon[DungeonManager.instance.CurrentLocation].DoorStatesList)
                {
                    if(roomDoor.Value == true)
                    {
                        DungeonManager.instance.EnableTeleportButton(roomDoor.Key);
                    }
                }
                PlayerManager.instance.MovmentValidator.HighlightValidMoveGrid(false);
            }
            else
            {
                if(roomIsCleared == false)
                     _SkillsManager.TickSkillsCooldowns();

                
            }
        
       
       

            TurnPhaseBegin = true;

            _currentTurnNumber = GameManager.instance.TurnCounter;
            GameManager.instance.TurnCounter = CurrentTurnNumber += 1;
            
            yield return new WaitUntil(()=>PlayerMoved && PlayerManager.instance.playerCurrentlyMoving==false);
            yield return new WaitForSeconds(turnPhaseDelay);
            PlayerManager.instance.MovmentValidator.HideMoveGrid();
            PlayerMoved = false;
            TurnPhaseBegin = false;
            CurrentTurnPhase = TurnPhase.PlayerAttack;

            yield return new WaitForSeconds(.05f);
            StartCoroutine(AddTurn());
            yield break;
        }
       
        if(CurrentTurnPhase == TurnPhase.PlayerAttack && PlayerAttacked == false)
        {
            TurnPhaseBegin = true;
            if(roomIsCleared)
            {
                // skip attack turn;
                Debug.Log("room is cleared");
                EndPlayerAttackTurn();
                yield break;
            }


            if(PlayerManager.instance.CurrentStamina >= 1) 
            {
                Debug.Log("attack turn - current stamina >=1");
                attackSelectorPopup.OPENandSpawnInitNodesTree();

               // yield return new WaitWhile(()=>PlayerManager.instance.AtackAnimationInProgress);
                yield return new WaitUntil(()=>GameManager.instance.PlayerAttacked);
                Debug.Log("czekam za koncem animacji ataku");

                yield return new WaitUntil(()=>SkillsManager.SkillAnimationFinished);
                Debug.Log("koniec animacji ataku, przejscie do tury ruchu mobkow");
            }
            else
            {
                // brak staminy na cokolwiek
                Debug.Log("brak staminy end of attack turn");
                EndPlayerAttackTurn();
                yield break;
            }

            PlayerManager.instance.MovmentValidator.HideAttackGrid();
          
            CurrentTurnPhase = TurnPhase.MonsterMovement;
            PlayerAttacked = false;
            TurnPhaseBegin = false;
            StartCoroutine(AddTurn());
            yield break;
        }

        if(CurrentTurnPhase == TurnPhase.MonsterMovement && MonstersMoved == false)
        {
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
                    countMonsterMoveThisTurn++;
                    continue;
                }
            }

            if(tempCurrentCreatureList.Count == 0)
                DungeonRoomScript.Dungeon[DungeonManager.instance.CurrentLocation].SetAllDoorsState(true);

            yield return new WaitUntil(()=>tempCurrentCreatureList.Where(m=>m.ParentCell.IsCurrentlyMoving).Count()==0);
            MonstersMoved = true;

            yield return new WaitUntil(()=>GameManager.instance.MonstersMoved);

            MonstersMoved = false;
            TurnPhaseBegin = false;
            CurrentTurnPhase = TurnPhase.MonsterAttack;
            if(tempCurrentCreatureList.Count > 0) yield return new WaitForSeconds(.05f);
            StartCoroutine(AddTurn());
            yield break;
        }

        if(CurrentTurnPhase == TurnPhase.MonsterAttack && MonsterAttack == false)
        {
           TurnPhaseBegin = true;
            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();

            foreach (var creature in tempCurrentCreatureList)
            {

                if(creature.ISReadyToMakeAction == false) continue;
                if(creature.TryAttack(GameManager.Player_CELL))
                {
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

            MonsterAttack = false;
            TurnPhaseBegin = false;

            CurrentTurnPhase = TurnPhase.PlayerMovement;
            StartCoroutine(AddTurn());
            yield break;
        }
       
        if(CurrentTurnPhase == TurnPhase.MapClear)
        {
            TurnPhaseBegin = true;
            GameManager.instance.ClearStageWindow.SetActive(true);
            PlayerManager.instance.SavePlayerData();
            yield break;
        }
    }

    public void EndPlayerAttackTurn()
    {
        PlayerManager.instance.MovmentValidator.HideAllGrid();
        CurrentTurnPhase = TurnPhase.MonsterMovement;
        PlayerAttacked = false;
        TurnPhaseBegin = false;
        StartCoroutine(AddTurn());
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
        TurnPhaseBegin = false;
        CurrentTurnPhase = TurnPhase.MapClear;
        StopAllCoroutines();
        StartCoroutine(AddTurn());
    }

       public void CloseClearWindowBackToDungeon()
    {
        TurnPhaseBegin = false;
        PlayerMoved = false;
        PlayerAttacked = false;
        CurrentTurnPhase = TurnPhase.PlayerMovement;
        StopAllCoroutines();
        StartCoroutine(AddTurn());
    }

    public float turnPhaseDelay = 0f;
    public int attackAnimationFrames = 16;   

    [SerializeField] public ChestLootWindowScript _chestLootScript;
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
                cellsToDestroy.Remove(cell);
                DamagedCells.Remove(cell);
                continue; 
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
            PlayerManager.instance.MovmentValidator.HighlightValidMoveGrid(_restrictedByStaminavalue:true);
        }
        else if (GameManager.instance.CurrentTurnPhase == TurnPhase.PlayerAttack)
        {
            PlayerManager.instance.MovmentValidator.HighlightValidAttackGrid();
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
    private bool roomIsCleared;

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
        DungeonManager.instance.DungeonClearAndGoToCamp();
        PlayerManager.instance.SavePlayerData();
        MenuScript.instance.KickToMenuAfterDeath(); 
    }
}