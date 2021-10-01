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
                if(value.node_data.Childs != null)
                {
                    CurrentOpenedNodeslist = value.node_data.Childs;
                    foreach(var node in AttackOptionsNodes)
                    {
                        Destroy(node.gameObject);
                    }
                    RebuildTree();
                }
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

    public void RebuildTree()
    {
       // if new selected node is parent of any other nodes, rebuild tree
        // REmove existing nodes objects
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();

        // Midddle = przeznaczony dla quit, back,exit ?
        NodeElements = SelectedNode.node_data.Childs.Count;
        int backnodeIndex = SelectedNode.node_data.Childs.Count / 2;

        for (int i = 0; i < SelectedNode.node_data.Childs.Count; i++)
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
            Destroy(this.gameObject);
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
            CurrentOpenedNodeslist = new List<SelectionPopupNode>()
            {
                (new SelectionPopupNode(0,"EXIT")),
                (new SelectionPopupNode(1,"Attack")),
                (new SelectionPopupNode(2,"Ranged")),
                (new SelectionPopupNode(3,"Magic")),
                (new SelectionPopupNode(4,"Special")),

            };
            CurrentOpenedNodeslist[1].Childs= new List<SelectionPopupNode>()
            {
                (new SelectionPopupNode(0,"BACK",parent:CurrentOpenedNodeslist[1])),
                (new SelectionPopupNode(1,"Attack 1",parent:CurrentOpenedNodeslist[1])),
                (new SelectionPopupNode(2,"Attack 2",parent:CurrentOpenedNodeslist[1])),
                (new SelectionPopupNode(3,"Attack 3",parent:CurrentOpenedNodeslist[1])),
            };
        }
        else
        {
            
            CurrentOpenedNodeslist = parentNode.Childs;
        }
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

        //-------------------------------------------------------------------------------------------
        // sprawdzenie czy rodzic do ktorego sie cofnelismy jest dzieckiem innego node'a, zeby wstawic go odrazu na srodku
        var currentCenterNode = CenterTreeObject.GetComponentInChildren<AttackSelectionPopupNodeScript>();
        if(parentNode.Parent != null)
        {
            // podstaw nowy
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);

            var GrandpaNode = Instantiate(PopupNodeObjectPrefab, this.transform);
            GrandpaNode.GetComponent<AttackSelectionPopupNodeScript>().ConfigureNodeForCenterPosition();
            GrandpaNode.GetComponent<AttackSelectionPopupNodeScript>().SelfConfigure(parentNode.Parent);
        }
        else
        {
            // zostaw pusty srodek
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);
        }
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
        
        // REmove existing nodes objects
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();

        //---------------------------------------------------------------------------------
        CurrentOpenedNodeslist = new List<SelectionPopupNode>()
        {
            (new SelectionPopupNode(0,"EXIT")),
            (new SelectionPopupNode(1,"Attack")),
            (new SelectionPopupNode(2,"Ranged")),
            (new SelectionPopupNode(3,"Magic")),
            (new SelectionPopupNode(4,"Special")),

        };
        CurrentOpenedNodeslist[1].Childs= new List<SelectionPopupNode>()
        {
            (new SelectionPopupNode(0,"BACK",parent:CurrentOpenedNodeslist[1])),
            (new SelectionPopupNode(1,"Attack 1",parent:CurrentOpenedNodeslist[1])),
            (new SelectionPopupNode(2,"Attack 2",parent:CurrentOpenedNodeslist[1])),
            (new SelectionPopupNode(3,"Attack 3",parent:CurrentOpenedNodeslist[1])),
        };
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
}

[Serializable]
public class SelectionPopupNode
{
    public int Node_ID;
    public string Node_Name;
    //public Action AssignedAcction;
    public SelectionPopupNode Parent;
    public List<SelectionPopupNode> Childs = new List<SelectionPopupNode>();

    
    public SelectionPopupNode(int node_ID, string node_Name, SelectionPopupNode parent = null, List<SelectionPopupNode> childs = null)
    {
        Node_ID = node_ID;
        Node_Name = node_Name;
        Parent = parent;
        Childs = childs;
    }
}