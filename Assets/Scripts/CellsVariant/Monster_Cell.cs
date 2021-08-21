using System.Linq;
using UnityEngine;

public class Monster_Cell : IEnemy, ITaskable
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
    public int ExperiencePoints {get;set;} = 10;
    public int HealthPoints {get;set;} = 2;
    public bool IsAlive => HealthPoints > 0;
    //TODO: dodac to do kontruktora potem
    public int lootID {get; private set;} = 1;
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
    public void TakeDamage(int damage, string source)
    {
        Debug.Log($"Monster HP decerase from [{HealthPoints}] to [{HealthPoints-damage}] by <{source}>");
        HealthPoints-=damage;
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
        GridManager.TrySwapTiles(ParentCell,_pathfinder.FinalPath[0].Coordination);
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

    public void ChangeToTreasureObject(string corpse_Url, object lootID)
    {
        
      ParentCell.SpecialTile = new Treasure_Cell(ParentCell,"zwłoki slime'a",corpse_Url,50);
       //1. remove Trash
       //2. change type to Treasure
       //3. set monstercorpse as treasure icon
       //4. assign LootID related reward to this object

       Debug.LogWarning("monster nie żyje, zmienia sie w wartościowy sosik kości ;d");
    }
}
