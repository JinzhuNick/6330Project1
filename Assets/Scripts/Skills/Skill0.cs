using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackSkill : Skill
{
    private GridCell targetCell = null;
    public DiceRoll diceRollScript;
    public GameObject dice;

    public NormalAttackSkill()
    {
        skillName = "普通攻击";
        damageMultiplier = 1.0f;
        attackWindup = 0.1f;
        attackWinddown = 1.5f;

        // 掷骰子映射
        diceDamageMapping = new Dictionary<int, float>()
        {
            {1, 0.8f},
            {2, 0.9f},
            {3, 1.0f},
            {4, 1.1f},
            {5, 1.2f}, 
            {6, 1.2f}
        };
    }

    public override void OnSelect(Character character)
    {
        foreach (GridCell cell in affectedCells)
        {
            cell.ClearCellStates();
        }
        //初始情况下不显示任何格子
    }

    public override void OnCancel(Character character)
    {
        // 清除高亮
        if (targetCell != null)
        {
            targetCell.RemoveCellState(CellState.Active);
            targetCell = null;
        }
    }

    public override void UpdateSkill(Character character)
    {
        if (isSkillActivated) return;
        // 获取鼠标相对于角色的方向
        UpdateTargetCell(character);

        // 处理鼠标点击
        if (GameManager.Instance.ifClickable == true && Input.GetMouseButtonDown(0) && targetCell != null)
        {
            // 锁定技能
            isSkillActivated = true;

            // 存储执行时的目标格子
            executionCells.Clear();
            executionCells.Add(targetCell);

            if (GameManager.Instance.useDice == true)
            {
                dice = GameObject.FindGameObjectWithTag("Dice");
                dice.GetComponent<MeshRenderer>().enabled = true;

                diceRollScript = dice.GetComponent<DiceRoll>();

                diceRollScript.StartRoll();
            }
            character.UpdateFacingDirection(character.transform.position, targetCell.GetCenterPosition());

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

    void UpdateTargetCell(Character character)
    {
        if (isSkillActivated)
        {
            return; // 如果技能已激活，停止更新目标格子
        }

        // 清除之前的高亮
        if (targetCell != null)
        {
            targetCell.RemoveCellState(CellState.Active);
            targetCell = null;
        }

        // 获取鼠标相对于角色的方向
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 direction = mouseWorldPosition - character.transform.position;
        direction.y = 0;
        direction.Normalize();

        // 确定方向
        Direction selectedDirection;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            selectedDirection = direction.x > 0 ? Direction.East : Direction.West;
        }
        else
        {
            selectedDirection = direction.z > 0 ? Direction.North : Direction.South;
        }

        // 获取目标格子
        int x = character.currentCell.x;
        int y = character.currentCell.y;
        GridManager gridManager = character.gridManager;

        int dx = 0, dy = 0;
        switch (selectedDirection)
        {
            case Direction.North:
                dy = 1;
                break;
            case Direction.South:
                dy = -1;
                break;
            case Direction.East:
                dx = 1;
                break;
            case Direction.West:
                dx = -1;
                break;
        }

        GridCell cell = gridManager.GetCell(x + dx, y + dy);
        if (cell != null)
        {
            cell.AddCellState(CellState.Active);
            targetCell = cell;
        }

    }

    Vector3 GetMouseWorldPosition()
    {
        // 与Skill2中相同
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
        // 在技能执行期间，高亮锁定的目标格子
        foreach (GridCell cell in executionCells)
        {
            cell.AddCellState(CellState.Active);
        }

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
            // 计算伤害，此处为固定伤害
            finalDamage = Mathf.RoundToInt(character.attackPower * damageMultiplier);
        }

        character.animator.SetTrigger("isAttacking");

        // 对目标造成伤害
        if (targetCell.occupant != null)
        {
            Enemy enemy = targetCell.occupant.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
            }
        }


        // 后摇
        yield return new WaitForSeconds(attackWinddown);

        // 攻击结束
        isSkillActivated = false;
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
