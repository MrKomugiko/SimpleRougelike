using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bomb_Cellcs : ISpecialTile
{
    public Vector2Int Position { get; set; }
    public TileTypes Type { get; set; }
    public string Name { get; set; }

    public CellScript ParentCell {get;set;}
    public string Effect {get;set;}
    public string Icon {get;set;}

    public Bomb_Cellcs(CellScript parent, Vector2Int position, TileTypes type, string name, string effect, string icon )
    {
        this.ParentCell = parent;
        this.Position = position;
        this.Type = type;
        this.Name = name;

        this.Effect = effect;
        this.Icon = icon;

        Debug.Log("pomyslnie utworzono Bombę, dla pola skojarzonego z pozycją "+ ParentCell.CurrentPosition);

        
    }

    public List<CellScript> CellsToDestroy = new List<CellScript>();
    public void MakeAction()
    {
        Debug.Log("EXPLODE !");
        
        if(GridManager.CellGridTable.ContainsKey(this.Position))
            CellsToDestroy.Add(GridManager.CellGridTable[this.Position] );

        if(GridManager.CellGridTable.ContainsKey(this.Position + Vector2Int.up))
            CellsToDestroy.Add(GridManager.CellGridTable[this.Position + Vector2Int.up]);

        if(GridManager.CellGridTable.ContainsKey(this.Position + Vector2Int.down))
        CellsToDestroy.Add(GridManager.CellGridTable[this.Position + Vector2Int.down]);

         if(GridManager.CellGridTable.ContainsKey(this.Position + Vector2Int.left))
        CellsToDestroy.Add(GridManager.CellGridTable[this.Position + Vector2Int.left]);

         if(GridManager.CellGridTable.ContainsKey(this.Position + Vector2Int.right))
        CellsToDestroy.Add(GridManager.CellGridTable[this.Position + Vector2Int.right]);

        foreach(var cell in CellsToDestroy)
        {   
            Debug.Log(cell.CurrentPosition);    
            Debug.Log(Effect);

            cell.AddEffectImage(imageUrl: Effect);

            // TODO: wysyłanie na cmentarz tilesów 
            // TODO: kasowanie obiektow z list Trash w tilesach
            // TODO: opóźnić sekwencje, zeb ybyło widać eksplozje a potem puste pole, nastepnie fill nowymi obiektami
            // GridManager.SendToGraveyard(cell.CurrentPosition);
        }
    }

    public void SpawnOnMap()
    {
        throw new System.NotImplementedException();
    }
}
