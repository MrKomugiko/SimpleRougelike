using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    public Vector2Int StartPosition;
    public Vector2Int TargetPosition;
    public List<Node> FinalPath = new List<Node>();
    internal CellScript _cellData;
    Heap<Node> _open;
    HashSet<Node> _closed = new HashSet<Node>();

    [ContextMenu("FindPath")] public void FindPath(CellScript _targetCell = null)
    {
        if(_targetCell == null)
            _targetCell = GameManager.Player_CELL; // default: celem jest gracz
            
        StartPosition = _cellData.CurrentPosition;;
        TargetPosition = _targetCell.CurrentPosition;

        _open = new Heap<Node>(NodeGrid.MapSize);
        _closed.Clear();
        Node start_Node = NodeGrid.MAPDATA[this.StartPosition.x, this.StartPosition.y];
        Node finish_Node = NodeGrid.MAPDATA[this.TargetPosition.x, this.TargetPosition.y];
       // print(start_Node.Coordination + " ===> " + finish_Node.Coordination);

        _open.Add(start_Node);

        while (_open.Count > 0)
        {
            Node current_node = _open.RemoveFirst();
            _closed.Add(current_node);

            if (current_node == finish_Node)
            {
                RetracePath(start_Node, current_node);
                return;
            }

            foreach (Node neighbour in NodeGrid.GetNeighbours(current_node))
            {
                if (_closed.Contains(neighbour) || !neighbour.walkable)
                    continue;

                int newCostToNeighbour = current_node.gCost + GetDistance(current_node, neighbour);

                if (newCostToNeighbour < neighbour.gCost || !_open.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, finish_Node);
                    neighbour.parent = current_node;

                    if (!_open.Contains(neighbour))
                    {
                        _open.Add(neighbour);
                    }
                }
            }
        }
        //print("brak trasy");
        FinalPath.Clear();
    }
    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        this.FinalPath = path;

        for (int i = 0; i < FinalPath.Count - 1; i++)
        {
            Node cell = FinalPath[i];
            //Debug.DrawLine(((Vector3Int)FinalPath[i].Coordination), ((Vector3Int)FinalPath[i+1].Coordination), Color.yellow,30f);
            //print($"[{cell.Coordination}] -->> ");
            // GridManager.CellGridTable[cell.Coordination]._cellImage.color = color;
        }
    }
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.Coordination.x - nodeB.Coordination.x);
        int dstY = Mathf.Abs(nodeA.Coordination.y - nodeB.Coordination.y);

        int output = (dstX * dstX) + (dstY + dstY);
        //print(output);
        return output;
    }
    private void OnDrawGizmos(){
        
        if(FinalPath.Count == 0) return;
        Gizmos.color = Color.red;

    // monster
        Gizmos.DrawCube((GridManager.CellGridTable[StartPosition].transform.position+new Vector3(.33f,.33f,0)),new Vector3(0.1f,0.1f,0.1f));
    // link from monster to 1st path
        Gizmos.DrawLine((GridManager.CellGridTable[StartPosition].transform.position+new Vector3(.33f,.33f,0)),(GridManager.CellGridTable[FinalPath[0].Coordination].transform.position+new Vector3(.33f,.33f,0)));
   
    // first path element
        Gizmos.DrawCube((GridManager.CellGridTable[FinalPath[0].Coordination].transform.position+new Vector3(.33f,.33f,0)),new Vector3(0.1f,0.1f,0.1f));

        for (int i = 1; i < FinalPath.Count; i++)
        {   
            // rest of elements of path to target
            Node point_A = FinalPath[i-1];
            Node point_B = FinalPath[i];
       

            Gizmos.DrawCube((GridManager.CellGridTable[point_A.Coordination].transform.position+new Vector3(.33f,.33f,0)),new Vector3(0.1f,0.1f,0.1f));
            Gizmos.DrawLine((GridManager.CellGridTable[point_A.Coordination].transform.position+new Vector3(.33f,.33f,0)),(GridManager.CellGridTable[point_B.Coordination].transform.position+new Vector3(.33f,.33f,0)));
        }
    }
}