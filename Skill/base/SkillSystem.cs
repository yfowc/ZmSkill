﻿using System.Collections;
using System.Collections.Generic;
using CSScriptLibrary;
using UnityEngine;

/// <summary>
/// 操作码包装类
/// </summary>
public class OpCodeObject
{
    public OpCodeObject(OpCode opCode, params float[] values)
    {
        this.opCode = opCode;
        this.values = values;
    }
    public OpCode opCode;
    public float[] values;
}

/// <summary>
/// 技能系统(每个对象都拥有属于自己的技能系统)
/// </summary>
public class SkillSystem : MonoBehaviour
{

    /// <summary>
    /// 所拥有的技能ID列表
    /// </summary>
    public List<int> _skillIdList;

    /// <summary>
    /// 所拥有的技能对象列表
    /// </summary>
    public Dictionary<int, SkillBase> _skillList;

    

    public List<int> shortCutList;

    /// <summary>
    /// 当前激活中的技能
    /// 注意:激活中指释放中
    /// 如果是buff类型技能，buff持续时间不算做激活中
    /// 释放buff技能的过程算作激活
    /// buff的实现另外编写逻辑
    /// </summary>
    public int currentId = -1;
    Role role;
    /// <summary>
    /// 操作码集合
    /// </summary>
    public Queue<OpCode> opCodes = new Queue<OpCode>();

    Animator animator;

    /// <summary>
    /// 被禁止的技能列表
    /// </summary>
    public ArrayList NotningSkillLIst;

    public Dictionary<int, SkillBase> SkillMap
    {
        get
        {
            if (_skillList == null)
            {
                _skillList = new Dictionary<int, SkillBase>();
            }
            return _skillList;
        }
    }

    private void Awake()
    {
        role = GetComponent<Role>();
        animator = GetComponent<Animator>();
        NotningSkillLIst = new ArrayList();
        string path = Application.streamingAssetsPath + @"\skillconfig\RoleSkillInit.cs";
        dynamic skillList = CSScript.Evaluator.LoadFile(path);
        _skillIdList = skillList.skillInitMap[role.id];
    }

    // Use this for initialization
    void Start()
    {
        foreach (int skillId in _skillIdList)
        {
            SkillBase skill = SkillManager.Instance.GetSkillById(skillId);
            if (skill != null)
            {
                skill.Init(role);
                SkillMap.Add(skill.GetId(), skill);
            }
        }
        
    }

    /// <summary>
    /// 自动自动发动被动技能效果
    /// </summary>
    void FixedUpdate()
    {
        SkillStart();
    }

    /// <summary>
    /// 技能效果携程
    /// </summary>
    /// <returns></returns>
    public void SkillStart()
    {
        foreach (int skillId in _skillIdList)
        {
            SkillBase skill = GetSkillById(skillId);
            if (skill == null)
            {
                Debug.LogError("无效技能");
                continue;
            }
            skill.Effect_Factory();
        }
    }

    /// <summary>
    /// 技能装载
    /// </summary>
    private void SkillLoad()
    {

    }

    public void AddOp(OpCode opCode, params float[] values)
    {
        AddOp(opCode, null, values);
    }

    /// <summary>
    /// 追加操作(已有操作不重复添加)
    /// </summary>
    /// <param name="opCode"></param>
    public void AddOp(OpCode opCode, Role role, params float[] values)
    {
        ////有窗口打开的状态下不接受指令输入
        //if (DevDogOpenManagers.isOpen)
        //{
        //    return;
        //}
        //被攻击时不接受操作
        if (animator)
        {
            
        }
        foreach (int skillId in _skillIdList)
        {
            SkillBase skill = GetSkillById(skillId);
            skill.opEffect(opCode, role, values);
        }


    }

    /// <summary>
    /// 通过技能ID获取技能实例
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public SkillBase GetSkillById(int skillId)
    {
        return SkillMap[skillId];
    }

    public void Use(int skillId)
    {
        SkillBase skill = GetSkillById(skillId);
        if (skill != null)
        {
            Debug.Log(skill.GetName());
            skill.Use();
        }
    }

    /// <summary>
    /// 清除所有技能事件，打断所有技能
    /// </summary>
    public void ClearEvent()
    {
        foreach (SkillBase skill in SkillMap.Values)
        {
            skill.ClearEvent();
        }
    }

    //清除指定的技能
    public void RemoveEvent(int skillId)
    {
        foreach (SkillBase skill in SkillMap.Values)
        {
            if (skillId == skill.GetId())
            {
                skill.ClearEvent();
                return;
            }
            
        }
    }

    /// <summary>
    /// 添加禁止使用的技能列表
    /// 置于此列表中的技能会立即失效，且无法发动,被动技能也无法幸免
    /// </summary>
    public void AddNotningSkill(int skillId)
    {
        NotningSkillLIst.Add(skillId);
    }

    public void DeleteNotingSkill(int skillId)
    {
        NotningSkillLIst.Remove(skillId);
    }


}
