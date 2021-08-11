using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellScript : MonoBehaviour
{
    public Vector2Int CurrentPosition;
    [SerializeField] public SpriteRenderer _cellImage;
    [SerializeField] TextMeshProUGUI _cellCoordinates_TMP;
    [SerializeField] RectTransform _recTransform;
    [SerializeField]public Button _button;
    [SerializeField] private TileTypes _type;

    public List<GameObject> Trash = new List<GameObject>();

    private ISpecialTile _specialTile;

    public TileTypes Type 
    { 
        get => _type; 
        set {
            _type = value; 
            if(value == TileTypes.wall)
            {
                // TODO: przeniesc ta funkcionalnosc do Ispecialtilesa
                // blokada ruchu 
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(()=>print("to jest sciana"));
            }
            if(value == TileTypes.treasure)
            {
                // TODO: przeniesc ta funkcionalnosc do Ispecialtilesa
                // dodawanie golda 
                _button.onClick.AddListener(()=>GameManager.instance.AddGold(value: 10));
            }
        }
    }

    public ISpecialTile SpecialTile 
    {
        get => _specialTile; 
        set 
        {
            _specialTile = value; 
            if(value == null) return;

            
            AssignType(value.Type);
            this.name = value.Name;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(()=>_specialTile.MakeAction());

            Trash.Add(Instantiate(GameManager.instance.specialEffectList.Where(e=>e.name == value.Icon).First() ,this.transform));

        }
    }
    
    public void SetCell(Vector2Int _position, bool runAnimation = true) {
        print("set cell");
        CurrentPosition = _position;
        _cellCoordinates_TMP.SetText(_position.ToString());
        
        if(runAnimation == true)
            StartCoroutine(SlideAnimation(_recTransform.localPosition, new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y) ));
        else
            _recTransform.localPosition = new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y);
        
       if(_specialTile is Bomb_Cellcs) return;
            
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(()=>
            {
                GameManager.instance.AddTurn();
                GridManager.CascadeMoveTo(GameManager.Player, this.CurrentPosition);
            }
        );
    }

    internal void AddEffectImage(string imageUrl)
    {
        // Instantiate(GameManager.instance.specialEffectList.Where(e=>e.name == this._specialTile.Effect).First() ,this._recTransform);
        Trash.Add(Instantiate(GameManager.instance.specialEffectList.Where(e=>e.name == imageUrl).First(), this._recTransform));
    }

    private IEnumerator SlideAnimation(Vector3 startingPosition, Vector3 endPosition)
    {
        // print("rozpoczęcie animacji xd");
        for(int i = 1; i <=8; i++)
        {
            float progress = i/8.0f;
            yield return new WaitForFixedUpdate();
            this._recTransform.localPosition = Vector3.Lerp(startingPosition,endPosition,progress);
        }

        // print("koneic animacji");
        yield return null;
    }
    public void AssignType(TileTypes _type)
    {

            this.Type = _type;

            switch(Type)
            {
                case TileTypes.player:
                    this._cellImage.color = Color.green;
                    break;

                case TileTypes.wall:
                    Trash.Add(Instantiate(GameManager.instance.ExtrasPrefabList.Where(s=>s.name == "wall").First(),this._recTransform));
                    break;  

                case TileTypes.monster:
                    Trash.Add(Instantiate(GameManager.instance.ExtrasPrefabList.Where(s=>s.name == "monster").First(),this._recTransform));
                    break;

                case TileTypes.treasure:
                    Trash.Add(Instantiate(GameManager.instance.ExtrasPrefabList.Where(s=>s.name == "treasure").First(),this._recTransform));
                    break;  

                default:
                    this._cellImage.sprite = GameManager.instance.SpritesList.Where(s=>s.name == "basic").First();
                    break;
        }

        this.gameObject.name = this.Type.ToString();
    }
}
