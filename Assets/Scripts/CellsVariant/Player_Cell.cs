using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player_Cell : ISpecialTile, ILivingThing, ISelectable
{
    public TileTypes Type { get; private set; } = TileTypes.player;
    public CellScript ParentCell {get;set;}
    public GameObject Icon_Sprite { get; set; }
    public GameObject Border { get; set; }
    public GameObject Corpse_Sprite { get; private set; }
    public List<(Action action, string description, ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string description,ActionIcon icon, bool singleAction)>();
    public string Name { get; set; }
    public int HealthPoints { get; set; }
    public int Level { get; set; }
    public int MaxHealthPoints{get;private set;}
    public int Damage {get; private set;}
    public bool IsHighlighted { get; set; }
    public bool IsAlive
    {
        get
        {
            if(HealthPoints > 0)
                return true;
            else
            {
               // TODO: Podmiana na kapliczke ;d
               // TODO: otwarcie okna  GAMEOVER
                Debug.Log("Player is DEAD");
                HealthPoints = 0;
                ChangeToPlayerCorpse();
                return  false;
            }
        }
    }

    PlayerManager Player;
    public Player_Cell(CellScript parent,  MonsterData _data/*PlayerData _data*/)
    {
        this.ParentCell                   =       parent;       
        this.Name                         =       _data.MonsterName;
        this.MaxHealthPoints              =       _data.MaxHealthPoints;
        this.HealthPoints                 =       _data.MaxHealthPoints;
        this.Damage                       =       _data.Damage;
        this.Type                         =       _data.Type;;
        this.ParentCell.IsWalkable        =       _data.IsWalkable;
        this.Icon_Sprite                  =       _data.Icon_Sprite;
        this.Corpse_Sprite                =       _data.Corpse_Sprite; 
        this.Level                        =       _data.Level;

        AvaiableActions.Add((()=>EquipmentScript.AssignItemToActionSlot(slotID:0),"Empty Slot: asign new...",ActionIcon.Empty, false));
        AvaiableActions.Add((()=>EquipmentScript.AssignItemToActionSlot(slotID:1),"Empty Slot: asign new...",ActionIcon.Empty, false));
        AvaiableActions.Add((()=>EquipmentScript.AssignItemToActionSlot(slotID:2),"Empty Slot: asign new...",ActionIcon.Empty, false));
        AvaiableActions.Add((()=>EquipmentScript.AssignItemToActionSlot(slotID:3),"Empty Slot: asign new...",ActionIcon.Empty, false));
        AvaiableActions.Add((()=>EquipmentScript.AssignItemToActionSlot(slotID:4),"Empty Slot: asign new...",ActionIcon.Empty, false));

        NotificationManger.CreateNewNotificationElement(this);
       
        var monsterObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(monsterObject);
    
        if(_data.IsPathfinderRequired)
        {
            monsterObject.AddComponent<Pathfinding>();  
            ConfigurePathfinderComponent();
        }

        Player = new PlayerManager(this);
    }

    public Pathfinding _pathfinder;
    public void ConfigurePathfinderComponent()
    {
        if (_pathfinder != null) return;
        
        ParentCell.Trash.Where(t => t.name == (Icon_Sprite.name + "(Clone)")).FirstOrDefault().TryGetComponent<Pathfinding>(out _pathfinder);
        if (_pathfinder == null)
            return;
        
        _pathfinder._cellData = ParentCell;
    }
    public void OnClick_MakeAction()
    {
       // Debug.Log("Gracz kliknął na siebie samego");
        GameManager.instance.StartCoroutine(GameManager.instance.AddTurn());
    }
    public void RemoveBorder()
    {
        IsHighlighted = false;
        if (Border != null)
        {
            GameObject.Destroy(Border.gameObject);
        }
    }
    public void TakeDamage(int damage, string source)
    {
        HealthPoints -= damage;
        if(IsAlive)
        {
            Debug.Log($"Player HP decerase from [{HealthPoints + damage}] to [{HealthPoints}] by <{source}>");
        } 
        GameManager.instance.HealthCounter_TMP.SetText(HealthPoints.ToString());
    }
    public void ChangeToPlayerCorpse()
    {
        ParentCell.Trash.ForEach(t=>GameObject.Destroy(t.gameObject));
        ParentCell.Trash.Clear();

        ParentCell.Trash.Add(GameObject.Instantiate(Corpse_Sprite,ParentCell.transform));

        if (Border != null)
        {
            Border.GetComponent<Image>().color = Color.yellow;
            GameObject.Destroy(Border, .5f);
            Border = null;
        }

        NotificationManger.TriggerActionNotification(ParentCell.SpecialTile as ISelectable,NotificationManger.AlertCategory.Loot);

        GameManager.instance.GameOverScreen.SetActive(true);
    }

}
