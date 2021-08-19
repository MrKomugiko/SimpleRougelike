using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Node : IHeapItem<Node> {
	public Vector2Int Coordination;
    public int gridX;
    public int gridY;
    public bool walkable = true;
	public int gCost; 	// dystans od startu do tego punktu
	public int hCost;	// przybliżona odległość od tego punktu do mety a^2 + b^2
	public Node parent;
    int heapIndex;
    public Node(int _x, int _y, bool _isWalkable)
    {
        this.Coordination = new Vector2Int(_x, _y);
        this.gridX = _x;
        this.gridY = _y;
        this.walkable = _isWalkable;
    }

    public int fCost => gCost + hCost;	// całkowity 'koszt' podróży

    public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}