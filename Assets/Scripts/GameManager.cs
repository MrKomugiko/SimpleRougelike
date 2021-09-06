using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
    public bool TurnFinished = true;
    public IEnumerator AddTurn()
    {
        if(CurrentTurnPhase == TurnPhase.PlayerMovement && PlayerMoved == false)
        {
            TurnFinished = true;
            print("PLAYER MOVE TURN BEGIN");

            _currentTurnNumber = Int32.Parse(TurnCounter_TMP.text);
            TurnCounter_TMP.SetText((CurrentTurnNumber += 1).ToString());

            PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();

            yield return new WaitUntil(()=>GameManager.instance.PlayerMoved);
            print("PLAYER MOVE TURN ENDED");
            PlayerManager.instance.MovmentValidator.HideGrid();
            PlayerMoved = false;
            CurrentTurnPhase = TurnPhase.PlayerAttack;
            StartCoroutine(AddTurn());
            yield break;
        }
        if(CurrentTurnPhase == TurnPhase.PlayerAttack && PlayerAttacked == false)
        {
            TurnFinished = true;

           int _monstersInRange = PlayerManager.instance.MovmentValidator.ShowValidAttackGrid();
           if(_monstersInRange == 0)
           {
                PlayerManager.instance.MovmentValidator.HideGrid();
                print("There is no monsters around able to attack");
                CurrentTurnPhase = TurnPhase.MonsterMovement;
                PlayerAttacked = false;
                print("PLAYER ATTACK TURN ENDED");
                StartCoroutine(AddTurn());
                yield break;
           }

            yield return new WaitUntil(()=>GameManager.instance.PlayerAttacked);
            CurrentTurnPhase = TurnPhase.MonsterMovement;
            PlayerManager.instance.MovmentValidator.HideGrid();
            print("PLAYER ATTACK TURN ENDED");
            PlayerAttacked = false;
            StartCoroutine(AddTurn());
            yield break;
        }

        if(CurrentTurnPhase == TurnPhase.MonsterMovement && MonstersMoved == false)
        {
            print("MONSTER MOVEMENT TURN BEGIN");
            TurnFinished = true;

            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();

            foreach (var creature in tempCurrentCreatureList)
            {
                creature.TurnsElapsedCounter ++;

                if(creature.ISReadyToMakeAction == false) continue;

                if(creature.TryMove(GameManager.Player_CELL))
                {
                    NotificationManger.TriggerActionNotification(creature, NotificationManger.AlertCategory.Info, "Moved.");
                   // yield return new WaitForSeconds(.1f);
                    continue;
                }
            }
            
             MonstersMoved = true;

            yield return new WaitUntil(()=>GameManager.instance.MonstersMoved);
            print("MONSTER MOVEMENT TURN ENDED");
            MonstersMoved = false;
            CurrentTurnPhase = TurnPhase.MonsterAttack;
            StartCoroutine(AddTurn());
            yield break;
        }

        if(CurrentTurnPhase == TurnPhase.MonsterAttack && MonsterAttack == false)
        {
            print("MONSTER ATTACKS TURN BEGIN");
            TurnFinished = true;

            List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
            tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();

            foreach (var creature in tempCurrentCreatureList)
            {
              //  creature.TurnsElapsedCounter ++;

                if(creature.ISReadyToMakeAction == false) continue;

                if(creature.TryAttack(GameManager.Player_CELL))
                {
                  //  yield return new WaitForSeconds(.1f);
                    continue;   
                }

                 NotificationManger.TriggerActionNotification(creature, NotificationManger.AlertCategory.Info, "Waiting for turn.");
            }
            MonsterAttack = true;


            yield return new WaitUntil(()=>GameManager.instance.MonsterAttack);
            print("MONSTER ATTACKS TURN ENDED");
            MonsterAttack = false;
            CurrentTurnPhase = TurnPhase.PlayerMovement;
            StartCoroutine(AddTurn());
            yield break;;
        }
    }


 
    [SerializeField] ChestLootWindowScript _chestLootScript;
    [SerializeField] EquipmentScript _equipmentScript;
    public enum TurnPhase
    {
        StartGame,
        Win,
        Lose,
        PlayerMovement,
        PlayerAttack,
        MonsterMovement,
        MonsterAttack
    }
    internal static void Restart()
    {      
        GameManager.instance.CurrentTurnPhase = TurnPhase.StartGame;
        PlayerManager.instance.Restart_ClearStatsAndEquipedItems();
        GameManager.instance.CurrentTurnNumber = 0;
        GameManager.instance.PlayerMoved = false;
        GameManager.instance.PlayerAttacked = false;
        GameManager.instance.MonstersMoved = false;
        GameManager.instance.MonsterAttack = false;

        instance._chestLootScript.Clear();
        instance._equipmentScript.Clear();

        NotificationManger.instance.NotificationList.ForEach(n=>Destroy(n.gameObject.transform.parent.gameObject));
        NotificationManger.instance.NotificationList.Clear();
        
        foreach (var cell in GridManager.CellGridTable)
        {
            Destroy(cell.Value.gameObject);
        }

        GridManager.destroyedTilesPool.Clear();
        GridManager.CellGridTable.Clear();

        GameObject.Find("BottomSection").GetComponent<AnimateWindowScript>().HideTabWindow();

        GridManager.instance.Start();
        GameManager.instance.Init_PlacePlayerOnGrid();

        GameManager.instance.TurnCounter_TMP.SetText("0");
        PlayerManager.instance.GoldCounter_TMP.SetText("0");
        PlayerManager.instance.HealthCounter_TMP.SetText((GameManager.Player_CELL.SpecialTile as ILivingThing).HealthPoints.ToString());
        PlayerManager.instance.ExperienceCounter_TMP.SetText("0");

        GameManager.instance.CurrentTurnPhase = TurnPhase.PlayerMovement;
        GameManager.instance.StopAllCoroutines();
        GameManager.instance.StartCoroutine(GameManager.instance.AddTurn());
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
    private void Start()
    {
        Init_PlacePlayerOnGrid();
    }
    private void Init_PlacePlayerOnGrid()
    {
        Player_CELL = GridManager.CellGridTable[StartingPlayerPosition];
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
        PlayerManager.instance.MovmentValidator.ShowValidMoveGrid();
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

    internal void ExecuteCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
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
}