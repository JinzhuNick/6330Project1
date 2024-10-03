using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1 : Skill
{
    public Skill1()
    {
        skillName = "技能1";
        damageMultiplier = 0.8f; // 80%
        attackWindup = 0.5f;
        attackWinddown = 0.5f;

        diceDamageMapping = new Dictionary<int, float>()
        {
            {1, 0.7f},
            {2, 0.85f},
            {3, 1.0f},
            {4, 1.15f},
            {5, 1.3f},
            {6, 1.5f}
        };
    }

    public override void OnSelect(Character character)
    {
        // 显示周围一圈的格子（包括斜方向）
        int x = character.currentCell.x;
        int y = character.currentCell.y;
        GridManager gridManager = character.gridManager;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                GridCell cell = gridManager.GetCell(x + dx, y + dy);
                if (cell != null)
                {
                    cell.AddCellState(CellState.Active);
                    affectedCells.Add(cell);
                }
            }
        }
    }

    public override void OnCancel(Character character)
    {
        // 清除高亮
        foreach (GridCell cell in affectedCells)
        {
            cell.RemoveCellState(CellState.Active);
        }
        affectedCells.Clear();
    }

    public override void UpdateSkill(Character character)
    {
        // 技能范围已固定，不需要动态更新

        // 处理鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            // 开始执行技能
            character.StartCoroutine(ExecuteSkill(character));
            
        }
    }

    protected override IEnumerator ExecuteSkill(Character character)
    {
        // 前摇
        yield return new WaitForSeconds(attackWindup);

        // 掷骰子
        int diceResult = Random.Range(1, 7);
        float damageBonus = diceDamageMapping[diceResult];

        // 计算伤害
        int standardDamage = Mathf.RoundToInt(character.attackPower * damageMultiplier);
        int finalDamage = Mathf.RoundToInt(standardDamage * damageBonus);

        // 对范围内的敌人造成伤害
        foreach (GridCell cell in affectedCells)
        {
            Debug.Log("检测到格子");
            if (cell.occupant != null)
            {
                Debug.Log("有敌人");
                Enemy enemy = cell.occupant.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(finalDamage);
                }
            }
        }

        // 后摇
        yield return new WaitForSeconds(attackWinddown);

        // 攻击结束
        character.ifAttack = false;
        //character.EndTurn(); // 如果需要，可以结束回合
        OnCancel(character); // 清除高亮
        character.selectedSkill = null;
    }
}
