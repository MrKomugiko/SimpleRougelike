using System.Linq;
using UnityEngine;

public class Monster_Cell : ISpecialTile, ITaskable
{
    #region core
    public string Name { get; set; }
    public TileTypes Type { get; private set; } = TileTypes.monster;
    public string Effect_Url { get; set; }
    public string Icon_Url { get; set; }
    public CellScript ParentCell { get; set; }
    #endregion
    #region Monster-specific
    internal Pathfinding _pathfinder;
    #endregion
    #region optional
    public bool Active { get; set; } = true;
    public bool IsReadyToUse => true;

    #endregion
    public Monster_Cell(CellScript parent,string name,string icon_Url, string effect_Url,  Pathfinding pathfinder = null)
    {
        Name = name;
        Effect_Url = effect_Url;
        Icon_Url = icon_Url;
        Active = true;
        Type = TileTypes.monster;
        _pathfinder = pathfinder;

        Debug.Log("monster created");
        ParentCell = parent;
    }
    public void OnClick_MakeAction()
    {
       
        // if null => load pathfindingScript
        if(_pathfinder == null)
        {
            Debug.Log("proba załadowania pathfindera z obiektu monster");
            ParentCell.Trash.Where(t=>t.name == (Icon_Url+"(Clone)")).FirstOrDefault().TryGetComponent<Pathfinding>(out _pathfinder);
            if(_pathfinder == null)
            {
                Debug.LogError("nieudane ładowanie pathfindera");
                return;
            }
            _pathfinder._cellData = ParentCell;
        }      
       
       if(TaskManager.TaskManagerIsOn == false)
        {
            // Moving toward player
         
            Move(GameManager.Player_CELL);
            Debug.LogWarning("player click on monster -> start interaction with monster");
        }
        else
        {
            AddActionToQUEUE();
        }
    }

    public void Attack(CellScript _target)
    {
        Debug.Log($"Monster zaatakował {_target.name}");
    }
    
    public void Move(CellScript _targetCell)
    {
        NodeGrid.UpdateMapObstacleData();
        Debug.Log($"Monster wykonuje krok w celu komórki {_targetCell.name}");
        _pathfinder.FindPath(_targetCell);
        GridManager.SwapTiles(ParentCell,_pathfinder.FinalPath[0].Coordination);
    }

    public void AddActionToQUEUE()
    {
        var position = ParentCell.CurrentPosition;
        TaskManager.AddToActionQueue(
            $"Attack Monster on position:[{position.x};{position.y}]",
            () =>
            {
                if (GridManager.CellGridTable[position].SpecialTile != null)
                {
                    if(GridManager.CellGridTable[position].SpecialTile is Monster_Cell)
                    {
                        return (true, "succes");
                    }
                }    
                
                return (false, "we wskazanym miejscu nie znajduje sie monster którego można zaatakować");
            }
        );
    }
}
