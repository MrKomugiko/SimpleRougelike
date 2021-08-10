using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellScript : MonoBehaviour
{
    public Vector2Int CurrentPosition;
    [SerializeField] public Image _cellImage;
    [SerializeField] TextMeshProUGUI _cellCoordinates_TMP;
    [SerializeField] RectTransform _recTransform;
    [SerializeField] Button _button;
    [SerializeField] private TileTypes _type;
    
    public ISpecialTile specialTile;

    public TileTypes Type 
    { 
        get => _type; 
        set {
            _type = value; 
            if(value == TileTypes.wall)
            {
                // blokada ruchu 
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(()=>print("to jest sciana"));
            }
        }
    }

    public void SetCell(Vector2Int _position, bool runAnimation = true) {
        CurrentPosition = _position;
        _cellCoordinates_TMP.SetText(_position.ToString());
        
        if(runAnimation == true)
            StartCoroutine(SlideAnimation(_recTransform.localPosition, new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y) ));
        else
            _recTransform.localPosition = new Vector2(CurrentPosition.x * _recTransform.rect.size.x, CurrentPosition.y * _recTransform.rect.size.y);
        
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(()=>GridManager.CascadeMoveTo(GameManager.Player, this.CurrentPosition));
    }
    private IEnumerator SlideAnimation(Vector3 startingPosition, Vector3 endPosition)
    {
        print("rozpoczęcie animacji xd");
        for(int i = 1; i <=10; i++)
        {
            float progress = i/10.0f;
            yield return new WaitForEndOfFrame();
            this._recTransform.localPosition = Vector3.Lerp(startingPosition,endPosition,progress);
        }

        print("koneic animacji");
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
                this._cellImage.color = Color.black;
                break;
            case TileTypes.grass:
                this._cellImage.color = Color.gray;
                break;
            case TileTypes.bomb:
                this._cellImage.color = Color.cyan;
                break;
            case TileTypes.treasure:
                this._cellImage.color = Color.yellow;
                break;
            case TileTypes.monster:
                this._cellImage.color = Color.red;
                break;
        }
        this.gameObject.name = this.Type.ToString();
    }
    
    
}
