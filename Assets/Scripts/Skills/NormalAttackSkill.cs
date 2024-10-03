using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackSkill : Skill
{
    private GridCell targetCell = null;

    public NormalAttackSkill()
    {
        skillName = "普通攻击";
        damageMultiplier = 1.0f; // 100%
        attackWindup = 0f;
        attackWinddown = 0.5f;

        // 掷骰子映射
        diceDamageMapping = new Dictionary<int, float>()
        {
            {1, 0.8f}, // 80%
            {2, 0.9f}, // 90%
            {3, 1.0f}, // 100%
            {4, 1.1f}, // 110%
            {5, 1.2f}, // 120%
            {6, 1.3f}  // 130%
        };
    }

    public override void OnSelect(Character character)
    {
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
        // 获取鼠标相对于角色的方向
        UpdateTargetCell(character);

        // 处理鼠标点击
        if (Input.GetMouseButtonDown(0) && targetCell != null)
        {
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
        // 前摇
        yield return new WaitForSeconds(attackWindup);

        // 掷骰子
        int diceResult = Random.Range(1, 7);
        float damageBonus = diceDamageMapping[diceResult];

        // 计算伤害
        int standardDamage = Mathf.RoundToInt(character.attackPower * damageMultiplier);
        int finalDamage = Mathf.RoundToInt(standardDamage * damageBonus);

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
        character.ifAttack = false;
        //character.EndTurn(); // 如果需要，可以结束回合
        OnCancel(character); // 清除高亮
        character.selectedSkill = null;
    }
}
