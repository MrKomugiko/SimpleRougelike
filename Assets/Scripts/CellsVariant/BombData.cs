using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName="New Treasure",menuName="GameData/Bomb")]
public class BombData : ScriptableObject
{
    public int ID;
    public string BombName; 
    public GameObject Icon_Sprite;
    public GameObject Exploasion_Sprite;
    public TileTypes Type = TileTypes.treasure;

    public ExplosionPatterns ExplosionPattern = ExplosionPatterns.SimpleCross;
    public Directions Direction = Directions.Default;

    public bool RestrictedByTimer = true;
    public int TicksToTurnOn = 5;
    public int Damage = 1;
    public int Size = 1;

    public bool IsWalkable = true;

    public List<Vector2Int> GetExplosionVectors(ExplosionPatterns _pattern = ExplosionPatterns.SimpleCross)
    {
        var VectorArea = new List<Vector2Int>(); 

//        Debug.Log($"generowanie obszaru wybuchu dla {_pattern.ToString()} o zasięgu {Size} ");
        switch(_pattern)
        {
            case ExplosionPatterns.SimpleCross:
                VectorArea.Add( Vector2Int.zero);
                for(int i = 1; i<=Size; i++)
                {    
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.up * i) );
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.right * i) );
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.down * i) );
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.left * i) );
                }
               break;

            case ExplosionPatterns.Square:
                for(int x=-1*(Size/2); x <= 1*(Size/2); x++ )
                {
                    for(int y=-1*(Size/2); y <= 1*(Size/2); y++ )
                    {
                       VectorArea.Add(new Vector2Int(x,y)); 
                    }
                }
              break;

            case ExplosionPatterns.Vertical:
                VectorArea.Add( Vector2Int.zero);
                for(int i = 0; i<Size/2; i++)
                {    
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.up * i) );
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.down * i) );

                }
                break;

            case ExplosionPatterns.Horizontal:
                VectorArea.Add( Vector2Int.zero);
                for(int i = 0; i<Size/2; i++)
                {    
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.right * i) );
                    VectorArea.Add( Vector2Int.zero + (Vector2Int.left * i) );
                }
                break;
        }

     //   Debug.Log("zawiera: "+VectorArea.Count());
        return VectorArea.Distinct<Vector2Int>().ToList();
    }
}
public enum ExplosionPatterns
{
    SimpleCross,
    /*              -   X   -
                    X   X   X
                    -   X   -               */
   // DiagnalCross,    
    /*              X   -   X
                    -   X   -
                    X   -   X               */
    Square,
    /*              X   X   X
                    X   X   X
                    X   X   X               */
    Vertical,
    /*              -   X   -
                    -   X   -
                    -   X   -               */
    Horizontal,
    /*              -   -   -
                    X   X   X
                    -   -   -               */

    //DirectionalTriangle
    /*          
                X   X   X   X   X
                -   X   X   X   -     
                -   -   X   -   -
                -   -   -   -   -           */
    
}
public enum Directions
{
    Default = 0,
    UP,
    RIGHT,
    DOWN,
    LEFT
}