using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player_Cell : ISpecialTile, ILivingThing, ISelectable
{
    public TileTypes Type { get; set; } = TileTypes.player;
    public CellScript ParentCell { get; set; }
    public GameObject Icon_Sprite { get; set; }
    public GameObject Border { get; set; }
    public GameObject Corpse_Sprite { get; private set; }
    public List<(Action action, string description, ActionIcon icon, bool singleAction)> AvaiableActions { get; private set; } = new List<(Action action, string description, ActionIcon icon, bool singleAction)>();
    public string Name { get; set; }
    public int HealthPoints 
    { 
        get => _healthPoints; 
        set
        {
            var HP = value;
            if(value > MaxHealthPoints) 
                HP = MaxHealthPoints;

            if(value < 0) 
                HP = 0;

            _healthPoints = HP;
            
            PlayerManager.instance.CurrentHealth = HP;
        }
    }
    public int MaxHealthPoints { get; private set; }
    public float Damage => UnityEngine.Random.Range(PlayerManager.instance.STATS.TotalDamage.min,PlayerManager.instance.STATS.TotalDamage.max);
    public bool IsHighlighted { get; set; }
    public bool IsAlive
    {
        get
        {
            if (HealthPoints > 0)
                return true;
            else
            {
                // TODO: Podmiana na kapliczke ;d
                // TODO: otwarcie okna  GAMEOVER
              //  Debug.Log("Player is DEAD");
                HealthPoints = 0;
                ChangeToPlayerCorpse();

                GameManager.instance.PLAYER_PROGRESS_DATA.isDead = true;

                return false;
            }
        }
    }
    public Pathfinding _pathfinder;
    private int _healthPoints;
    public GameObject playerSpriteObject;
    public event EventHandler<(CellScript parent, int damageTaken, bool criticalHit, bool blockedHit, bool dodgedHit)> OnPlayerTakeDamageEvent;
    public Player_Cell(CellScript parent, MonsterData _data)
    {
        this.ParentCell = parent;
        this.Type = _data.Type;
        this.ParentCell.IsWalkable = _data.IsWalkable;
        this.Icon_Sprite = _data.Icon_Sprite;
        this.Corpse_Sprite = _data.Corpse_Sprite;

       // this.Damage = 1 + PlayerManager.instance.STATS.BaseDamage;
        this.MaxHealthPoints = Mathf.RoundToInt(PlayerManager.instance.STATS.HealthPoints);
        this.Name = PlayerManager.instance.NickName;
        this.HealthPoints = PlayerManager.instance.CurrentHealth;

        
        AvaiableActions.Add((() => EquipmentScript.AssignItemToActionSlot(quickslotID: 0), "Tap to <b>Add Item</b>", ActionIcon.Empty, false));
        AvaiableActions.Add((() => EquipmentScript.AssignItemToActionSlot(quickslotID: 1), "Tap to <b>Add Item</b>", ActionIcon.Empty, false));
        AvaiableActions.Add((() => EquipmentScript.AssignItemToActionSlot(quickslotID: 2), "Tap to <b>Add Item</b>", ActionIcon.Empty, false));
        AvaiableActions.Add((() => EquipmentScript.AssignItemToActionSlot(quickslotID: 3), "Tap to <b>Add Item</b>", ActionIcon.Empty, false));
        AvaiableActions.Add((() => EquipmentScript.AssignItemToActionSlot(quickslotID: 4), "Tap to <b>Add Item</b>", ActionIcon.Empty, false));

        playerSpriteObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(playerSpriteObject);

        if (_data.IsPathfinderRequired)
        {
            playerSpriteObject.AddComponent<Pathfinding>();
            ConfigurePathfinderComponent();
        }
        GameObject.Find("PlayerManager").GetComponent<PlayerManager>().SetPlayerManager(this);
        NotificationManger.CreatePlayerNotificationElement(PlayerManager.instance._playerCell);

        CustomEventManager.instance.RegisterPlayerInEventManager(this);
    }
   
  
    public object SaveAndGetCellProgressData()
    {
       return null;
    }
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
        if(GameManager.instance.CurrentTurnPhase != GameManager.TurnPhase.PlayerMovement) 
        {
            //Log("trwa inna faza niz ruchu gracza");
            return;
        }

        if(PlayerManager.instance.playerCurrentlyMoving == true)
        {
          //  Debug.Log("player currenly moving");
            return;
        }

       // Debug.Log("spokojnie pominąć ture klikajac na siebie");
        PlayerManager.instance.MovmentValidator.HideGrid();
        PlayerManager.instance.playerCurrentlyMoving = false;
        GameManager.instance.PlayerMoved = true;

    }
    public void RemoveBorder()
    {
        IsHighlighted = false;
        if (Border != null)
        {
            GameObject.Destroy(Border.gameObject);
        }
    }
    public void TakeDamage(float damage, string source)
    {
        if(damage >0){
            // redukcja obrazen lub unik
            bool _isCritical = false;
            bool _isDodged = (PlayerManager.instance.STATS.Evasion*100) > UnityEngine.Random.Range(0,1000)?true:false;
            bool _isBlocked = (PlayerManager.instance.STATS.BlockChance*100) > UnityEngine.Random.Range(0,1000)?true:false;
         //   if(_isDodged) Debug.Log("gracz uniknął ataku");
            if(_isBlocked) damage *= .5f;
            

            int _damageAfterReduction = Mathf.RoundToInt(damage-(damage*(PlayerManager.instance.STATS.DamageReduction/100)));

            OnPlayerTakeDamageEvent?.Invoke(this,(parent:ParentCell,damageTaken:Int32.Parse(_damageAfterReduction.ToString()),criticalHit:_isCritical,blockedHit:_isBlocked,dodgedHit:_isDodged));
            if(_isDodged) return;

           // Debug.Log("aktualne hp "+HealthPoints);
            HealthPoints  -= _damageAfterReduction;
            //Debug.Log("otrzymałes :"+_damageAfterReduction);
           // Debug.Log("zaktualizowane hp "+HealthPoints);

            PlayerManager.instance.CurrentHealth = HealthPoints;
            if (IsAlive)
            {
              //  Debug.Log($"Player HP decerase from [{HealthPoints + _damageAfterReduction}] to [{HealthPoints}] by <{source}>");
            }
            PlayerManager.instance.CumulativeStageDamageGained += _damageAfterReduction;
        }

        if(damage < 0)
        {
            // HEAL
          //  Debug.Log("HEAL");
            HealthPoints -= Mathf.RoundToInt(damage);
            PlayerManager.instance.CurrentHealth = HealthPoints;
        }


    }
    public void ChangeToPlayerCorpse()
    {
        ParentCell.Trash.ForEach(t => GameObject.Destroy(t.gameObject));
        ParentCell.Trash.Clear();

        ParentCell.Trash.Add(GameObject.Instantiate(Corpse_Sprite, ParentCell.transform));

        if (Border != null)
        {
            Border.GetComponent<Image>().color = Color.yellow;
            GameObject.Destroy(Border, .5f);
            Border = null;
        }

        NotificationManger.TriggerActionNotification(ParentCell.SpecialTile as ISelectable, NotificationManger.AlertCategory.Loot);

        GameManager.instance.GameOverScreen.SetActive(true);
        PlayerManager.instance.SavePlayerData();
    }
    
}
