using System;
using System.Collections.Generic;

[Serializable]
public class SkillNode
{
    public string Node_Name;
    public int Node_ID;
    public SkillNode Parent;
    public SkillData Skill = null;

    public List<SkillNode> Childs = new List<SkillNode>();
    public SkillNode this[string childName] =>  Childs[Childs.FindIndex(child=>child.Node_Name == childName)];
    public SkillNode this[int childIndex] =>  Childs[childIndex];
    
    // DETAILS
    public bool IsLocked = false;
    public bool IsReadyToUse = true;
    public bool IsAvaiable = true;

    public SkillNode(int node_ID, string node_Name, SkillNode parent, SkillData _data)
    {
        Node_ID = node_ID;
        Node_Name = node_Name;
        Parent = parent;
        Skill = _data;
    }
  
    public void AddChild((string name, SkillData data) _nodeTitle)
    {
        if(Childs.Count == 0)
        {
            Childs.Add(new SkillNode(Childs.Count,"BACK",parent:this, null));
        }
        Childs.Add(new SkillNode(Childs.Count,_nodeTitle.name,parent:this, _nodeTitle.data));
    }
    public void AddChildsRange(List<(string name,SkillData _data)> _nodeTitleList)
    {
        if(Childs.Count == 0)
            Childs.Add(new SkillNode(Childs.Count,"BACK",parent:this, null));

        foreach(var title in _nodeTitleList)
        {
            Childs.Add(new SkillNode(Childs.Count,title.name,parent:this, title._data));
        }
    }
}