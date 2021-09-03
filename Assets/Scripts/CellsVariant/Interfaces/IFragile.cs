using UnityEngine;

internal interface IFragile : IUsable
{
    void ActionOnMove(Vector2Int _currentPosition, Vector2Int direction);
}