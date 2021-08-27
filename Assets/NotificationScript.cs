using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationScript : MonoBehaviour
{
    [SerializeField] public GameObject PossibleActions;
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
        if(BaseCell.SpecialTile is ICreature)
        {  
            var creature = BaseCell.SpecialTile as ICreature;

            IconImage.sprite = creature.Icon_Sprite.GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.Type}] {creature.Name} [Level {creature.Level} ]"); 
            HP.SetText($"HP: {creature.HealthPoints}/{creature.MaxHealthPoints}");
            Speed.SetText($"Speed: {creature.TurnsRequiredToMakeAction} turns");
            Deffence.SetText($"Deffence: ?W.I.P?");
            Type.SetText($"Type: ?W.I.P?");
            SpecialAttack.SetText($"Special atk: ?W.I.P?");
            return;
        }
        
        if(BaseCell.SpecialTile is IValuable)
        {
            //check if its a dead monster or common valuable
            IconImage.sprite = GameManager.instance.specialEffectList.Where(s => s.name == BaseCell.SpecialTile.Icon_Url).First().GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.Type}] {BaseCell.SpecialTile.Name}"); 

            var valuable = BaseCell.SpecialTile as IValuable;
            HP.SetText("");
            Speed.SetText("GOLD VALUE : "+valuable.GoldValue);
            Deffence.SetText("");
            Type.SetText("");
            SpecialAttack.SetText("");
            return;
        }

        if(BaseCell.SpecialTile is Bomb_Cell)
        {
            IconImage.sprite = GameManager.instance.specialEffectList.Where(s => s.name == BaseCell.SpecialTile.Icon_Url).First().GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.SpecialTile.Type}] {BaseCell.SpecialTile.Name}"); 

            var bomb = BaseCell.SpecialTile as Bomb_Cell;
            HP.SetText($"Turns to activate : {Math.Min(bomb.TickCounter.CurrentTickValue,bomb.TickCounter.TickMaxValue)}/{bomb.TickCounter.TickMaxValue}");
            Speed.SetText($"AoE damage : {bomb.DAMAGE} DMG");
            Deffence.SetText("Is activated : "+bomb.IsReadyToUse);
            Type.SetText("Type: Explosive");
            SpecialAttack.SetText("Destroy empty fields");
            return;
        }
    }
    public bool IsVisibleOnNotificationList(CellScript playerCell)
    {   
        // ZOSTAW NA PODGLĄDZIE JEŻELI USTAWIONY JEST SZNACZNIK SLEDZENIA IsHighlighted
        if(BaseCell.SpecialTile == null) return false;
        if((BaseCell.SpecialTile is ISelectable) == false) return false;
        if((BaseCell.SpecialTile as ISelectable).IsHighlighted) return true;

        float distance = Vector2Int.Distance(playerCell.CurrentPosition, BaseCell.SpecialTile.ParentCell.CurrentPosition);
        if(distance < 1.5f)
            return true;
        else
            return false;
    }

    
}
