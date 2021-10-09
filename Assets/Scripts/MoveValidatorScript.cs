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
        Debug.Log("spawn marks on grid");
        DestroyAllGridObjects();
        foreach(var cell in GridManager.CellGridTable.Values)
        {
            var mark = Instantiate(Mark,cell.transform);
            All_GridIndicators.Add(cell.CurrentPosition,mark.GetComponent<SpriteRenderer>());
            All_GridIndicators[cell.CurrentPosition].color = Color.clear;
        }
    }
    public void HighlightValidMoveGrid(bool _restrictedByStaminavalue){
        validMovePosiitonsCounter = 0;
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
                    validMovePosiitonsCounter++; 
                }
            }
            else if (ParentPathfinder.FinalPath.Count <= (_restrictedByStaminavalue?PlayerManager.instance.CurrentStamina:PlayerManager.instance.MoveRange))
            {
                if(Move_Indicators.ContainsKey(cell.CurrentPosition) == false)
                {
                    Move_Indicators.Add(cell.CurrentPosition,All_GridIndicators[cell.CurrentPosition]);
                }   
                Move_Indicators[cell.CurrentPosition].color = new Color32(128,255,0,50);    
                validMovePosiitonsCounter++;
            }
        }

        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = true;
        }
    }
    public int HighlightValidAttackGridGlobal(int? overteDistanceCheck = null)
    {
        Debug.Log("HighlightValidAttackGridGlobal");
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
            checkedMonster.Value.IsWalkable = true;
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
        monsterList.ForEach(m=>m.Value.IsWalkable = false);
        return _monstersInRange;   
    }
    public (int count,List<CellScript> targets) HighlightValidAttackGridNearPlayer(List<Vector2Int> vectorsforpossibleattackCheck)
    {
        Debug.Log("HighlightValidAttackGridNearPlayer");
        var monsterList = new List<CellScript>();
        var PlayerPosition = PlayerManager.instance._playerCell.ParentCell.CurrentPosition;

        foreach(var vector in vectorsforpossibleattackCheck)
        {
            if(GridManager.CellGridTable.ContainsKey(PlayerPosition+vector) == false) continue; 
            if(GridManager.CellGridTable[PlayerPosition+vector].SpecialTile is Monster_Cell)
            {
                monsterList.Add(GridManager.CellGridTable[PlayerPosition+vector]);
            }
        }
        int _validTargets = monsterList.Count();

        foreach(var monster in monsterList)
        {
            if(monster == null) continue;
            if(Attack_Indicators.ContainsKey(monster.CurrentPosition)) continue;

            Attack_Indicators.Add(monster.CurrentPosition, All_GridIndicators[monster.CurrentPosition]);
            Attack_Indicators.Last().Value.gameObject.SetActive(true);
            Attack_Indicators.Last().Value.color = Color.red; 
        }
        return (_validTargets,monsterList);   
    }

    public (int count,List<CellScript> targets) HighlightValidAttackGridStraightProjectile(
                                                    List<Vector2Int> vectorsforpossibleattackCheck, 
                                                                bool isPenetrationEnabled = false, 
                                                                bool isFlyAboweWalls = false)
    {
        Debug.Log("HighlightValidAttackGridStraightProjectile");
        var monsterList = new List<CellScript>();
        var PlayerPosition = PlayerManager.instance._playerCell.ParentCell.CurrentPosition;

        List<Vector2> directionsBloced = new List<Vector2>();
        foreach(var vector in vectorsforpossibleattackCheck)
        {
            //TODO: !! to dziala tak dlugo jak podane vektorki są posortowane od odleglosci od centrum (gracza) rosnąco
            var checkingPosition = PlayerPosition+vector;
            if(GridManager.CellGridTable.ContainsKey(checkingPosition) == false) continue; 

            Vector2Int checkingDirection = Vector2Int.zero;

            if(vector.x > 0)     
                checkingDirection.x = 1;    
            else if( vector.x < 0)
                checkingDirection.x = -1;
            
            if(vector.y > 0)     
                checkingDirection.y = 1;    
            else if( vector.y < 0)
                checkingDirection.y = -1;

            if(directionsBloced.Contains(checkingDirection)) continue;

            if(GridManager.CellGridTable[checkingPosition].Type == TileTypes.wall)
            {
                if(isFlyAboweWalls == false)
                {   
                    directionsBloced.Add(checkingDirection);
                    continue;                    
                }
                continue;  
            }

            if(GridManager.CellGridTable[checkingPosition].SpecialTile is Monster_Cell)
            {
                if(isPenetrationEnabled == false)  
                {
                    monsterList.Add(GridManager.CellGridTable[checkingPosition]);
                    directionsBloced.Add(checkingDirection);
                    continue;
                }
                monsterList.Add(GridManager.CellGridTable[checkingPosition]);
                continue;
            }
        }
        int _validTargets = monsterList.Count();

        foreach(var monster in monsterList)
        {
            if(Attack_Indicators.ContainsKey(monster.CurrentPosition)) continue;

            Attack_Indicators.Add(monster.CurrentPosition, All_GridIndicators[monster.CurrentPosition]);
            Attack_Indicators.Last().Value.gameObject.SetActive(true);
            Attack_Indicators.Last().Value.color = Color.red; 
        }
        return (_validTargets,monsterList);   
    }

    public void HideAllGrid()
    {
        Debug.Log("hide all grid");
        foreach (var grid in All_GridIndicators.Values)
        {
            grid.gameObject.SetActive(false);
        }
    }
    public void HideMoveGrid()
    {
         Debug.Log("hide move grid");
        foreach (var grid in Move_Indicators.Values)
        {
            if(grid == null) continue;
            
            grid.gameObject.SetActive(false);
        }
    }
    public void HideAttackGrid()
    {
        Debug.Log("hide attack grid");
        foreach (var grid in Attack_Indicators.Values)
        {
            grid.gameObject.SetActive(false);
        }
    }
    public void DestroyAllGridObjects()
    {
        Debug.Log("destroy all grid objects");
        foreach (var grid in All_GridIndicators.Values)
        {
            if(grid == null) continue;
            Destroy(grid.gameObject);
        }
        All_GridIndicators.Clear();
        Attack_Indicators.Clear();
        Move_Indicators.Clear();
    }

   [Obsolete] public void ShowValidAttackAndMoveCombinedGrid(bool staminaRestriction)
    {
        Debug.Log("ShowValidAttackAndMoveCombinedGrid");
        DestroyAllGridObjects();
        SpawnMarksOnGrid();
        
        HighlightValidMoveGrid(_restrictedByStaminavalue: staminaRestriction);

        int intStaminaValue = Mathf.FloorToInt(PlayerManager.instance.CurrentStamina);
        HighlightValidAttackGridGlobal(intStaminaValue);
    }
}
