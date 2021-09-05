using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CellScript : MonoBehaviour
{
    [SerializeField] private Vector2Int _currentPosition;
    [SerializeField] public SpriteRenderer _cellImage;
    [SerializeField] TextMeshProUGUI _cellCoordinates_TMP;
    [SerializeField] public RectTransform _recTransform;
    [SerializeField] public Button _button;
    [SerializeField] private TileTypes _type;
    public List<String> DEBUG_BUTTON_ATTACHED_METHODS;
    public List<GameObject> Trash = new List<GameObject>();
    [SerializeField] private ISpecialTile _specialTile;
    [SerializeField] private bool isWalkable = true;

    [SerializeField] private string CurrentAssignedSpecialTileScript;
    private void OnDrawGizmos() 
    {
        if(isWalkable == false)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(transform.position+new Vector3(.33f,.33f,.33f),new Vector3(.5f,.5f,.5f));   
        }   
        if(_specialTile != null)
        {
            if(SpecialTile is Bomb_Cell )
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position+new Vector3(.33f,.33f,.33f),new Vector3(.25f,.25f,.25f));   
            }

             if(SpecialTile is Treasure_Cell )
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(transform.position+new Vector3(.33f,.33f,.33f),new Vector3(.25f,.25f,.25f));   
            }
        }  
    }
    public TileTypes Type
    {
        get => _type;
        set
        {
            if(value == TileTypes.grass || value == TileTypes.player)
            {
                isWalkable = true;
            }
            _type = value;
        }
    }
    public ISpecialTile SpecialTile
    {
        get => _specialTile;
        set
        {
            _specialTile = value;
            if (value == null)
            {
                Trash.ForEach(t=>Destroy(t));
                Trash.Clear();
                 CurrentAssignedSpecialTileScript = "_EMPTY_";
                return;
            }

            this.gameObject.name = value.Name;
            if (value != SpecialTile)
            {
                AssignType(value.Type, value);
            }
            
            DEBUG_BUTTON_ATTACHED_METHODS.Clear();
            _button.onClick.RemoveAllListeners();

            DEBUG_BUTTON_ATTACHED_METHODS.Add($"{value.GetType().ToString()}.OnClick_MakeAction");
            _button.onClick.AddListener(() => value.OnClick_MakeAction());

            this.gameObject.name = value.Name;
            CurrentAssignedSpecialTileScript = value.GetType().ToString();
        }
    }
    public Vector2Int CurrentPosition
    {
        get => _currentPosition;
        set
        {
            var direction = _currentPosition - value;
            var oldPosition = _currentPosition;
            _currentPosition = value;

            if (_specialTile != null)
            {
                if (_specialTile is IFragile)
                {
                    if((_specialTile as Bomb_Cell).IsUsed == false)
                    {
                        if((_specialTile as Bomb_Cell).IsReadyToUse == true)
                        {
                            if( GridManager.instance.BombDetonatedByChainReaction.Contains(this) == false)
                            {
                                GridManager.instance.BombDetonatedByChainReaction.Add(this);
                            }
                        }
                    }
                }
            }
            if(Math.Abs(direction.x)>1 || Math.Abs(direction.y)>1 )
            {
                StartCoroutine(
                    FadeInAnimation(new Vector2(value.x * _recTransform.rect.size.x, value.y * _recTransform.rect.size.y))
                );     
            } 
            else
            {
                StartCoroutine(
                    SlideAnimation( _recTransform.localPosition,  new Vector2(value.x * _recTransform.rect.size.x, value.y * _recTransform.rect.size.y))
                );
            }
            
            //this._recTransform.localPosition =  new Vector2(value.x * _recTransform.rect.size.x, value.y * _recTransform.rect.size.y);
        

        }
    }
    public bool IsWalkable
    { 
        get => isWalkable;
        set 
        {
            isWalkable = value;
        } 
    } 
    public void SetCell(Vector2Int _position, bool runAnimation = true)
    {
        _cellCoordinates_TMP.SetText(_position.ToString());

        CurrentPosition = _position;
       
        if (SpecialTile == null)
        {
            DEBUG_BUTTON_ATTACHED_METHODS.Clear();
            _button.onClick.RemoveAllListeners();
            DEBUG_BUTTON_ATTACHED_METHODS.Add("CellScript.MoveTo()");
            _button.onClick.AddListener(() =>
            {
                MoveTo();
            }
            );
        }
        if (SpecialTile != null)
        {
            if(SpecialTile.ParentCell == null) 
            {
              //  Debug.LogWarning("cell ze specialtilesem ale bez parenta ?");
                return;
            }

            SpecialTile.ParentCell.DEBUG_BUTTON_ATTACHED_METHODS.Clear();
            SpecialTile.ParentCell._button.onClick.RemoveAllListeners();
            
            if (SpecialTile != null)
            {
                SpecialTile.ParentCell._button.onClick.AddListener(() => SpecialTile.OnClick_MakeAction());
                SpecialTile.ParentCell.DEBUG_BUTTON_ATTACHED_METHODS.Add($"{SpecialTile.GetType().ToString()}.OnClick_MakeAction()");
            }
        }
    }
    private IEnumerator FadeInAnimation(Vector2 position)
    {
        this._cellImage.transform.localScale = Vector3.zero;
        this._recTransform.localPosition = position;
        for (int i = 1; i <= 8; i++)
        {
            float progress = i / 8.0f;
            yield return new WaitForFixedUpdate();
            this._cellImage.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(150, 150, 1), progress);
        }
        yield return null;
    }
    private IEnumerator SlideAnimation(Vector3 startingPosition, Vector3 endPosition)
    {
        for (int i = 1; i <= 8; i++)
        {
            float progress = i / 8.0f;
            yield return new WaitForFixedUpdate();
            this._recTransform.localPosition = Vector3.Lerp(startingPosition, endPosition, progress);
        }
        yield return null;
    }
    public void MoveTo()
    {
        if(PlayerManager.instance.currentAutopilot == null)
          PlayerManager.instance.currentAutopilot = StartCoroutine(PlayerManager.instance.Autopilot(this));

        if(GameManager.instance.TurnFinished == false) 
            return;

        Vector2Int direction = GameManager.Player_CELL.CurrentPosition-CurrentPosition;

//        print(direction);
        if(direction.x == 0)
            GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
        
        if(direction.y == 0)
            GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";
        
        PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();
        if(Vector3.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition,(Vector3Int)CurrentPosition) < 1.1f)
        {
            if (GameManager.instance.WybuchWTrakcieWykonywania == true)
                return;

            GridManager.CascadeMoveTo(GameManager.Player_CELL, this.CurrentPosition);
            GameManager.instance.StartCoroutine(GameManager.instance.AddTurn());
        }
    }
    internal void AddEffectImage(GameObject sprite)
    {
        Trash.Add(Instantiate(sprite, this._recTransform));
    }
    public void AssignType(TileTypes _type, ISpecialTile _specialTile = null)
    {
        this.Type = _type;
        if (_specialTile == null) {
            switch (Type)
            {
                case TileTypes.player:
                    this.SpecialTile = new Player_Cell(parent: this, GameManager.instance.GetMonsterData(666));       return;

                case TileTypes.wall:
                    this.SpecialTile = new Obstacle_Cell(
                        parent: this,
                        name: "Sciana",
                        icon_Url: "wall"
                      );
                      return;

                case TileTypes.monster:
                    this.SpecialTile = new Monster_Cell(parent: this, GameManager.instance.GetMonsterData());       return;

                case TileTypes.treasure:
                {
                    this.SpecialTile = new Treasure_Cell(parent: this, GameManager.instance.GetTreasureData(-1));    
                    (this.SpecialTile as Treasure_Cell).RemoveFromMapIfChesIsEmpty();
                    return;
                }

                case TileTypes.bomb:
                    this.SpecialTile = new Bomb_Cell(parent: this, GameManager.instance.GetBombData());             return;

            };
        }
        else
            this.SpecialTile = _specialTile;
    }
}
