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
        if(BaseCell.SpecialTile is Player_Cell)
        {  
            var player = BaseCell.SpecialTile as ILivingThing;

            IconImage.sprite = BaseCell.SpecialTile.Icon_Sprite.GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.Type}] {BaseCell.SpecialTile.Name} [Level {player.Level} ]"); 
            HP.SetText($"HP: {player.HealthPoints}/{player.MaxHealthPoints}");
            Speed.SetText($" ");
            Deffence.SetText($" ");
            Type.SetText($" ");
            SpecialAttack.SetText($" ");

            this.gameObject.transform.parent.SetSiblingIndex(0);
            return;
        }

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
            var valuable = BaseCell.SpecialTile as IValuable;

            IconImage.sprite = valuable.Icon_Sprite.GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{BaseCell.Type}] {BaseCell.SpecialTile.Name}"); 

            HP.SetText("");
            if(valuable.chest != null)
                Speed.SetText("GOLD VALUE : "+valuable.chest.TotalValue);
            else
                Speed.SetText("GOLD VALUE : "+valuable.GoldValue);
                
            Deffence.SetText("");
            Type.SetText("");
            SpecialAttack.SetText("");
            return;
        }

        if(BaseCell.SpecialTile is Bomb_Cell)
        {
            var bomb = BaseCell.SpecialTile as Bomb_Cell;

            IconImage.sprite = bomb.Icon_Sprite.GetComponent<SpriteRenderer>().sprite;
            CreatureName.SetText($"[{bomb.Type}] {bomb.Name}"); 

            HP.SetText($"Turns to activate : {Math.Min(bomb.TickCounter.CurrentTickValue,bomb.TickCounter.TickMaxValue)}/{bomb.TickCounter.TickMaxValue}");
            Speed.SetText($"AoE damage : {bomb.BombDamage} DMG");
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
        if(playerCell == null) return false;
        float distance = Vector2Int.Distance(playerCell.CurrentPosition, BaseCell.SpecialTile.ParentCell.CurrentPosition);
        if(distance < 1.5f)
            return true;
        else
            return false;
    }

    
}
