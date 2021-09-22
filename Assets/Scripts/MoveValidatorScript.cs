using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveValidatorScript : MonoBehaviour
{
    [SerializeField] GameObject Mark;
    [SerializeField] List<SpriteRenderer> GridIndicators = new List<SpriteRenderer>();
    public Pathfinding ParentPathfinder;
    public int validMovePosiitonsCounter = 0;
    public void SpawnMarksOnGrid()
    {
        
        GridIndicators.ForEach(g=>{if(g!=null){Destroy(g.gameObject);}});
        GridIndicators.Clear();
        foreach(var cell in GridManager.CellGridTable.Values)
        {
            var mark = Instantiate(Mark,cell.transform);
            GridIndicators.Add(mark.GetComponent<SpriteRenderer>());
        }
        
    }
    [ContextMenu("showGrid")]
    public void ShowValidMoveGrid(){
        validMovePosiitonsCounter = 0;
        SpawnMarksOnGrid();

        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = false;
        }

        NodeGrid.UpdateMapObstacleData();

        int index = 0;
        foreach(var cell in GridManager.CellGridTable.Values)
        {
            ParentPathfinder.FindPath(cell);

        // print(ParentPathfinder.FinalPath.Count);

            if(ParentPathfinder.FinalPath.Count == 0 && ParentPathfinder.FinalPath.Count < PlayerManager.instance.MoveRange)
            {
                if(cell.Type == TileTypes.player)
                {
                    validMovePosiitonsCounter ++;
                   // Debug.Log(validMovePosiitonsCounter);

                    GridIndicators[index].color = Color.green;    
                }
                else
                {
                    GridIndicators[index].gameObject.SetActive(false);
                }

            }
            else if(ParentPathfinder.FinalPath.Count > 2)
            {
                GridIndicators[index].gameObject.SetActive(false);
            }
            else
            {
                validMovePosiitonsCounter ++;
//                Debug.Log(validMovePosiitonsCounter);
            }
            index++;
        }

        foreach(var monster in GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster))
        {
            monster.Value.IsWalkable = true;
        }
    
    }

    public void HideGrid()
    {
        GridIndicators.ForEach(g=>g.gameObject.SetActive(false));
    }

    [ContextMenu("showAttackGrid")]
    public int ShowValidAttackGrid()
    {
        int _monstersInRange = 0;
        GridIndicators.ForEach(g=>Destroy(g.gameObject));
        GridIndicators.Clear();
        var monsterList = GridManager.CellGridTable.Where(c=>c.Value.Type == TileTypes.monster).ToList();
        foreach(var monster in monsterList)
        {
            var mark = Instantiate(Mark,monster.Value.transform);
            GridIndicators.Add(mark.GetComponent<SpriteRenderer>());
        }

        int index = 0;
        foreach(var checkedMonster in monsterList)
        {
            monsterList.ForEach(m=>m.Value.IsWalkable = false);
            monsterList.Where(m=>m.Value==checkedMonster.Value).First().Value.IsWalkable = true;
            
            NodeGrid.UpdateMapObstacleData();

            ParentPathfinder.FindPath(checkedMonster.Value);
//            print(ParentPathfinder.FinalPath.Count);
            if(ParentPathfinder.FinalPath.Count >0 && ParentPathfinder.FinalPath.Count<=PlayerManager.instance.AttackRange)
            {
            //    print("in attack range");
                _monstersInRange++;
                GridIndicators[index].color = Color.red; 
                GridIndicators[index].gameObject.SetActive(true);
            }
            else
            {
              //  print("too far");
                GridIndicators[index].gameObject.SetActive(false);          
            }        
            index++;
        }

        return _monstersInRange;   
    }
}
