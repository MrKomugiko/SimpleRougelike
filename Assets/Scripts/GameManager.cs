using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TurnCounter_TMP;
    [SerializeField] TextMeshProUGUI GoldCounter_TMP;
    [SerializeField] TextMeshProUGUI ExperienceCounter_TMP;
    [SerializeField] private Vector2Int StartingPlayerPosition;
    [SerializeField] GridManager _gridManager;
    public List<GameObject> specialEffectList = new List<GameObject>();
    public static CellScript Player;
    public static GameManager instance;
    private void Awake() {
        if(instance == null)
            instance = this;
        else
            Destroy(this);
    }
    private void Start() {

        PlacePlayerOnGrid();
    }
    private void PlacePlayerOnGrid()
    {
      Player = GridManager.CellGridTable[StartingPlayerPosition];
      Player.Trash.ForEach(t=>Destroy(t.gameObject));
      Player.Trash.Clear();
      Player.AssignType(TileTypes.player);
    }
    
    public void SpawnBomb()
    {
        int x = UnityEngine.Random.Range(0,_gridManager._gridSize.x);
        int y = UnityEngine.Random.Range(0,_gridManager._gridSize.y);

        Vector2Int randomPosition = new Vector2Int(x,y);
        print($"spawn bomby na losową pozycje: {randomPosition}.");

       GridManager.CellGridTable[randomPosition].SpecialTile = 
        new Bomb_Cellcs(
            GridManager.CellGridTable[randomPosition], 
            name: "Mina przeciwpiechotna",
            effect_Url: "bomb_explosion_image",
            icon_Url: "bomb_icon"
        );
    }

    [SerializeField] private GameObject TickCounterPrefab;
    internal GameObject InstantiateTicker(Bomb_Cellcs bomb_Cellcs)
    {
        return Instantiate(TickCounterPrefab, bomb_Cellcs.ParentCell.transform);
    }
    public void Countdown_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
       foreach(var damagedCell in GameManager.DamagedCells)
       {
            if(cellsToDestroy.Contains(damagedCell))
            {
                cellsToDestroy.Remove(damagedCell);
            }
       }
       StartCoroutine(routine_SendToGraveyard(time,cellsToDestroy));
    }
    
    public static List<CellScript> DamagedCells = new List<CellScript>();
    public bool WybuchWTrakcieWykonywania = false;
    private IEnumerator routine_SendToGraveyard(float time, List<CellScript> cellsToDestroy)
    {
        DamagedCells.AddRange(cellsToDestroy);
        yield return new WaitWhile(()=>WybuchWTrakcieWykonywania);
        WybuchWTrakcieWykonywania = true;
        yield return new WaitForSeconds(time);

        foreach(var cell in cellsToDestroy.Where(c=>c != null).ToList())
        { 
            if(cell.Type== TileTypes.player) continue;

            GridManager.SendToGraveyard(cell.CurrentPosition);
            cell.SpecialTile = null;
        }

        cellsToDestroy.Where(c=>c != null).ToList().ForEach(cell => cell.Trash.ForEach(trash => Destroy(trash.gameObject)));
        cellsToDestroy.Where(c=>c != null).ToList().ForEach(cell => {
            cell.SpecialTile = null;
            cell.Trash.Clear();
            }
            );
            cellsToDestroy.ForEach(cell=> DamagedCells.Remove(cell));
        
        WybuchWTrakcieWykonywania = false;
        print("DamagesCells = "+DamagedCells.Count);
        if(DamagedCells.Count == 0)
        {
            _gridManager.FillGaps();
        }
        yield return null;
    }

    private int _currentTurnNumber = 0;
    public void AddTurn()
    {
        _currentTurnNumber = Int32.Parse(TurnCounter_TMP.text);
        TurnCounter_TMP.SetText((CurrentTurnNumber+=1).ToString());
        print("dodanie tury");
    }
    public void AddGold(int value)
    {
        int currentTurnnumber = Int32.Parse(GoldCounter_TMP.text);
        GoldCounter_TMP.SetText((currentTurnnumber+=value).ToString());
    }

    public List<Sprite> SpritesList = new List<Sprite>();
    public List<GameObject> ExtrasPrefabList = new List<GameObject>();

    public int CurrentTurnNumber 
    { 
        get => _currentTurnNumber; 
        set {
             _currentTurnNumber = value; 
            foreach(var tile in GridManager.CellGridTable.Where(cell=>cell.Value.SpecialTile != null).Select(c=>c.Value))
            {
                if(tile.SpecialTile is Bomb_Cellcs) 
                {   
                    if((tile.SpecialTile as Bomb_Cellcs).TickCounter != null)
                    {
                         print("tick");
                        (tile.SpecialTile as Bomb_Cellcs).TickCounter.AddTick(1);
                    }
                }

                if(tile.SpecialTile is IFragile == false) continue;

                if(tile.SpecialTile.IsReadyToUse == true)
                {
                    tile.Trash.ForEach(trash=>{
                        if(trash != null)
                        {
                            var trash_SR = trash.GetComponent<SpriteRenderer>();  
                            if(trash_SR != null)
                            {
                                if(trash_SR.name.Contains("icon"))
                                {
                                    trash_SR.color = Color.red;        
                                }
                            }
                        }
                    });
                    // jakas tam funkja ktora bedzie pokazywać status naładowania obiektu
                }
               
            }
        }
    }

    [ContextMenu("Start simulation")]
    public void StartSimulation()
    {
        StartCoroutine(SimulateGameplay());
    }

private IEnumerator SimulateGameplay()
{
    
    for(;;)
    {
        if(GridManager.CellGridTable.Where(cell=>cell.Value.Type != TileTypes.wall && cell.Value.Type != TileTypes.player).Count()>0)
        {
            foreach(var tile in GridManager.CellGridTable.Where(cell=>cell.Value.Type != TileTypes.wall && cell.Value.Type != TileTypes.player))
            {
                print($"click [{tile.Value.CurrentPosition}]");
                tile.Value._button.onClick.Invoke();
                break;
            }
            yield return new WaitForSeconds(.5f);
        }
        else
        {
            print("no more avaiable moves");
            break;
        }
    }
    
    yield return null;

}
}



public enum TileTypes
{
    undefined = 0,
    player,
    wall,
    grass,
    bomb,
    treasure,
    monster
}
