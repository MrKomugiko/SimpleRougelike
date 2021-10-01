using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackSelectionPopupNodeScript : MonoBehaviour
{
    public SelectionPopupNode node_data;
    public Image CircleMainColor;
    public GameObject Content;
    public GameObject RoundProgressBarContainer;

    public void AdjustImageRotation()
    {
        // counter-rotate
        Content.transform.Rotate(0,0,-this.transform.localEulerAngles.z);
    }
    public void HideProgressBar()
    {
        // counter-rotate
        RoundProgressBarContainer.SetActive(false);
    }

    public void OnClick_Select()
    {
       var selectionTree = GetComponentInParent<AttackSelectionPopupController>();
       selectionTree.SelectedNode = this;
    }

    [SerializeField] private bool IsSelected = false;
    public void ConfigureNodeForCenterPosition()
    {
        this.gameObject.name = "centeredNode";
        Debug.Log("centrowanie nowego noda");
        var _rect = GetComponent<RectTransform>();
        _rect.anchorMin = new Vector2(.5f,.5f);
        _rect.anchorMax = new Vector2(.5f,.5f);
        _rect.pivot = new Vector2(.5f,.5f);

        _rect.sizeDelta = new Vector2(120,120);
        _rect.localPosition = Vector3.zero;

        GetComponentInChildren<Button>().onClick.RemoveAllListeners();

        IsSelected = true;
    }

    internal void SelfConfigure(SelectionPopupNode selectionPopupNode)
    {
        node_data = selectionPopupNode;
        AdjustImageRotation();
        gameObject.name = node_data.Node_Name;

        if(selectionPopupNode.Childs.Count > 0)
            HideProgressBar();

        if(node_data.Node_ID == 0) // back / exit button
        {
            CircleMainColor.color = Color.gray;
            if(selectionPopupNode.Childs.Count == 0)
            {
                 HideProgressBar();
                Content.GetComponentInChildren<TextMeshProUGUI>().SetText("BACK");
                Content.GetComponent<Button>().onClick.AddListener(()=>GetComponentInParent<AttackSelectionPopupController>().RebuildTree_BackButton(node_data.Parent));
            }

            if(selectionPopupNode.Parent == null)
            {
                Content.GetComponentInChildren<TextMeshProUGUI>().SetText("EXIT");
                Content.GetComponent<Button>().onClick.AddListener(()=>this.transform.parent.gameObject.SetActive(false));
            }
            return;
        }
        Content.GetComponent<Button>().onClick.AddListener(()=>GetComponentInParent<AttackSelectionPopupNodeScript>().OnClick_Select());
        Content.GetComponentInChildren<TextMeshProUGUI>().SetText(node_data.Node_Name);
    }
}
