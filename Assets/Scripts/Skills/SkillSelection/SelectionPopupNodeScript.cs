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
        if(node_data.Skill.ReadyToUse == false)
        {
            Debug.Log("skill nie jest gotowy do uzycia");
            return;
        }
        if(node_data.Skill.IsEnoughtResourcesToUse == true)
        {
            var selectionTree = GetComponentInParent<SelectionPopupController>();
            selectionTree.SelectedNode = this;
            EnableSelectionDarkOverlay(true);
        }
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
        if(node_data.Childs.Count > 0)
            CircleMainImage.transform.parent.GetComponent<Image>().sprite = null;

        IsSelected = true;

        GetComponentInChildren<Button>().onClick.AddListener(()=>node_data.Skill.SkillLogic.Select());
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
                Content.GetComponentInChildren<TextMeshProUGUI>().SetText("Back.");
                Content.GetComponent<Button>().onClick.AddListener(()=>GetComponentInParent<SelectionPopupController>().RebuildTree_BackButton(node_data.Parent));
                CircleMainImage.sprite = null;
            }

            if(selectionPopupNode.Parent == null)
            {
                Content.GetComponentInChildren<TextMeshProUGUI>().SetText("End turn.");
                Content.GetComponent<Button>().onClick.AddListener(()=>GameManager.instance.EndPlayerAttackTurn());
                Content.GetComponent<Button>().onClick.AddListener(()=>this.transform.parent.gameObject.SetActive(false));
                GetComponentInParent<SelectionPopupController>().ClearCenteredNode();
                SkillsManager.SelectedAttackSkill=null;
                CircleMainImage.sprite = null;
            }
            return;
        }
        Content.GetComponent<Button>().onClick.AddListener(()=>GetComponentInParent<SelectionPopupNodeScript>().OnClick_Select());
        Content.GetComponentInChildren<TextMeshProUGUI>().SetText(node_data.Node_Name);

        if(node_data.Childs.Count > 0) // bedzie z tego kwadratowy a`la katalog
            CircleMainImage.transform.parent.GetComponent<Image>().sprite = null;

        CircleMainImage.sprite = node_data.Skill.SkillIcon;
        CheckIfSkillIsLocked();
        CheckIfResourcesIsEnough();
        SetAndCheckCooldown();
        CheckIfIsReadyToUse();
    }

    private void CheckIfIsReadyToUse()
    {
       if(node_data.Skill.isObtained)
       {
           if(node_data.Skill.ReadyToUse == false)
           {
               EnableSelectionDarkOverlay(true);
           }
           else
           {
               EnableSelectionDarkOverlay(false);
           }
       }
    }

    private void SetAndCheckCooldown()
    {
        if(node_data.Skill.isObtained && node_data.Skill.isCategoryType == false)
        {
            GetComponentInChildren<SkillRoundProgressBar>().UpdateBar(node_data.Skill.Cooldown-node_data.Skill.CooldownLeftToBeReady, node_data.Skill.Cooldown);
        }
    }

    private void CheckIfResourcesIsEnough()
    {   
        if(node_data.Skill.isObtained && node_data.Skill.isCategoryType == false)
        {
            Debug.Log("sprawdzanie czy stac nas na skila");
            // wskaznikiem  czy sa zasoby bedzie kolor paska łądowania wokół = czwerowny pełny, git, ale nie stac cie na niego xd
            if(node_data.Skill.isLocked)
            {
                GetComponentInChildren<SkillRoundProgressBar>().fillProgressBarIMG.color = Color.black;
                return;
            }

            if(node_data.Skill.IsEnoughtResourcesToUse)
            {
                GetComponentInChildren<SkillRoundProgressBar>().fillProgressBarIMG.color = Color.green;
            }
            else
            {
                GetComponentInChildren<SkillRoundProgressBar>().fillProgressBarIMG.color = Color.red;
            }
        }
    }

    public void CheckIfSkillIsLocked()
    {   
        node_data.Skill.SetLockedStatusBasedOnPlayerLevel();
        
        if(node_data.Skill.isLocked == true)
        {
            Content.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        LockedOverlay.gameObject.SetActive(node_data.Skill.isLocked); 
        
    }
    public void EnableSelectionDarkOverlay(bool value)
    {
        if(SelectedOverlay.gameObject == null)
            return;
            
       // Debug.Log("przelaczenie przyciemnienia:"+value+" dla "+node_data.Node_Name);
        SelectedOverlay.SetActive(value);
    }
}
