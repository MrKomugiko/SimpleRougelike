using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackSelectionPopupController : MonoBehaviour
{   
    [SerializeField] private GameObject PopupNodeObjectPrefab;
    private int NodeElements;
    [SerializeField] private AttackSelectionPopupNodeScript _selectedNode = null;
    private float AngleChangeValue => 360f / NodeElements;
    [SerializeField] private GameObject CenterTreeObject;
    [SerializeField] private List<AttackSelectionPopupNodeScript> AttackOptionsNodes = new List<AttackSelectionPopupNodeScript>();

    public List<SelectionPopupNode> CurrentOpenedNodeslist = new List<SelectionPopupNode>();
    public AttackSelectionPopupNodeScript SelectedNode
    {
        get => _selectedNode;
        set {

            if(value != null && value != _selectedNode)
            {
                _selectedNode = value;

                DuplicateSelectedNodeAndCenter();
                if(value.node_data.Childs.Count > 0)
                {
                    Debug.Log("wybrany node - parent");
                    CurrentOpenedNodeslist = value.node_data.Childs;
                    foreach(var node in AttackOptionsNodes)
                    {
                        Destroy(node.gameObject);
                    }
                    RebuildTree(_nodeElements: SelectedNode.node_data.Childs.Count);
                    return;
                }
                Debug.Log("wybrany node to pojedynczy child");

            }
        }
    }

    private void DuplicateSelectedNodeAndCenter()
    {
       var currentCenterNode = CenterTreeObject.GetComponentInChildren<AttackSelectionPopupNodeScript>();
       
        if(currentCenterNode != null)
            Destroy(currentCenterNode.gameObject);

        var copy = Instantiate(SelectedNode.gameObject,CenterTreeObject.transform);
        _selectedNode = copy.GetComponent<AttackSelectionPopupNodeScript>();
        _selectedNode.ConfigureNodeForCenterPosition();
    }

    public void RebuildTree(int _nodeElements)
    {
        Debug.Log("rebuld");
       // if new selected node is parent of any other nodes, rebuild tree
        // REmove existing nodes objects
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();

        // Midddle = przeznaczony dla quit, back,exit ?
    
        NodeElements = _nodeElements;
        int backnodeIndex = NodeElements / 2;

        for (int i = 0; i < NodeElements; i++)
        {
            var node = Instantiate(PopupNodeObjectPrefab,this.transform);
            var nodeScript = node.GetComponent<AttackSelectionPopupNodeScript>();
           
            AttackOptionsNodes.Add(nodeScript);
            node.transform.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2Int(0, -50);
            nodeScript.transform.Rotate(0,0,(i == 0)?180:(180 + (AngleChangeValue * i)));
            nodeScript.SelfConfigure(CurrentOpenedNodeslist[i]);
         
            //node.transform.GetComponentInParent<RectTransform>().SetPositionAndRotation(new Vector3(0, -50,0), _quaternion);
        }
    }
    public void RebuildTree_BackButton(SelectionPopupNode parentNode)
    {
        if(parentNode == null)
        {
            this.gameObject.SetActive(false);
            Debug.Log("parentnode == null, hide and quit");
            return;
        }
     // REmove existing nodes objects
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();

        //---------------------------------------------------------------------------------
        if(parentNode.Parent == null)
        {
            Debug.Log("obiekt do ktorego sie cofamy pochodzi z głównego roota, nie ma rodzica, zostanie wygenerowany standardowy rozklad"); 
            CurrentOpenedNodeslist = GetNodesTree();
        }
        else
        {
            CurrentOpenedNodeslist = parentNode.Parent.Childs;
        }

        //-------------------------------------------------------------------------------------------
        // sprawdzenie czy rodzic do ktorego sie cofnelismy jest dzieckiem innego node'a, zeby wstawic go odrazu na srodku
        var currentCenterNode = CenterTreeObject.GetComponentInChildren<AttackSelectionPopupNodeScript>();
        if(parentNode.Parent != null)
        {
            Debug.Log("wrzucenei na srodek dziadka xd");
            // podstaw nowy
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);

            var GrandpaNode = Instantiate(PopupNodeObjectPrefab, CenterTreeObject.transform);
            Debug.Log("stwozenie nowego obiektu kolka");

            GrandpaNode.GetComponent<AttackSelectionPopupNodeScript>().SelfConfigure(parentNode.Parent);
            Debug.Log("skonfigurowanie kolka, nazwy/koloru");

            GrandpaNode.GetComponent<AttackSelectionPopupNodeScript>().ConfigureNodeForCenterPosition();
            Debug.Log("skonfigurowanie go na srodek - wysrodkowanie itp");

            _selectedNode =  GrandpaNode.GetComponent<AttackSelectionPopupNodeScript>();
            Debug.Log("przypisanie wartosci do selectednode");
        }
        else
        {
            // zostaw pusty srodek
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);
        }

        RebuildTree(_nodeElements:CurrentOpenedNodeslist.Count);    
    }
    

    [ContextMenu("Fix rotations")]
    public void FixRotations()
    {
        foreach (var node in AttackOptionsNodes)
        {
            node.AdjustImageRotation();
        }
    }

    [ContextMenu("Spawn 5 nodes + 1 back-exit dla 180\"")]
    public void OPENandSpawnInitNodesTree()
    {
        this.gameObject.SetActive(true);
        // REmove existing nodes objects
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();

        //---------------------------------------------------------------------------------
        CurrentOpenedNodeslist = GetNodesTree();
        //---------------------------------------------------------------------------------
        NodeElements = CurrentOpenedNodeslist.Count;
        // Midddle = przeznaczony dla quit, back,exit ?
        int backnodeIndex = CurrentOpenedNodeslist.Count / 2;
        for (int i = 0; i < CurrentOpenedNodeslist.Count; i++)
        {
            var node = Instantiate(PopupNodeObjectPrefab, this.transform);
            var nodeScript = node.GetComponent<AttackSelectionPopupNodeScript>();

            AttackOptionsNodes.Add(nodeScript);
            node.transform.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2Int(0, -50);
            nodeScript.transform.Rotate(0,0,(i == 0)?180:(180 + (AngleChangeValue * i)));
            nodeScript.SelfConfigure(CurrentOpenedNodeslist[i]);
        }
    }

    private List<SelectionPopupNode> GetNodesTree()
    {
        var ROOT = new List<SelectionPopupNode>()
        {
            (new SelectionPopupNode(0,"EXIT",parent:null)),
            (new SelectionPopupNode(1,"Attack",parent:null)),
            (new SelectionPopupNode(2,"Ranged",parent:null)),
            (new SelectionPopupNode(3,"Magic",parent:null)),
            (new SelectionPopupNode(4,"Special",parent:null)),
        };
        ROOT[1].AddChildRange(new List<string>(){"Slash","Kick","Strong Attacks","Special Attacks"});
        ROOT[1]["Strong Attacks"].AddChildRange(new List<string>(){"Wind Slash","Piercing stab","Guillotine"});
        ROOT[1]["Special Attacks"].AddChildRange(new List<string>(){"Backstab teleport","Dash attack"});

        ROOT[2].AddChildRange(new List<string>(){"Shoot","Headshoot","Long Distance","Special Attacks"});
        ROOT[2]["Long Distance"].AddChildRange(new List<string>(){"Piercing shoot","Rock throw"});
        ROOT[2]["Special Attacks"].AddChildRange(new List<string>(){"Arrows rain","Holy arrow","Explosive arrow"});

        return ROOT;
    }
}

[Serializable]
public class SelectionPopupNode
{
    public string Node_Name;
    public int Node_ID;
    //public Action AssignedAcction;
    public SelectionPopupNode Parent;
    public List<SelectionPopupNode> Childs = new List<SelectionPopupNode>();
    public SelectionPopupNode this[string childName] =>  Childs[Childs.FindIndex(child=>child.Node_Name == childName)];
    public SelectionPopupNode this[int childIndex] =>  Childs[childIndex];
    public SelectionPopupNode(int node_ID, string node_Name, SelectionPopupNode parent)
    {
        Node_ID = node_ID;
        Node_Name = node_Name;
        Parent = parent;
    }

    public void AddChild(string _nodeTitle)
    {
        if(Childs.Count == 0)
        {
            Childs.Add(new SelectionPopupNode(Childs.Count,"BACK",parent:this));
        }
        Childs.Add(new SelectionPopupNode(Childs.Count,_nodeTitle,parent:this));
    }
    public void AddChildRange(List<string> _nodeTitleList)
    {
        if(Childs.Count == 0)
            Childs.Add(new SelectionPopupNode(Childs.Count,"BACK",parent:this));

        foreach(var title in _nodeTitleList)
        {
            Childs.Add(new SelectionPopupNode(Childs.Count,title,parent:this));
        }
    }
}