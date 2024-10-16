using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill2 : Skill

{
    private Direction selectedDirection = Direction.North;
    public DiceRoll diceRollScript;
    public GameObject dice;
    public Skill2()
    {
        skillName = "技能2";
        damageMultiplier = 0.8f;
        attackWindup = 0.1f;
        attackWinddown = 0.5f;

        diceDamageMapping = new Dictionary<int, float>()
        {
            {1, 0.7f},
            {2, 0.9f},
            {3, 1.0f},
            {4, 1.1f},
            {5, 1.2f},
            {6, 1.3f}
        };
    }

    public override void OnSelect(Character character)
    {
        foreach (GridCell cell in affectedCells)
        {
            cell.ClearCellStates();
        }
        // 初始化，等待玩家选择方向
        UpdateAffectedCells(character);
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
        // 根据鼠标位置更新攻击方向和范围
        UpdateAffectedCells(character);

        // 处理鼠标点击
        if (GameManager.Instance.ifClickable == true && Input.GetMouseButtonDown(0) && affectedCells.Count > 0)
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

        // 处理取消攻击
        if (Input.GetKeyDown(KeyCode.X))
        {
            OnCancel(character);
            character.selectedSkill = null;
        }
    }


    void UpdateAffectedCells(Character character)
    {
        // 清除之前的高亮
        foreach (GridCell cell in affectedCells)
        {
            cell.RemoveCellState(CellState.Active);
        }
        affectedCells.Clear();

        // 获取鼠标相对于角色的方向
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 direction = mouseWorldPosition - character.transform.position;
        direction.y = 0;
        direction.Normalize();

        // 确定方向
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            selectedDirection = direction.x > 0 ? Direction.East : Direction.West;
        }
        else
        {
            selectedDirection = direction.z > 0 ? Direction.North : Direction.South;
        }

        // 更新攻击范围
        int x = character.currentCell.x;
        int y = character.currentCell.y;
        GridManager gridManager = character.gridManager;

        for (int i = 1; i <= 2; i++)
        {
            int dx = 0, dy = 0;
            switch (selectedDirection)
            {
                case Direction.North:
                    dy = i;
                    break;
                case Direction.South:
                    dy = -i;
                    break;
                case Direction.East:
                    dx = i;
                    break;
                case Direction.West:
                    dx = -i;
                    break;
            }

            GridCell cell = gridManager.GetCell(x + dx, y + dy);
            if (cell != null)
            {
                cell.AddCellState(CellState.Active);
                affectedCells.Add(cell);
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            return ray.GetPoint(rayDistance);
        }
        return Vector3.zero;
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
            if (cell.occupant != null)
            {
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
        if(GameManager.Instance.useDice == true)
        {
            dice.GetComponent<MeshRenderer>().enabled = false;
        }
        GameManager.Instance.ifClickable = true;
    }
}
