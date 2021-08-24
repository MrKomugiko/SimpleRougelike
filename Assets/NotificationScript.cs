using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationScript : MonoBehaviour
{
    public CellScript BaseCell = null;

   [SerializeField] Image IconHolderImage;
   [SerializeField] Image IconImage;
   [SerializeField] public Image SelectBorder;

   [SerializeField] TextMeshProUGUI CreatureName;
   [SerializeField] TextMeshProUGUI HP;
   [SerializeField] TextMeshProUGUI Speed;
   [SerializeField] TextMeshProUGUI Type;
   [SerializeField] TextMeshProUGUI Deffence;
   [SerializeField] TextMeshProUGUI SpecialAttack;

    public void RefreshData()
    {   
     
        if(BaseCell.SpecialTile is ICreature){  
            
            IconImage.sprite = GameManager.instance.specialEffectList.Where(s => s.name == BaseCell.SpecialTile.Icon_Url).First().GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.Type}] {BaseCell.SpecialTile.Name} [Level ? ]"); 

            var creature = BaseCell.SpecialTile as ICreature;
            HP.SetText($"HP: {creature.HealthPoints}/{creature.MaxHealthPoints}");
            Speed.SetText($"Speed: {creature.TurnsRequiredToMakeAction} turns");
            Deffence.SetText($"Deffence: ?W.I.P?");
            Type.SetText($"Type: ?W.I.P?");
            SpecialAttack.SetText($"Special atk: ?W.I.P?");
            return;
        }
        
        if(BaseCell.SpecialTile is IValuable){
            IconImage.sprite = GameManager.instance.specialEffectList.Where(s => s.name == BaseCell.SpecialTile.Icon_Url).First().GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.Type}] {BaseCell.SpecialTile.Name} [Level ? ]"); 

            var valuable = BaseCell.SpecialTile as IValuable;
            HP.SetText("");
            Speed.SetText("GOLD VALUE : "+valuable.GoldValue);
            Deffence.SetText("");
            Type.SetText("");
            SpecialAttack.SetText("");
            return;
        }
    }

    public bool IsInRange(CellScript cell)
    {   
        if(BaseCell.SpecialTile == null) return false;

        float distance = Vector2Int.Distance(cell.CurrentPosition, BaseCell.SpecialTile.ParentCell.CurrentPosition);
        if(distance < 1.5f)
            return true;
        else
            return false;
    }
}
