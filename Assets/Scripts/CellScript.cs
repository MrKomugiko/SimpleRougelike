using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CellScript : MonoBehaviour, ITaskable
{
    [SerializeField] private Vector2Int _currentPosition;
    [SerializeField] public SpriteRenderer _cellImage;
    [SerializeField] TextMeshProUGUI _cellCoordinates_TMP;
    [SerializeField] RectTransform _recTransform;
    [SerializeField] public Button _button;
    [SerializeField] private TileTypes _type;

    public List<String> DEBUG_BUTTON_ATTACHED_METHODS;
    
    public List<GameObject> Trash = new List<GameObject>();

    [SerializeField] private ISpecialTile _specialTile;
    [SerializeField] private bool isWalkable = true;

[SerializeField] private string CurrentAssignedSpecialTileScript;
    private void OnDrawGizmos() {
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

            try
            {
                Trash.Add(Instantiate(GameManager.instance.specialEffectList.Where(e => e.name == value.Icon_Url).First(), this.transform));   
            }
            catch (System.Exception)
            {
    
               //  throw;
            }
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
                // print("gettype = "+ _specialTile.GetType());    
                if (_specialTile is IFragile)
                {
                    if ((_specialTile as IFragile).IsReadyToUse)
                    {
                        
                            if((_specialTile as IFragile).IsReadyToUse)
                            {
                              //  print("boomb move");
                                (_specialTile as IFragile).ActionOnMove(_currentPosition,direction);
                            }
                        
                    }
                }
            }
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
        
        CurrentPosition = _position;
       // _cellCoordinates_TMP.SetText(_position.ToString());

        if (runAnimation == true)
        {
            GameManager.CurrentMovingTiles.Add(this);
            StartCoroutine(SlideAnimation(_recTransform.localPosition, new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y)));
        }
        else
            StartCoroutine(FadeInAnimation(new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y)));


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
                Debug.LogWarning("cell ze specialtilesem ale bez parenta");
                return;
            }
            SpecialTile.ParentCell.DEBUG_BUTTON_ATTACHED_METHODS.Clear();
            SpecialTile.ParentCell._button.onClick.RemoveAllListeners();
            
            if (SpecialTile != null)
            {
                SpecialTile.ParentCell._button.onClick.AddListener(() => SpecialTile.OnClick_MakeAction());
                SpecialTile.ParentCell.DEBUG_BUTTON_ATTACHED_METHODS.Add($"{SpecialTile.GetType().ToString()}.OnClick_MakeAction()");
            }
            else
            {
                Debug.LogWarning("pominięcie sprawdzania Specialtile==null, następnie w srodku warunku dowołuje sie do specialtile.parent ? ");
            }
        }
    }

    private IEnumerator FadeInAnimation(Vector2 position)
    {
        // print("rozpoczęcie animacji xd");
        this._cellImage.transform.localScale = Vector3.zero;
        this._recTransform.localPosition = position;
        for (int i = 1; i <= 10; i++)
        {
            float progress = i / 10.0f;
            yield return new WaitForFixedUpdate();
            this._cellImage.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(150, 150, 1), progress);
        }

        // print("koneic animacji");
        yield return null;
    }

    public void MoveTo()
    {
        if(Vector3.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition,(Vector3Int)CurrentPosition) < 1.1f)
        {
            // print("click: move to");
            if (GameManager.instance.WybuchWTrakcieWykonywania == true)
            {
                // print("poczekaj aż zakończą się wybuchy ;d");
                return;
            }

            GridManager.CascadeMoveTo(GameManager.Player_CELL, this.CurrentPosition);
            GameManager.instance.StartCoroutine(GameManager.instance.AddTurn());
        }

        // if(TaskManager.TaskManagerIsOn == false)
        // {
        //     GridManager.CascadeMoveTo(GameManager.Player_CELL, this.CurrentPosition);
        //     GameManager.instance.AddTurn();
        // }
        // else
        // {
        //     AddActionToQUEUE();
        // }

    }

    public void AddActionToQUEUE()
    {
        var position = this.CurrentPosition;
        TaskManager.AddToActionQueue(
            $"Add turn and Move player to:[{position.x};{position.y}]",
            () =>
            {
                if(GridManager.CellGridTable[position].SpecialTile !=null)
                    if(GridManager.CellGridTable[position].SpecialTile is Monster_Cell)
                        return (false, "nie możesz przejśc na wskazaną pozycje, znajduje sie tam monsterek");
                    else if(GridManager.CellGridTable[position].SpecialTile is Bomb_Cell)
                        return (false, "nie możesz przejśc na wskazaną pozycje, znajduje sie tam bomba");

                if (GridManager.CellGridTable[position].isWalkable == false)
                    return (false, "wskazane pole jest nieosiągalne z powodu znacznika IsWalkable = false");

                if (GameManager.instance.WybuchWTrakcieWykonywania == true)
                    return (false, "oczekiwanie na zakończenie animacji wybuchów");
                else
                {
                    if(Vector3.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition,(Vector3Int)position) > 1.1f)
                    {
                        return (false, "Wskazane pole znajduje się poza zasięgiem ruchu 1 pola");  
                    }

                     GameManager.instance.StartCoroutine(GameManager.instance.AddTurn());
                    GridManager.CascadeMoveTo(GameManager.Player_CELL, position);
                    return (true, "succes");
                }
            }
        );
    }

    internal void AddEffectImage(string imageUrl)
    {
        // Instantiate(GameManager.instance.specialEffectList.Where(e=>e.name == this._specialTile.Effect).First() ,this._recTransform);
        Trash.Add(Instantiate(GameManager.instance.specialEffectList.Where(e => e.name == imageUrl).First(), this._recTransform));
    }

    private IEnumerator SlideAnimation(Vector3 startingPosition, Vector3 endPosition)
    {

        // print("rozpoczęcie animacji xd");
        for (int i = 1; i <= 8; i++)
        {
            float progress = i / 8.0f;
            yield return new WaitForFixedUpdate();
            this._recTransform.localPosition = Vector3.Lerp(startingPosition, endPosition, progress);
        }
        GameManager.CurrentMovingTiles.Remove(this);
        // print("koneic animacji");
        yield return null;
    }
    public void AssignType(TileTypes _type, ISpecialTile _specialTile = null)
    {

        this.Type = _type;
        if (Type == TileTypes.player)
        {
            this._cellImage.color = Color.green;

            return;
        };
        
        if (_specialTile == null) {
            switch (Type)
            {
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
                    this.SpecialTile = new Treasure_Cell(parent: this, GameManager.instance.GetTreasureData(0));    return;

                case TileTypes.bomb:
                    this.SpecialTile = new Bomb_Cell(
                        parent: this,
                        name: "Mina przeciwpiechotna",
                        effect_Url: "bomb_explosion_image",
                        icon_Url: "bomb_icon",
                        turnsRequiredToActivate: 5
                    );
                    return;
            };
        }
        else
            this.SpecialTile = _specialTile;
    }
}
