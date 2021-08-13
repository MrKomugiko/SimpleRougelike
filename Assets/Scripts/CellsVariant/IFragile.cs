using UnityEngine;

internal interface IFragile
{
    void DetonateOnMove(Vector2Int _currentPosition, Vector2Int direction);
}