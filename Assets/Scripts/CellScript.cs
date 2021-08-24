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

    public List<GameObject> Trash = new List<GameObject>();

    private ISpecialTile _specialTile;
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
            // IsWalkable = true;
            _type = value;
        }
    }

    public ISpecialTile SpecialTile
    {
        get => _specialTile;
        set
        {
            if (value == SpecialTile)
                return;
            _specialTile = value;
            if (value == null)
            {
                Trash.ForEach(t=>Destroy(t));
                Trash.Clear();
                return;
            }

            this.gameObject.name = value.Name;
            AssignType(value.Type, value);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => SpecialTile.OnClick_MakeAction());
            Trash.Add(Instantiate(GameManager.instance.specialEffectList.Where(e => e.name == value.Icon_Url).First(), this.transform));
            this.gameObject.name = SpecialTile.Name;
            CurrentAssignedSpecialTileScript = SpecialTile.GetType().ToString();
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
                        {
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
        internal set 
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
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                MoveTo();
            }
            );
        }
    //     else
    //     {
    //         if(SpecialTile is ISelectable)
    //         {
    //             Debug.LogWarning("SET CELL");
    //             NotificationManger.RefreshNotification(SpecialTile as ISelectable);
    //         }
    // }   

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
        print("click: move to");
        if (GameManager.instance.WybuchWTrakcieWykonywania == true)
        {
            print("poczekaj aż zakończą się wybuchy ;d");
            return;
        }

        if(TaskManager.TaskManagerIsOn == false)
        {
            GridManager.CascadeMoveTo(GameManager.Player_CELL, this.CurrentPosition);
            GameManager.instance.AddTurn();
        }
        else
        {
            AddActionToQUEUE();
        }

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

                    GameManager.instance.AddTurn();
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
        this.gameObject.name = this.Type.ToString();

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
                    this.SpecialTile = new Monster_Cell(
                        parent: this,
                        name: "Monster_X",
                        icon_Url: "monster",
                        maxHealthPoints: 2,
                        speed: 2
                        );
                    (this.SpecialTile as Monster_Cell).ConfigurePathfinderComponent();
                    return;

                case TileTypes.treasure:
                    this.SpecialTile = new Treasure_Cell(
                        parent: this,
                        name: "Złote monety",
                        icon_Url: "treasure",
                        goldValue: 10
                    );
                    return;

                case TileTypes.bomb:
                    this.SpecialTile = new Bomb_Cell(
                        parent: this,
                        name: "Mina przeciwpiechotna",
                        effect_Url: "bomb_explosion_image",
                        icon_Url: "bomb_icon",
                        turnsRequiredToActivate: 5
                    );
                    return;

            default:
                this._cellImage.sprite = GameManager.instance.SpritesList.Where(s => s.name == "basic").First();
                break;
            };
        }
        else
            this.SpecialTile = _specialTile;
    }
}
