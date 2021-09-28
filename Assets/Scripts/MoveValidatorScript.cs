using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveValidatorScript : MonoBehaviour
{
    [SerializeField] GameObject Mark;
    [SerializeField] Dictionary<Vector2Int,SpriteRenderer> All_GridIndicators = new Dictionary<Vector2Int,SpriteRenderer>();

    [SerializeField] public Dictionary<Vector2Int,SpriteRenderer> Move_Indicators = new Dictionary<Vector2Int,SpriteRenderer>();
    [SerializeField] public Dictionary<Vector2Int,SpriteRenderer> Attack_Indicators = new Dictionary<Vector2Int,SpriteRenderer>();

    public Pathfinding ParentPathfinder;
    public int validMovePosiitonsCounter = 0;
    public void SpawnMarksOnGrid()
    {
        DestroyAllGridObjects();
        foreach(var cell in GridManager.CellGridTable.Values)
        {
            var mark = Instantiate(Mark,cell.transform);
            All_GridIndicators.Add(cell.CurrentPosition,mark.GetComponent<SpriteRenderer>());
            All_GridIndicators[cell.CurrentPosition].color = Color.clear;
        }
        Debug.LogError("spawn siatki");
    }
    public void HighlightValidMoveGrid(){
       // SpawnMarksOnGrid();
        Debug.LogError("highlight move grid");

        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = false;
        }

        NodeGrid.UpdateMapObstacleData();

        foreach(var cell in GridManager.CellGridTable.Values)
        {
            ParentPathfinder.FindPath(cell);
            if(ParentPathfinder.FinalPath.Count == 0 && ParentPathfinder.FinalPath.Count < PlayerManager.instance.MoveRange)
            {
                if(cell.Type == TileTypes.player)
                {
                    if(Move_Indicators.ContainsKey(cell.CurrentPosition) == false)
                    {
                        Move_Indicators.Add(cell.CurrentPosition,All_GridIndicators[cell.CurrentPosition]);
                    }                        
                    Move_Indicators[cell.CurrentPosition].color = new Color32(128,255,0,200);    
                }
                
            }
            else if (ParentPathfinder.FinalPath.Count <= 2)
            {
                if(Move_Indicators.ContainsKey(cell.CurrentPosition) == false)
                {
                    Move_Indicators.Add(cell.CurrentPosition,All_GridIndicators[cell.CurrentPosition]);
                }
                Move_Indicators[cell.CurrentPosition].color = new Color32(128,255,0,200);    
            }
        }

        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = true;
        }
    
    }
    public int HighlightValidAttackGrid(int? overteDistanceCheck = null)
    {
        int _monstersInRange = 0;
        var monsterList = GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster).ToList();
        foreach(var monster in monsterList)
        {
            if(Attack_Indicators.ContainsKey(monster.Key))
                continue;

            Attack_Indicators.Add(monster.Key, All_GridIndicators[monster.Key]);
        }
        foreach(var checkedMonster in monsterList)
        {
            monsterList.ForEach(m=>m.Value.IsWalkable = false);
            monsterList.Where(m=>m.Value==checkedMonster.Value).First().Value.IsWalkable = true;
            
            NodeGrid.UpdateMapObstacleData();

            ParentPathfinder.FindPath(checkedMonster.Value);
            if(ParentPathfinder.FinalPath.Count >0 && ParentPathfinder.FinalPath.Count<=(overteDistanceCheck==null?PlayerManager.instance.AttackRange:(int)overteDistanceCheck))
            {
                _monstersInRange++;
                Attack_Indicators[checkedMonster.Key].color = Color.red; 
                Attack_Indicators[checkedMonster.Key].gameObject.SetActive(true);
            }
            else
            {
                Attack_Indicators[checkedMonster.Key].gameObject.SetActive(false);          
            }        
        }

        return _monstersInRange;   
    }

    public void HideAllGrid()
    {
        foreach (var grid in All_GridIndicators.Values)
        {
            grid.gameObject.SetActive(false);
        }
    }
    public void HideMoveGrid()
    {
        foreach (var grid in Move_Indicators.Values)
        {
            if(grid == null) continue;
            
            grid.gameObject.SetActive(false);
        }
    }
    public void HideAttackGrid()
    {
        foreach (var grid in Attack_Indicators.Values)
        {
            grid.gameObject.SetActive(false);
        }
    }
    public void DestroyAllGridObjects()
    {
        foreach (var grid in All_GridIndicators.Values)
        {
            Destroy(grid.gameObject);
        }
        All_GridIndicators.Clear();
        Attack_Indicators.Clear();
        Move_Indicators.Clear();
    }

    [ContextMenu("Show combined grid")]
    public void ShowValidAttackAndMoveCombinedGrid()
    {
        DestroyAllGridObjects();
        SpawnMarksOnGrid();
        HighlightValidMoveGrid();
        
        HighlightValidAttackGrid(PlayerManager.instance.AttackRange+PlayerManager.instance.MoveRange);
    }
}
