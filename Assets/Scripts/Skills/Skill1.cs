using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1 : Skill
{
    public DiceRoll diceRollScript;
    public GameObject dice;


    public Skill1()
    {
        skillName = "技能1";
        damageMultiplier = 0.6f;
        attackWindup = 0.1f;
        attackWinddown = 0.5f;

        diceDamageMapping = new Dictionary<int, float>()
        {
            {1, 0.9f},
            {2, 0.9f},
            {3, 1.0f},
            {4, 1.0f},
            {5, 1.1f},
            {6, 1.1f}
        };
    }

    public override void OnSelect(Character character)
    {
        foreach (GridCell cell in affectedCells)
        {
            cell.ClearCellStates();
        }
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
        if (GameManager.Instance.ifClickable == true && Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.useDice == true)
            {
                dice = GameObject.FindGameObjectWithTag("Dice");
                dice.GetComponent<MeshRenderer>().enabled = true;

                diceRollScript = dice.GetComponent<DiceRoll>();

                diceRollScript.StartRoll();
            }
            // 开始执行技能
            character.StartCoroutine(ExecuteSkill(character));

            
        }
    }

    protected override IEnumerator ExecuteSkill(Character character)
    {
        GameManager.Instance.ifClickable = false;
        int finalDamage = 0;
        // 前摇
        yield return new WaitForSeconds(attackWindup);

        if(GameManager.Instance.useDice == true)
        {
            // 掷骰子
            // int diceResult = Random.Range(1, 7);
            dice = GameObject.FindGameObjectWithTag("Dice");
            diceRollScript = dice.GetComponent<DiceRoll>();
            while (diceRollScript.isRolling)
            {
                yield return null;

            }

            float damageBonus = diceDamageMapping[diceRollScript.finalFaceValue];

            // 计算伤害
            int standardDamage = Mathf.RoundToInt(character.attackPower * damageMultiplier);
            finalDamage = Mathf.RoundToInt(standardDamage * damageBonus);
        }
        else
        {
            finalDamage = Mathf.RoundToInt(character.attackPower * damageMultiplier);
        }
        

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
        if (GameManager.Instance.useDice == true)
        {
            dice.GetComponent<MeshRenderer>().enabled = false;
        }
        GameManager.Instance.ifClickable = true;
    }
}
