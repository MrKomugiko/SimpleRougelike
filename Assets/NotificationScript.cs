using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationScript : MonoBehaviour
{
    public CellScript RelatedCell = null;

   [SerializeField] Image IconHolderImage;
   [SerializeField] Image IconImage;
   [SerializeField] public Image SelectBorder;

   [SerializeField] TextMeshProUGUI CreatureName;
   [SerializeField] TextMeshProUGUI HP;
   [SerializeField] TextMeshProUGUI Speed;
   [SerializeField] TextMeshProUGUI Type;
   [SerializeField] TextMeshProUGUI Deffence;
   [SerializeField] TextMeshProUGUI SpecialAttack;

    public void RefreshData(CellScript relatedCellData)
    {   
        CreatureName.SetText($"[{relatedCellData.Type}] {relatedCellData.name} [Level ?W.I.P? ]"); 
        
        if(relatedCellData.SpecialTile is ICreature){  
            IconImage.sprite = GameManager.instance.specialEffectList.Where(s => s.name == RelatedCell.SpecialTile.Icon_Url).First().GetComponent<SpriteRenderer>().sprite;
            var creature = relatedCellData.SpecialTile as ICreature;
            HP.SetText($"HP: {creature.HealthPoints}/{creature.MaxHealthPoints}");
            Speed.SetText($"Speed: {creature.TurnsRequiredToMakeAction} turns");
            Deffence.SetText($"Deffence: ?W.I.P?");
            Type.SetText($"Type: ?W.I.P?");
            SpecialAttack.SetText($"Special atk: ?W.I.P?");
            return;
        }
        
        if(relatedCellData.SpecialTile is IValuable){
            IconImage.sprite = GameManager.instance.specialEffectList.Where(s => s.name == RelatedCell.SpecialTile.Icon_Url).First().GetComponent<SpriteRenderer>().sprite;
            var valuable = relatedCellData.SpecialTile as IValuable;
            HP.SetText("");
            Speed.SetText("GOLD VALUE : "+valuable.GoldValue);
            Deffence.SetText("");
            Type.SetText("");
            SpecialAttack.SetText("");
            return;
        }
        
        try
        {
            Destroy(this.transform.parent.gameObject);
        }
        catch (System.Exception)
        {
            Debug.LogError("juz usuniety");
        }
    }
}
