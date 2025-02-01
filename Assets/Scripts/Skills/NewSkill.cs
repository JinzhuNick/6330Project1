using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Blunt,
    Sharp,
    Fire,
    Frost
}

public class NewSkill : Skill
{
    private GridCell targetCell = null;
    public DiceRoll diceRollScript;
    public GameObject dice;

    // ÐÂÔöÉËº¦ÀàÐÍ
    public float bluntDamageMultiplier = 0.5f; // ¶ÛÆ÷ÉËº¦ÏµÊý
    public float sharpDamageMultiplier = 0.5f; // ÀûÆ÷ÉËº¦ÏµÊý
    public float fireDamage = 0.0f; // »ðÑæÉËº¦
    public float frostDamage = 0.0f; // ±ùËªÉËº¦

    public NewSkill()
    {
        skillName = "ÆÕÍ¨¹¥»÷";
        damageMultiplier = 1.0f;
        attackWindup = 0.1f;
        attackWinddown = 1.5f;

        // ÖÀ÷»×ÓÓ³Éä
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

    public override void UpdateSkill(Character character)
    {
        if (isSkillActivated) return;
        //UpdateTargetCell(character);

        if (GameManager.Instance.ifClickable && Input.GetMouseButtonDown(0) && targetCell != null)
        {
            isSkillActivated = true;
            executionCells.Clear();
            executionCells.Add(targetCell);

            if (GameManager.Instance.useDice)
            {
                dice = GameObject.FindGameObjectWithTag("Dice");
                dice.GetComponent<MeshRenderer>().enabled = true;
                diceRollScript = dice.GetComponent<DiceRoll>();
                diceRollScript.StartRoll();
            }
            character.UpdateFacingDirection(character.transform.position, targetCell.GetCenterPosition());
            character.StartCoroutine(ExecuteSkill(character));
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            OnCancel(character);
            character.selectedSkill = null;
        }
    }

    protected override IEnumerator ExecuteSkill(Character character)
    {
        foreach (GridCell cell in executionCells)
        {
            cell.AddCellState(CellState.Active);
        }

        GameManager.Instance.ifClickable = false;
        int finalDamage = 0;
        float damageBonus = 1.0f;

        yield return new WaitForSeconds(attackWindup);

        if (GameManager.Instance.useDice)
        {
            dice = GameObject.FindGameObjectWithTag("Dice");
            diceRollScript = dice.GetComponent<DiceRoll>();
            while (diceRollScript.isRolling)
            {
                yield return null;
            }
            damageBonus = diceDamageMapping[diceRollScript.finalFaceValue];
        }

        int baseDamage = Mathf.RoundToInt(character.attackPower * damageMultiplier);
        int bluntDamage = Mathf.RoundToInt(baseDamage * bluntDamageMultiplier * damageBonus);
        int sharpDamage = Mathf.RoundToInt(baseDamage * sharpDamageMultiplier * damageBonus);
        int fire = Mathf.RoundToInt(fireDamage * damageBonus);
        int frost = Mathf.RoundToInt(frostDamage * damageBonus);

        finalDamage = bluntDamage + sharpDamage + fire + frost;
        character.animator.SetTrigger("isAttacking");

        if (targetCell.occupant != null)
        {
            Enemy enemy = targetCell.occupant.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
               // enemy.ApplyElementalEffect(fire, frost);
            }
        }

        yield return new WaitForSeconds(attackWinddown);
        isSkillActivated = false;
        character.ifAttack = false;
        OnCancel(character);
        character.selectedSkill = null;
        if (GameManager.Instance.useDice)
        {
            dice.GetComponent<MeshRenderer>().enabled = false;
        }
        GameManager.Instance.ifClickable = true;
    }

    public override void OnSelect(Character character)
    {
        throw new System.NotImplementedException();
    }

    public override void OnCancel(Character character)
    {
        throw new System.NotImplementedException();
    }
}
