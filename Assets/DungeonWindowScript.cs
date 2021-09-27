using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonWindowScript : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI MaxDistanceTraveled_TMP;
    private void OnEnable() {
        MaxDistanceTraveled_TMP.SetText("Max distance traveled:\n"+DungeonManager.instance.maxDungeonTraveledDistance);
    }
}
