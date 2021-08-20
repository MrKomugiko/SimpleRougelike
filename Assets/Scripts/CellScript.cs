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

    [SerializeField] private int damagedTimes = 0;

    public List<GameObject> Trash = new List<GameObject>();

    private ISpecialTile _specialTile;
    [SerializeField] private bool isWalkable = true;

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
        }  
    }
    public TileTypes Type
    {
        get => _type;
        set
        {
             IsWalkable = true;
            _type = value;
            if (value == TileTypes.wall)
            {
                // TODO: przeniesc ta funkcionalnosc do Ispecialtilesa
                // blokada ruchu 
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => print("to jest sciana"));
                IsWalkable = false;
            }
            if (value == TileTypes.treasure)
            {
                // TODO: przeniesc ta funkcionalnosc do Ispecialtilesa
                // dodawanie golda 
                _button.onClick.AddListener(() => GameManager.instance.AddGold(value: 10));
            }
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
                return;

            AssignType(value.Type);
            this.gameObject.name = value.Name;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => SpecialTile.MakeAction());

            Trash.Add(Instantiate(GameManager.instance.specialEffectList.Where(e => e.name == value.Icon_Url).First(), this.transform));
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
                if (_specialTile.IsReadyToUse)
                {
                    if (_specialTile is IFragile)
                    {
                        {
                            (_specialTile as IFragile).DetonateOnMove(_currentPosition, direction);
                        }
                    }
                }
            }
        }
    }

    public int DamagedTimes
    {
        get => damagedTimes;
        set
        {
            damagedTimes = value;
            if (_specialTile != null)
            {
                if (_specialTile.Active)
                {
                    print("special tile został zaatakowany / zniszczony ? oberał = wykonaj jego akcje");
                    _specialTile.MakeAction();
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
        _cellCoordinates_TMP.SetText(_position.ToString());

        if (runAnimation == true)
            StartCoroutine(SlideAnimation(_recTransform.localPosition, new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y)));
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
            GameManager.instance.AddTurn();
            GridManager.CascadeMoveTo(GameManager.Player_CELL, this.CurrentPosition);
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
                if (GridManager.CellGridTable[position].isWalkable == false)
                    return (false, "wskazane pole jest nieosiągalne z powodu znacznika IsWalkable = false");

                if (GameManager.instance.WybuchWTrakcieWykonywania == true)
                {
                    print("poczekaj aż zakończą się wybuchy ;d");
                    return (false, "oczekiwanie na zakończenie animacji wybuchów");
                }
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

        // print("koneic animacji");
        yield return null;
    }
    public void AssignType(TileTypes _type)
    {
        this.Type = _type;
        if (Type == TileTypes.player)
        {

        };
        this.gameObject.name = this.Type.ToString();

        switch (Type)
        {
            case TileTypes.player:
                this._cellImage.color = Color.green;
                break;

            case TileTypes.wall:
                Trash.Add(Instantiate(GameManager.instance.ExtrasPrefabList.Where(s => s.name == "wall").First(), this._recTransform));
                break;

            case TileTypes.monster:
                Trash.Add(Instantiate(GameManager.instance.ExtrasPrefabList.Where(s => s.name == "monster").First(), this._recTransform));

                break;

            case TileTypes.treasure:
                if (SpecialTile == null)
                {
                    this.SpecialTile = new Treasure_Cell(
                        parent: this,
                        name: "Złote monety",
                        icon_Url: "treasure",
                        goldValue: 10
                    );
                }
                break;

            case TileTypes.bomb:
                if (SpecialTile == null)
                {
                    this.SpecialTile = new Bomb_Cell(
                        parent: this,
                        name: "Mina przeciwpiechotna",
                        effect_Url: "bomb_explosion_image",
                        icon_Url: "bomb_icon"
                    );
                }
                break;

            default:
                this._cellImage.sprite = GameManager.instance.SpritesList.Where(s => s.name == "basic").First();
                break;
        }


    }
}
