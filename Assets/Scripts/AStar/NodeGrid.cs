using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    static public Vector2Int GridSize;
    static public Node[,] MAPDATA;
    public static int MapSize => GridSize.x*GridSize.y;

    private void Start() 
    {
        GridSize = GridManager.instance._gridSize;
        MAPDATA = new Node[GridSize.x,GridSize.y];
        // create empty grid

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
               MAPDATA[x,y] = new Node(x,y,_isWalkable: true);                
            }
        }
      //  print("liczba dostępnych nodów w MAPDATA =  "+MAPDATA.Length);

    }

    public static void UpdateMapObstacleData()
    {
       // print("odświeżono nodeGrid'a o aktualną pozycje ścian na mapie");
        // set all to true
        foreach(var node in MAPDATA)
        {
            node.walkable = true;
        }

        foreach(var cell in GridManager.CellGridTable)
        {
            if(! cell.Value.IsWalkable)
                MAPDATA[cell.Key.x,cell.Key.y].walkable = false;
        }
    }
    public static List<Node> GetNeighbours(Node node)
    {
        List<Node> ListOfNeighbours = new List<Node>();
        Vector2Int UP = Vector2Int.zero;
        Vector2Int DOWN = Vector2Int.zero;
        Vector2Int LEFT = Vector2Int.zero;
        Vector2Int RIGHT = Vector2Int.zero;

        UP = node.Coordination + Vector2Int.up;
        if(! ((UP.x <0 || UP.x > GridSize.x-1) || (UP.y <0 || UP.y > GridSize.y-1)))
        {
            ListOfNeighbours.Add(MAPDATA[UP.x,UP.y]);
        }
        
        DOWN = node.Coordination + Vector2Int.down;
        if(! ((DOWN.x <0 || DOWN.x > GridSize.x-1) || (DOWN.y <0 || DOWN.y > GridSize.y-1)))
        {
            ListOfNeighbours.Add(MAPDATA[DOWN.x,DOWN.y]);
        }

        LEFT = node.Coordination + Vector2Int.left;
        if(! ((LEFT.x <0 || LEFT.x > GridSize.x-1) || (LEFT.y <0 || LEFT.y > GridSize.y-1)))
        {
            ListOfNeighbours.Add(MAPDATA[LEFT.x,LEFT.y]);
        }

        RIGHT = node.Coordination + Vector2Int.right;
        if(! ((RIGHT.x <0 || RIGHT.x > GridSize.x-1) || (RIGHT.y <0 || RIGHT.y > GridSize.y-1)))
        {
            ListOfNeighbours.Add(MAPDATA[RIGHT.x,RIGHT.y]);
        }
      
        return ListOfNeighbours;
    }
}
