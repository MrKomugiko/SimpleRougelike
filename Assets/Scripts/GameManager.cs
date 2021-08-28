using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI TurnCounter_TMP;
    [SerializeField] public TextMeshProUGUI GoldCounter_TMP;
    [SerializeField] public TextMeshProUGUI ExperienceCounter_TMP;
    [SerializeField] public TextMeshProUGUI HealthCounter_TMP;


    [SerializeField] public Vector2Int StartingPlayerPosition;
    [SerializeField] private GameObject TickCounterPrefab;
    [SerializeField] public GameObject SelectionBorderPrefab;


    public List<GameObject> specialEffectList = new List<GameObject>();

    public static CellScript Player_CELL;
    public static List<CellScript> DamagedCells = new List<CellScript>();
    public static GameManager instance;

    private bool wybuchWTrakcieWykonywania = false;
    private int _currentTurnNumber = 0;
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
                        continue;
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
                            x.Refresh(tile.SpecialTile);
                        }
                        (tile.SpecialTile as Bomb_Cell).SwitchHighlightImpactArea();
                    }
                }
            }
        }
    }

    public bool WybuchWTrakcieWykonywania { get => wybuchWTrakcieWykonywania; set {

        wybuchWTrakcieWykonywania = value; 
        }
    }

    public static List<CellScript> CurrentMovingTiles = new List<CellScript>();

    public IEnumerator AddTurn()
    {
        _currentTurnNumber = Int32.Parse(TurnCounter_TMP.text);
        TurnCounter_TMP.SetText((CurrentTurnNumber += 1).ToString());
        //print("dodanie tury");

        List<ICreature> tempCurrentCreatureList  = new List<ICreature>();
        tempCurrentCreatureList = GridManager.CellGridTable.Where(c => (c.Value.SpecialTile is ICreature)).Select(c=>c.Value.SpecialTile as ICreature).ToList();

        foreach (var creature in tempCurrentCreatureList)
        {
            creature.TurnsElapsedCounter ++;
            if(creature.ISReadyToMakeAction == false) continue;

            if(creature.TryMove(GameManager.Player_CELL))
            {
                NotificationManger.TriggerActionNotification(creature, NotificationManger.AlertCategory.Info, "Moved.");
                continue;
            }

            if(creature.TryAttack(GameManager.Player_CELL))
            {
                continue;   
            }

             NotificationManger.TriggerActionNotification(creature, NotificationManger.AlertCategory.Info, "Waiting for turn.");
        }
        yield return new WaitForSeconds(.25f);
        yield return null;
    }
    public void AddGold(int value)
    {
        int currentGoldValue = Int32.Parse(GoldCounter_TMP.text);
        GoldCounter_TMP.SetText((currentGoldValue + value).ToString());
        NotificationManger.AddValueTo_Gold_Notification(value);
    }
    public void Exit()
    {
        Application.Quit();
    }
    public GameObject InstantiateTicker(Bomb_Cell bomb_Cellcs)
    {
        // print("Instantiate ticker");
        return Instantiate(TickCounterPrefab, bomb_Cellcs.ParentCell.transform);
    }
    // public void Countdown_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    // {
    //     foreach (var damagedCell in GameManager.DamagedCells)
    //     {
    //         if (cellsToDestroy.Contains(damagedCell))
    //         {
    //             cellsToDestroy.Remove(damagedCell);
    //         }
    //     }
    //     StartCoroutine(routine_SendToGraveyard(time, cellsToDestroy));
    // }

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
            if (cell.Type == TileTypes.player) 
            {
                print("gracz oberawał");
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
                     print("monster died and should leave his bones on this cell");
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

    internal MonsterData GetMonsterData(int MonsterID = -1)
    {
        if(MonsterID == -1)
        {
            var randomIndex = UnityEngine.Random.Range(0,MonsterVariants.Count);
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