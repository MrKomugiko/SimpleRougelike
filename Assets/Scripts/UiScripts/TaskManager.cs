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
    public static Queue<Func<(bool status, string message)>> myQueue_Actions = new Queue<Func<(bool status, string message)>>();

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
        foreach(var task in myQueue_Debug)
        {
            if(task!=null)
                Destroy(task);
        }
        myQueue_Actions.Clear();
        myQueue_Debug.Clear();
    }
    private IEnumerator RunActionsFromList()
    {
        while(myQueue_Actions.Count>0)
        {  
            var _ACTION = myQueue_Actions.Dequeue();

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
                
                return _ACTION().status;
            });
            if(isCancelled)
            {
                ResultTMP.color = Color.red;
                ResultTMP.SetText($"<b>Result: [FAILED] </b> {_ACTION().message}");
                foreach(var Qdebug in myQueue_Debug)
                {
                    var QdebugTMP = Qdebug.GetComponent<TextMeshProUGUI>();
                    QdebugTMP.color = Color.red;
                    QdebugTMP.SetText($"<s>{QdebugTMP.text}</s>");
                }
                yield break;
            }
            var _debug = myQueue_Debug.Dequeue();
            Destroy(_debug);
           // print("queue :" + TaskManager.myQueue_Actions.Count);
            yield return new WaitForSeconds(TIME_DELAY_BEETWEN_ACTIONS);
        }
    }

    public static void AddToActionQueue(string desctiption, Func<(bool status, string message)> action)
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
