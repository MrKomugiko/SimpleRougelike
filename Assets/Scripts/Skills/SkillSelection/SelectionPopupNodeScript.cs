using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPopupNodeScript : MonoBehaviour
{
    public SkillNode node_data;
    public Image CircleMainImage;
    public GameObject Content;
    public GameObject RoundProgressBarContainer;

    [SerializeField] private GameObject LockedOverlay;
    [SerializeField] private GameObject SelectedOverlay;

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
        var selectionTree = GetComponentInParent<SelectionPopupController>();
        selectionTree.SelectedNode = this;
        EnableSelectionDarkOverlay(true);
    }

    [SerializeField] private bool IsSelected = false;
    public void ConfigureNodeForCenterPosition()
    {
       // EnableSelectionDarkOverlay(false);
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

    internal void SelfConfigure(SkillNode selectionPopupNode)
    {
        node_data = selectionPopupNode;
        AdjustImageRotation();
        gameObject.name = node_data.Node_Name;

        if(selectionPopupNode.Childs.Count > 0)
            HideProgressBar();

        if(node_data.Node_ID == 0) // back / exit button
        {
            CircleMainImage.color = Color.gray;
            if(selectionPopupNode.Childs.Count == 0)
            {
                 HideProgressBar();
                Content.GetComponentInChildren<TextMeshProUGUI>().SetText("BACK");
                Content.GetComponent<Button>().onClick.AddListener(()=>GetComponentInParent<SelectionPopupController>().RebuildTree_BackButton(node_data.Parent));
                CircleMainImage.sprite = null;
            }

            if(selectionPopupNode.Parent == null)
            {
                Content.GetComponentInChildren<TextMeshProUGUI>().SetText("EXIT");
                Content.GetComponent<Button>().onClick.AddListener(()=>this.transform.parent.gameObject.SetActive(false));
                CircleMainImage.sprite = null;
            }
            return;
        }
        Content.GetComponent<Button>().onClick.AddListener(()=>GetComponentInParent<SelectionPopupNodeScript>().OnClick_Select());
        Content.GetComponentInChildren<TextMeshProUGUI>().SetText(node_data.Node_Name);

        CircleMainImage.sprite = node_data.Skill.SkillIcon;
        CheckIfSkillIsLocked();
    }
    public void CheckIfSkillIsLocked()
    {
        if(node_data.IsLocked)
        {
            Content.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        LockedOverlay.gameObject.SetActive(node_data.IsLocked); 
    }

    public void EnableSelectionDarkOverlay(bool value)
    {
        if(SelectedOverlay.gameObject == null)
            return;
            
        Debug.Log("przelaczenie przyciemnienia:"+value+" dla "+node_data.Node_Name);
        SelectedOverlay.SetActive(value);
    }
}
