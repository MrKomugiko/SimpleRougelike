using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static bool TaskManagerIsOn = false;
    public const int TICKS_AFTER_FAIL = 100;
    public const float TIME_DELAY_BEETWEN_ACTIONS = .2f;
    [SerializeField] private GameObject TaskListContainer;
    [SerializeField] private GameObject QueueObjectPrefab;
    [SerializeField] private TextMeshProUGUI ResultTMP;
    public static TaskManager instance;
    
    public static Queue<GameObject> myQueue_Debug = new Queue<GameObject>();
    public static Queue<Func<bool>> myQueue_Actions = new Queue<Func<bool>>();

    public static GameObject _task_Debug = null;


    private void Awake() {
        instance = this;
    }

    [ContextMenu("ExecuteFirst")]   public void OnClick_ExecuteFirst()
    {
        var debug = myQueue_Debug.Dequeue();
        Destroy(debug);
        var action = myQueue_Actions.Dequeue();
        action();
    }
    [ContextMenu("ExecuteAll")]     public void OnClick_ExecuteAll()
    {
        StartCoroutine(RunActionsFromList());
    }
    [ContextMenu("ClearTaskList")]  public void OnClick_ClearTaskList()
    {
        foreach(var task in myQueue_Actions)
        {
            var debug = myQueue_Debug.Dequeue();
            Destroy(debug);
        }
        myQueue_Actions.Clear();
    }
    private IEnumerator RunActionsFromList()
    {
        foreach(var action in myQueue_Actions)
        {   
            int i = 0;
            bool isCancelled = false;
            yield return new WaitUntil(()=>
            {
                i++;
                if(i>TICKS_AFTER_FAIL)
                {
                    isCancelled = true;
                    return true;
                }
                return action();
            });
            if(isCancelled)
            {
                ResultTMP.color = Color.red;
                ResultTMP.SetText("<b>Result: [FAILED] </b> Nie można wykonać akcji, przerwanie wykonywania zadań");
                foreach(var Qdebug in myQueue_Debug)
                {
                    var QdebugTMP = Qdebug.GetComponent<TextMeshProUGUI>();
                    QdebugTMP.color = Color.red;
                    QdebugTMP.SetText($"<s>{QdebugTMP.text}</s>");
                }
                yield break;
            }
            var debug = myQueue_Debug.Dequeue();
            Destroy(debug);
            
            yield return new WaitForSeconds(TIME_DELAY_BEETWEN_ACTIONS);
        }
    }

    public static void AddToActionQueue(string desctiption, Func<bool> action)
    {
        _task_Debug = Instantiate(TaskManager.instance.QueueObjectPrefab,TaskManager.instance.TaskListContainer.transform);
        _task_Debug.GetComponent<TextMeshProUGUI>().SetText(desctiption);

        myQueue_Debug.Enqueue(_task_Debug);
        myQueue_Actions.Enqueue(action);
    }

    public void OnClick_TurnONOFF()
    {
        TaskManagerIsOn = !TaskManagerIsOn;
        if(TaskManagerIsOn == false)
            OnClick_ClearTaskList();
    }
}
