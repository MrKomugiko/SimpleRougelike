using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Monster_Cell : ICreature, ITaskable
{
    #region core
    public CellScript ParentCell { get; set; }
    public TileTypes Type { get; private set; } = TileTypes.monster;
    public string Name { get; set; }
    public string Icon_Url { get; set; }
    #endregion

    #region Monster-specific
    internal Pathfinding _pathfinder;
    public int ExperiencePoints { get; set; } = 10;
    public int HealthPoints 
    { 
        get => _healthPoints; 
        set 
        {
            _healthPoints = value;
        }
    }
    public bool IsAlive
    {
        get
        {
            if(HealthPoints > 0)
                return true;
            else
            {
                ChangeIntoTreasureObject(corpse_Url: "monster_bones", lootID: lootID);
                return  false;
            }
        }
    }

    //TODO: dodac to do kontruktora potem
    public int lootID { get; private set; } = 1;
    #endregion




    public Monster_Cell(CellScript parent, string name, string icon_Url, Pathfinding pathfinder = null)
    {
        Name = name;
        Icon_Url = icon_Url;
        Type = TileTypes.monster;
        _pathfinder = pathfinder;


        Debug.Log("monster created");
        ParentCell = parent;


    }
    public void ConfigurePathfinderComponent()
    {

        if (_pathfinder == null)
        {
            Debug.Log("proba załadowania pathfindera z obiektu monster");
            ParentCell.Trash.Where(t => t.name == (Icon_Url + "(Clone)")).FirstOrDefault().TryGetComponent<Pathfinding>(out _pathfinder);
            if (_pathfinder == null)
            {
                Debug.LogError("nieudane ładowanie pathfindera");
                return;
            }
            _pathfinder._cellData = ParentCell;
        }
        else
            return;

    }
    public void OnClick_MakeAction()
    {

        if (TaskManager.TaskManagerIsOn == true)
        {
            AddActionToQUEUE();
            return;
        }

        Debug.LogWarning("player click on monster -> start interaction with monster");
        TakeDamage(1, "Attacked by player");
        GameManager.instance.AddTurn();
    }
    GameObject border;
    private int _healthPoints = 2;

    public void TakeDamage(int damage, string source)
    {
        HealthPoints -= damage;
     
        if(IsAlive)
            Debug.Log($"Monster HP decerase from [{HealthPoints + damage}] to [{HealthPoints}] by <{source}>");
        else
            Debug.Log("monster died and left bones");
    }
    public bool TryAttack(CellScript _target)
    {
        if (Vector2Int.Distance(ParentCell.CurrentPosition, _target.CurrentPosition) > 1.1f)
        {
            return false;
        }

        Debug.Log($"Monster zaatakował {_target.name}");
        //TODO: rozpisać to , aktualnie na sztywno -10hp
        int currentHP = Int32.Parse((GameManager.instance.HealthCounter_TMP.text.Replace("%", "")));
        currentHP -= 10;
        GameManager.instance.HealthCounter_TMP.SetText(currentHP + " %");
        return true;

    }
    public bool TryMove(CellScript _targetCell)
    {
        NodeGrid.UpdateMapObstacleData();
        _pathfinder.FindPath(_targetCell);
        if (_pathfinder.FinalPath.Count > 1)
        {
            Debug.Log($"Monster wykonuje krok w strone celu [ komórki {_targetCell.name} ]");
            GridManager.TrySwapTiles(ParentCell, _pathfinder.FinalPath[0].Coordination);
            return true;
        }
        return false;

    }
    public void AddActionToQUEUE()
    {

        var position = ParentCell.CurrentPosition;
        TaskManager.AddToActionQueue(
            $"Attack Monster on position:[{position.x};{position.y}]",
            () =>
            {
                if (Vector2Int.Distance(GameManager.Player_CELL.CurrentPosition, ParentCell.CurrentPosition) > 1.1f)
                {
                    return (false, "Monsterek jest poza zasięgiem 1 pola");
                }

                if (GridManager.CellGridTable[position].SpecialTile != null)
                {
                    if (GridManager.CellGridTable[position].SpecialTile is Monster_Cell)
                    {
                        // TODO: zró coś po kliknięciu na monsterka
                        Debug.Log("click on monster");
                        return (true, "succes");
                    }
                }

                return (false, "we wskazanym miejscu nie znajduje sie monster którego można zaatakować");
            }
        );
    }
    public void ChangeIntoTreasureObject(string corpse_Url, object lootID)
    {

        ParentCell.SpecialTile = new Treasure_Cell(ParentCell, "zwłoki slime'a", corpse_Url, 50);
        //1. remove Trash
        //2. change type to Treasure
        //3. set monstercorpse as treasure icon
        //4. assign LootID related reward to this object
        if (border != null)
        {
            border.GetComponent<Image>().color = Color.yellow;
            GameObject.Destroy(border, .5f);
            border = null;
        }

        Debug.LogWarning("monster nie żyje, zmienia sie w wartościowy sosik kości ;d");
    }
    public void ShowBorder()
    {
        if (border != null) return;

        border = GameObject.Instantiate(GameManager.instance.SelectionBorderPrefab, ParentCell.transform);
        border.GetComponent<Image>().color = Color.red;
        GameObject.Destroy(border, 0.5f);
    }

    public void HideBorder()
    {
        if (border == null) return;

        border.GetComponent<Image>().color = Color.green;
        GameObject.Destroy(border, 0.5f);
    }
}
