using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class SelectionPopupController : MonoBehaviour
{   
    public List<Sprite> SkillIconsList = new List<Sprite>();
    [SerializeField] private GameObject PopupNodeObjectPrefab;
    private int CurrentDisplayedNodeElements;
    
    [SerializeField] private SelectionPopupNodeScript _selectedNode = null;
    private float AngleChangeValue => 360f / CurrentDisplayedNodeElements;
    [SerializeField] private GameObject CenterTreeObject;
    [SerializeField] private List<SelectionPopupNodeScript> AttackOptionsNodes = new List<SelectionPopupNodeScript>();

    public List<SkillNode> CurrentOpenedNodeslist = new List<SkillNode>();
    public SelectionPopupNodeScript SelectedNode
    {
        get => _selectedNode;
        set {

            if(value != null && value != _selectedNode)
            {
                // szukanie wczesniejszczego wybranego node'a ( z listy, ogolnej )
                if(_selectedNode != null)
                {
                    var x = AttackOptionsNodes.Where(c=>c.node_data.Node_ID == _selectedNode.node_data.Node_ID).FirstOrDefault();
                    if(x != null) 
                        x.EnableSelectionDarkOverlay(false);
                }


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
            }
        }
    }

    private void DuplicateSelectedNodeAndCenter()
    {
       var currentCenterNode = CenterTreeObject.GetComponentInChildren<SelectionPopupNodeScript>();
       // odznaczenie wczesniej wybranego skilla powiązanego z obiektem w srodku ( to jego klon, ale ID takie samo )
  
        if(currentCenterNode != null)
            Destroy(currentCenterNode.gameObject);

        var copy = Instantiate(SelectedNode.gameObject,CenterTreeObject.transform);
        _selectedNode = copy.GetComponent<SelectionPopupNodeScript>();
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
    
        CurrentDisplayedNodeElements = _nodeElements;
        int backnodeIndex = CurrentDisplayedNodeElements / 2;

        for (int i = 0; i < CurrentDisplayedNodeElements; i++)
        {
            var node = Instantiate(PopupNodeObjectPrefab,this.transform);
            var nodeScript = node.GetComponent<SelectionPopupNodeScript>();
           
            AttackOptionsNodes.Add(nodeScript);
            node.transform.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2Int(0, -50);
            nodeScript.transform.Rotate(0,0,(i == 0)?180:(180 + (AngleChangeValue * i)));
            nodeScript.SelfConfigure(CurrentOpenedNodeslist[i]);
         
            //node.transform.GetComponentInParent<RectTransform>().SetPositionAndRotation(new Vector3(0, -50,0), _quaternion);
        }
    }
    public void RebuildTree_BackButton(SkillNode parentNode)
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
            CurrentOpenedNodeslist = SkillsManager.ROOT_SKILLTREE.Childs;
        }
        else
        {
            CurrentOpenedNodeslist = parentNode.Parent.Childs;
        }

        //-------------------------------------------------------------------------------------------
        // sprawdzenie czy rodzic do ktorego sie cofnelismy jest dzieckiem innego node'a, zeby wstawic go odrazu na srodku
        var currentCenterNode = CenterTreeObject.GetComponentInChildren<SelectionPopupNodeScript>();
        if(parentNode.Parent != null)
        {
            Debug.Log("wrzucenei na srodek dziadka xd");
            // podstaw nowy
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);

            var GrandpaNode = Instantiate(PopupNodeObjectPrefab, CenterTreeObject.transform);
            Debug.Log("stwozenie nowego obiektu kolka");

            GrandpaNode.GetComponent<SelectionPopupNodeScript>().SelfConfigure(parentNode.Parent);
            Debug.Log("skonfigurowanie kolka, nazwy/koloru");

            GrandpaNode.GetComponent<SelectionPopupNodeScript>().ConfigureNodeForCenterPosition();
            Debug.Log("skonfigurowanie go na srodek - wysrodkowanie itp");

            _selectedNode =  GrandpaNode.GetComponent<SelectionPopupNodeScript>();
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
        CurrentOpenedNodeslist = SkillsManager.ROOT_SKILLTREE.Childs;
        //---------------------------------------------------------------------------------
        CurrentDisplayedNodeElements = CurrentOpenedNodeslist.Count;
        // Midddle = przeznaczony dla quit, back,exit ?
        int backnodeIndex = CurrentOpenedNodeslist.Count / 2;
        for (int i = 0; i < CurrentOpenedNodeslist.Count; i++)
        {
            var node = Instantiate(PopupNodeObjectPrefab, this.transform);
            var nodeScript = node.GetComponent<SelectionPopupNodeScript>();

            AttackOptionsNodes.Add(nodeScript);
            node.transform.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2Int(0, -50);
            nodeScript.transform.Rotate(0,0,(i == 0)?180:(180 + (AngleChangeValue * i)));
            nodeScript.SelfConfigure(CurrentOpenedNodeslist[i]);
        }
    }

}
