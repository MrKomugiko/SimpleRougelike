using System;
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
        //Debug.LogError("spawn siatki");
    }
    public void HighlightValidMoveGrid(bool _restrictedByStaminavalue){

        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = false;
        }

        NodeGrid.UpdateMapObstacleData();

        foreach(var cell in GridManager.CellGridTable.Values)
        {
            ParentPathfinder.FindPath(cell);
            if(ParentPathfinder.FinalPath.Count == 0 && ParentPathfinder.FinalPath.Count < (_restrictedByStaminavalue?PlayerManager.instance.CurrentStamina:PlayerManager.instance.MoveRange))
            {
                if(cell.Type == TileTypes.player)
                {
                    if(Move_Indicators.ContainsKey(cell.CurrentPosition) == false)
                    {
                        Move_Indicators.Add(cell.CurrentPosition,All_GridIndicators[cell.CurrentPosition]);
                    }                        
                    Move_Indicators[cell.CurrentPosition].color = new Color32(128,255,0,50);    
                }
                
            }
            else if (ParentPathfinder.FinalPath.Count <= (_restrictedByStaminavalue?PlayerManager.instance.CurrentStamina:PlayerManager.instance.MoveRange))
            {
                if(Move_Indicators.ContainsKey(cell.CurrentPosition) == false)
                {
                    Move_Indicators.Add(cell.CurrentPosition,All_GridIndicators[cell.CurrentPosition]);
                }   
                Move_Indicators[cell.CurrentPosition].color = new Color32(128,255,0,50);    
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
            checkedMonster.Value.IsWalkable = true;
            
            NodeGrid.UpdateMapObstacleData();
            ParentPathfinder.FindPath(checkedMonster.Value);
            if(ParentPathfinder.FinalPath.Count >0 && ParentPathfinder.FinalPath.Count<=(overteDistanceCheck==null?PlayerManager.instance.AttackRange:(int)overteDistanceCheck))
            {
                _monstersInRange++;
            Debug.LogWarning("ZAZNACZONO monster z pozycji"+checkedMonster.Key+" dystans do gracza = "+ParentPathfinder.FinalPath.Count);
                Attack_Indicators[checkedMonster.Key].color = Color.red; 
                Attack_Indicators[checkedMonster.Key].gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("NIE ZAZNACZONO monster z pozycji"+checkedMonster.Key+" dystans do gracza = "+ParentPathfinder.FinalPath.Count);
                Attack_Indicators[checkedMonster.Key].gameObject.SetActive(false);          
            }        
        }
        monsterList.ForEach(m=>m.Value.IsWalkable = false);
        Debug.LogError("_monstersinrange = "+_monstersInRange);
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
            if(grid == null) continue;
            Destroy(grid.gameObject);
        }
        All_GridIndicators.Clear();
        Attack_Indicators.Clear();
        Move_Indicators.Clear();
    }

    [ContextMenu("Show combined grid")]
    public void ShowValidAttackAndMoveCombinedGrid(bool staminaRestriction)
    {
        DestroyAllGridObjects();
        SpawnMarksOnGrid();
        
        // POPRAWKA NA AKTUALNIE POSIADANE ZASOBY (STAMINE)
        HighlightValidMoveGrid(_restrictedByStaminavalue: staminaRestriction);

        int intStaminaValue = Mathf.FloorToInt(PlayerManager.instance.CurrentStamina);
        HighlightValidAttackGrid(intStaminaValue);
    }
}
