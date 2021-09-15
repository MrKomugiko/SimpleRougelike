using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Statistics : MonoBehaviour
{
    [SerializeField] private int _experience, _level, _availablePoints, _dexterity, _strength, _inteligence, _vitality;
    [SerializeField] private float _extra_Critical_Hit_Rate, _extra_Critical_Hit_Damage, _critical_Hit_Rate, 
        _critical_Hit_Damage, _extra_Accuracy, _extra_Evasion,_accuracy,_evasion,_baseDamage, _staminaPoints, 
        _staminaRegeneration, _energyPoints, _energyRegeneration, _healthPoints, _healthRegeneration, _blockChance,
        _extra_BlockChance;
    [SerializeField] public (float min, float max) _extraDamage;

    public int Level {
        get => _level;
        set
        {
            Debug.Log("LEVEL UP!");
            // w przypadku awansu o 2 poziomy , dostanie sie 12 punktow do rozdania
            AvailablePoints += (value-_level) * 6;
            _level = value;
            UIManager.instance.Level_TMP.SetText(Level.ToString());
        }
    }
    public int Strength { 
        get => _strength; 
        set {
            _strength = value; 
             // wartośc przy przycisku sie odświezy
             CoreStatButtonsList.Where(b => b.transform.parent.name == "Strength")
                .First().transform.parent.transform.Find("Value")
                .GetComponent<TextMeshProUGUI>()
                .SetText(Strength.ToString());

            // refresh
            Critical_Hit_Damage = Critical_Hit_Damage;
            StaminaPoints = StaminaPoints;
            StaminaRegeneration = StaminaRegeneration;
            BaseDamage = BaseDamage;
        } 
    }
    public int Dexterity {
        get => _dexterity;
        set
        {
            _dexterity = value;
            // wartośc przy przycisku sie odświezy
            CoreStatButtonsList.Where(b => b.transform.parent.name == "Dexterity")
                .First().transform.parent.transform.Find("Value")
                .GetComponent<TextMeshProUGUI>()
                .SetText(Dexterity.ToString());

            // refresh
            Critical_Hit_Damage = Critical_Hit_Damage;
            Critical_Hit_Rate = Critical_Hit_Rate;
            Accuracy = Accuracy;
            Evasion = Evasion;
            BaseDamage = BaseDamage;
        }
    }
    public int Inteligence {
        get => _inteligence;
        set
        {
            _inteligence = value;
            // wartośc przy przycisku sie odświezy
            CoreStatButtonsList.Where(b => b.transform.parent.name == "Inteligence")
                .First().transform.parent.transform.Find("Value")
                .GetComponent<TextMeshProUGUI>()
                .SetText(Inteligence.ToString());

            //TODO: magic damage
            // refresh
            Critical_Hit_Damage = Critical_Hit_Damage;
            Critical_Hit_Rate = Critical_Hit_Rate;
            EnergyPoints = EnergyPoints;
            EnergyRegeneration = EnergyRegeneration;
        }
    }
    public int Vitality {
        get => _vitality;
        set
        {
            _vitality = value;
            // wartośc przy przycisku sie odświezy
            CoreStatButtonsList.Where(b => b.transform.parent.name == "Vitality")
                .First().transform.parent.transform.Find("Value")
                .GetComponent<TextMeshProUGUI>()
                .SetText(Vitality.ToString());

            // refresh
            BlockChance = BlockChance;
            HealthPoints = HealthPoints;
            HealthRegeneration = HealthRegeneration;
            StaminaPoints = StaminaPoints;
            StaminaRegeneration = StaminaRegeneration;
        }
    }
    public float Critical_Hit_Rate {
        get => _critical_Hit_Rate; 
        private set 
        {
            float _criticalHitRate_base = (Inteligence * .2f) + (Dexterity * .5f);
            _critical_Hit_Rate = _criticalHitRate_base + Extra_Critical_Hit_Rate;
            CriticalHitRate_TMP.SetText(Critical_Hit_Rate.ToString() + "%");
        }
    }
    public float Critical_Hit_Damage { 
        get => _critical_Hit_Damage; 
        private set 
        {
            float _criticalHitDamage_base = 200 + (Strength * 1) + (Inteligence * 2) + (Dexterity * 1);
            _critical_Hit_Damage = _criticalHitDamage_base + Extra_Critical_Hit_Damage;
            CriticalHitDamage_TMP.SetText(Critical_Hit_Damage.ToString() + "%");
        }
    }
    public int AvailablePoints {
        get => _availablePoints;
        set
        {
            if (value <= 0)
            {
                _availablePoints = 0;
                CoreStatButtonsList.ForEach(btn => btn.interactable = false);
            }
            else
            {
                _availablePoints = value;
                CoreStatButtonsList.ForEach(btn => btn.interactable = true);
            }

            AvailablePoints_TMP.SetText(AvailablePoints.ToString());
        }
    }
    public int Experience {
        get => _experience;
        set
        {
            _experience = value;
            // ExperienceCounter_TMP.SetText(Experience.ToString());
            if (Experience >= NextLevelExperience)
            {
                _experience = NextLevelExperience - Experience;

                Level++;
                // print("Level UP");
                if (_experience < 0) _experience = 1;
                UIManager.instance.Experience_Bar.UpdateBar(_experience, NextLevelExperience);
            }
            if (_experience < 0)
            {
                 _experience = 1;
                UIManager.instance.Experience_Bar.UpdateBar(_experience, NextLevelExperience);
            }
        }
    }
    public float Accuracy { 
        get => _accuracy; 
        private set 
        {
            float _accuracy_base = 75 + (Dexterity * 1);
            _accuracy = _accuracy_base + Extra_Accuracy;
            Accuracy_TMP.SetText(Accuracy.ToString() + "%");
        }
    }
    public float Evasion { 
        get => _evasion; 
        private set 
        {
            float _Evasion_base = 0 + (Dexterity * 1);
            _evasion = _Evasion_base + Extra_Evasion;
            Evasion_TMP.SetText(Evasion.ToString() + "%");
        }
    }
    public (float min, float max) TotalDamage 
    {
        get => (BaseDamage+Extra_Damage.min, BaseDamage +Extra_Damage.max);
        private set{
            TotalDamage_TMP.SetText($"{TotalDamage.min} - {TotalDamage.max} [{(TotalDamage.min+TotalDamage.max)/2} AVG]");
        }
    } 
    public float BaseDamage { 
        get => _baseDamage; 
        private set 
        {
            _baseDamage = 1+(Dexterity*.05f)+(Strength*.1f); 
            BaseDamage_TMP.SetText(BaseDamage.ToString("N2")+"DMG");
            //refresh
            TotalDamage = TotalDamage;
        } 
    }
    public float StaminaPoints { 
        get => _staminaPoints; 
        set 
        {
            _staminaPoints = 4 + Mathf.RoundToInt((Strength * .5f) + (Vitality * .2f)); 
            StaminaPoints_TMP.SetText(_staminaPoints.ToString());
        } 
    }
    public float StaminaRegeneration { 
        get => _staminaRegeneration; 
        set {
            _staminaRegeneration = (Strength * .1f) + (Vitality * .05f); 
            StaminaRegeneration_TMP.SetText(_staminaRegeneration.ToString());
        } 
    }   
    public float EnergyPoints { 
        get => _energyPoints; 
        set 
        {
            _energyPoints = 4 + Inteligence * 1f;
            EnergyPoints_TMP.SetText(_energyPoints.ToString());
        } 
    }
    public float EnergyRegeneration { 
        get => _energyRegeneration; 
        set {
            _energyRegeneration = 0 + (Inteligence * .1f);
            EnergyRegeneration_TMP.SetText(_energyRegeneration.ToString());
        } 
    }
    public float HealthPoints { 
        get => _healthPoints; 
        set 
        {
            _healthPoints = 25 + Mathf.RoundToInt(Vitality * 1f);
            HealthPoints_TMP.SetText(_healthPoints.ToString());
            UIManager.instance.Health_Bar.UpdateBar(PlayerManager.instance.CurrentHealth,Mathf.RoundToInt(_healthPoints));
        } 
    }
    public float HealthRegeneration { 
        get => _healthRegeneration; 
        set {
            _healthRegeneration = 0 + Mathf.RoundToInt(Vitality * .1f);
            HealthRegeneration_TMP.SetText(_healthRegeneration.ToString());
        } 
    }
    public float BlockChance { 
        get => _blockChance; 
        private set {
                float _blockChance_base = Vitality * .2f;
                _blockChance = _blockChance_base + Extra_Evasion;
                BlockChance_TMP.SetText(BlockChance.ToString() + "%");
            } 
        }
    
    public  (float min, float max) Extra_Damage{
        get => _extraDamage;
        set {
             _extraDamage = value;  
             BaseDamage_TMP.SetText(BaseDamage.ToString("N2")+"DMG");
             //refresh
             TotalDamage = TotalDamage;
        }
    }
    public float Extra_Critical_Hit_Rate { 
        get => _extra_Critical_Hit_Rate; 
        set 
        {
            _extra_Critical_Hit_Rate = value; 
            //refresh
            Critical_Hit_Rate = Critical_Hit_Rate;
        } 
    }
    public float Extra_Critical_Hit_Damage { 
        get => _extra_Critical_Hit_Damage; 
        set 
        {
            _extra_Critical_Hit_Damage = value; 
            //refresh;
            Critical_Hit_Damage = Critical_Hit_Damage;
        }
    }
    public float Extra_Accuracy {
        get => _extra_Accuracy;
        set
        {
            _extra_Accuracy = value;
            //refresh
            Accuracy = Accuracy; 
        }
    }
    public float Extra_Evasion {
        get => _extra_Evasion;
        set
        {
            _extra_Evasion = value;
            //refresh
            Evasion = Evasion; 
        }
    }
    public float Extra_BlockChance{
        get => _extra_BlockChance; 
        set
        {
            _extra_BlockChance = value;
            BlockChance = BlockChance;
        }
    }

    public int NextLevelExperience => (Level) * (15 * Level * 2);

    [SerializeField] TextMeshProUGUI AvailablePoints_TMP, CriticalHitRate_TMP, CriticalHitDamage_TMP, 
        Accuracy_TMP, Evasion_TMP, BaseDamage_TMP, StaminaPoints_TMP, StaminaRegeneration_TMP, HealthPoints_TMP, 
        HealthRegeneration_TMP, EnergyPoints_TMP, EnergyRegeneration_TMP, BlockChance_TMP,TotalDamage_TMP;
    [SerializeField] List<Button> CoreStatButtonsList = new List<Button>();

    public void AddValue(string statname, float statisticChangeValue = 1)
    {
        Debug.Log("add value +1 to "+statname);
        switch (statname)
        {
            case "INT":
                Inteligence += Mathf.RoundToInt(statisticChangeValue);
                break;

            case "VIT":
                Vitality += Mathf.RoundToInt(statisticChangeValue);
                break;

            case "STR":
                Strength += Mathf.RoundToInt(statisticChangeValue);
                break;

            case "DEX":
                Dexterity += Mathf.RoundToInt(statisticChangeValue);
                break;
        }
    }
    public void ResetExtraValues()
    {
        Extra_BlockChance = 0;
        Extra_Evasion = 0;
        Extra_Accuracy = 0;
        Extra_Critical_Hit_Damage = 0;
        Extra_Critical_Hit_Rate = 0;
        Extra_Damage = (0,0);
    }
    

    public void EquipItem_UpdateStatistics(EquipmentItem item)
    {
        Debug.Log("ZDJĘCIE ITEMKA - staty w dół");
        item.MainPerks.ForEach(perk=>UpdateValueByPerkData(perk));
        item.ExtraPerks.ForEach(perk=>UpdateValueByPerkData(perk));
    }

    public void UnequipItem_UpdateStatistics(EquipmentItem item)
    {
        Debug.Log("ZAŁOŻENIE ITEMKA - staty w góe");
                item.MainPerks.ForEach(perk=>UpdateValueByPerkData(perk));
        item.ExtraPerks.ForEach(perk=>UpdateValueByPerkData(perk));
    }

    private void UpdateValueByPerkData(Perk perk)
    {
        switch(perk.type)
        {
            case PerkType.MinAttack:
                Extra_Damage = ( Extra_Damage.min+Int32.Parse(perk.value), Extra_Damage.max);
                return;

            case PerkType.MaxAttack:
                Extra_Damage = ( Extra_Damage.min, Extra_Damage.max+Int32.Parse(perk.value));
                return;

            case PerkType.StrengthBonus:
                Strength+=Int32.Parse(perk.value);
                return;

            case PerkType.VitalityBonus:
                Vitality+=Int32.Parse(perk.value);
                return;
        }
    }
}
