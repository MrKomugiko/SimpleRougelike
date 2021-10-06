using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class SelectionPopupController : MonoBehaviour
{   
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
                    List<SkillNode> enabledSkillsList = new List<SkillNode>();
                    foreach(var skill in value.node_data.Childs)
                    {
                        if(skill.Skill == null)
                        {
                            enabledSkillsList.Add(skill);
                        }
                        else if(skill.Skill.CheckRequirmentsToEnableSkill())
                            enabledSkillsList.Add(skill);
                    }
                    CurrentOpenedNodeslist = enabledSkillsList;

                    foreach(var node in AttackOptionsNodes)
                    {
                        Destroy(node.gameObject);
                    }
                    RebuildTree(CurrentOpenedNodeslist);
                    return;
                }
            }
        }
    }

    private void DuplicateSelectedNodeAndCenter()
    {
       var currentCenterNode = CenterTreeObject.GetComponentInChildren<SelectionPopupNodeScript>();
  
        if(currentCenterNode != null)
            Destroy(currentCenterNode.gameObject);

        var copy = Instantiate(SelectedNode.gameObject,CenterTreeObject.transform);
        _selectedNode = copy.GetComponent<SelectionPopupNodeScript>();
        _selectedNode.ConfigureNodeForCenterPosition();
    }

    public void RebuildTree(List<SkillNode> skillList)
    {
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();
    
        int backnodeIndex = skillList.Count / 2;

        int index = 0;
        foreach(var skill in skillList)
        {
            var node = Instantiate(PopupNodeObjectPrefab,this.transform);
            var nodeScript = node.GetComponent<SelectionPopupNodeScript>();
           
            AttackOptionsNodes.Add(nodeScript);
            node.transform.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2Int(0, -50);
            nodeScript.transform.Rotate(0,0,(index == 0)?180:(180 + ((360/skillList.Count) * index)));
            nodeScript.SelfConfigure(skill);
         
            index++;
        }
    }

    internal void ClearCenteredNode()
    {
        if(CenterTreeObject.transform.childCount == 1 )
        {
            Destroy(CenterTreeObject.transform.GetChild(0).gameObject);
        }
        _selectedNode = null;
    }

    public void RebuildTree_BackButton(SkillNode parentNode)
    {
        if(parentNode == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();

        List<SkillNode> enabledSkillsList = new List<SkillNode>();
        if(parentNode.Parent == null)
        {
            foreach(var skill in SkillsManager.ROOT_SKILLTREE.Childs)
            {
                if(skill.Skill != null)
                {
                    if(skill.Skill.CheckRequirmentsToEnableSkill())
                        enabledSkillsList.Add(skill);
                }
                else
                {
                    enabledSkillsList.Add(skill);
                }
            }
        }
        else
        {  
            foreach(var skill in parentNode.Parent.Childs)
            {
                if(skill.Skill.CheckRequirmentsToEnableSkill())
                    enabledSkillsList.Add(skill);
            }
        }
        CurrentOpenedNodeslist = enabledSkillsList;

        var currentCenterNode = CenterTreeObject.GetComponentInChildren<SelectionPopupNodeScript>();
        if(parentNode.Parent != null)
        {
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);

            var GrandpaNode = Instantiate(PopupNodeObjectPrefab, CenterTreeObject.transform);
            GrandpaNode.GetComponent<SelectionPopupNodeScript>().SelfConfigure(parentNode.Parent);
            GrandpaNode.GetComponent<SelectionPopupNodeScript>().ConfigureNodeForCenterPosition();
            _selectedNode =  GrandpaNode.GetComponent<SelectionPopupNodeScript>();
        }
        else
        {
            if(currentCenterNode != null)
                Destroy(currentCenterNode.gameObject);
        }

        RebuildTree(CurrentOpenedNodeslist);    
    }

    public void FixRotations()
    {
        foreach (var node in AttackOptionsNodes)
        {
            node.AdjustImageRotation();
        }
    }

    public void OPENandSpawnInitNodesTree()
    {
        this.gameObject.SetActive(true);
        foreach (var node in AttackOptionsNodes)
        {
            DestroyImmediate(node.gameObject);
        }
        AttackOptionsNodes.Clear();
        List<SkillNode> enabledSkillsList = new List<SkillNode>();
        foreach(var skill in SkillsManager.ROOT_SKILLTREE.Childs)
        {
            if(skill.Skill == null) {
                // kategorie i eexit bez przypisanego skila
                 enabledSkillsList.Add(skill);
                 continue;
            }
            if(skill.Skill.CheckRequirmentsToEnableSkill())
                enabledSkillsList.Add(skill);
        }
        CurrentOpenedNodeslist = enabledSkillsList;
        int backnodeIndex = enabledSkillsList.Count / 2;
        int index = 0;
        foreach(var skill in enabledSkillsList)
        {
            var node = Instantiate(PopupNodeObjectPrefab, this.transform);
            var nodeScript = node.GetComponent<SelectionPopupNodeScript>();

            AttackOptionsNodes.Add(nodeScript);
            node.transform.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2Int(0, -50);
            nodeScript.transform.Rotate(0,0,(index == 0)?180:(180 + ((360/enabledSkillsList.Count) * index)));
            nodeScript.SelfConfigure(skill);
            index++;
        }
    }
}
