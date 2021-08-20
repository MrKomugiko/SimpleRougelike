using System.Linq;
using UnityEngine;

public class Monster_Cell : ISpecialTile
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
    public void MakeAction()
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
        }
        // Moving toward player
        Debug.LogWarning("Monster wykonuje akcje");
    }
}
