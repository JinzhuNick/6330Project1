using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    public string skillName;
    public float damageMultiplier;  // 攻击力加成（百分比，例如 1.0f 表示 100%）
    public float attackWindup = 0.5f;   // 前摇时间
    public float attackWinddown = 0.5f; // 后摇时间
    public Dictionary<int, float> diceDamageMapping;  // 掷骰子结果与伤害加成的映射

    protected List<GridCell> affectedCells = new List<GridCell>();

    // 表示技能是否已被激活（已点击）
    protected bool isSkillActivated = false;

    // 存储技能执行时的目标格子
    protected List<GridCell> executionCells = new List<GridCell>();

    // 当技能被选择时调用
    public abstract void OnSelect(Character character);

    // 当技能被取消时调用
    public abstract void OnCancel(Character character);

    // 在技能选择后，每帧更新调用
    public abstract void UpdateSkill(Character character);

    // 执行技能
    protected abstract IEnumerator ExecuteSkill(Character character);
}
