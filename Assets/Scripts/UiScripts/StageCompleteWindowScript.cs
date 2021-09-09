using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageCompleteWindowScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI StageTitle_TMP;
    [SerializeField] private TextMeshProUGUI ContentDetailedInto_TMP;

    private void OnEnable() {

        StageTitle_TMP.SetText($"STAGE {GameManager.instance.CurrentStageLevel}:{GameManager.instance.CurrentStageFloor}");
        string contentText = 
        $"  <b>Killed monsters:</b> \t{PlayerManager.instance.CumulativeMonsterKilled}\n"+
        $"  <b>Exp Earned:</b> \t\t{PlayerManager.instance.CumulativeStageExperienceEarned}\n"+
        $"  <b>Gold Earned:</b> \t\t{PlayerManager.instance.CumulativeStageGoldEarned}\n"+
        $"  <b>Damage taken:</b> \t{PlayerManager.instance.CumulativeStageDamageTaken}\n"+
        $"  <b>Damage gained:</b> \t{PlayerManager.instance.CumulativeStageDamageGained}\n"+
        $"  <b>Elapsed Turns:</b> \t{GameManager.instance.CurrentTurnNumber}\n"+
        $"  <b>Revievals used:</b>\t{0}";
        ContentDetailedInto_TMP.SetText(contentText);
    }

}
