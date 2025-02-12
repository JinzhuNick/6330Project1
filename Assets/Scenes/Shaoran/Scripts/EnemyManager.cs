using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoundManager;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemyGameObjects;
    public GameObject player;

    private void Awake()
    {
        RoundManager.OnStateChanged += RoundManagerOnStateChanged;
        Enemy.onEnemyDeath += OnEnemyDeath;
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        enemyGameObjects.Remove(enemy);
        if (CheckEnemyList())
        {
            RoundManager.Instance.UpdateGameState(GameState.Victory);
            Debug.Log("You Win!");
        }
    }

    private void Start()
    {
        RoundManager.Instance.UpdateGameState(GameState.EnemyTurn);
    }

    private void OnDestroy()
    {
        RoundManager.OnStateChanged -= RoundManagerOnStateChanged;
        Enemy.onEnemyDeath -= OnEnemyDeath;
    }

    private void RoundManagerOnStateChanged(RoundManager.GameState state)
    {
        if (state == RoundManager.GameState.EnemyTurn)
        {
           StartCoroutine(EnemyActionCoroutine());
        }
        if (state == RoundManager.GameState.PlayerTurn)
        {
            StartCoroutine(PlayerActionCoroutine());
        }
    }
    IEnumerator EnemyActionCoroutine() 
    {
        Enemy enemyClass;
        foreach (GameObject enemy in enemyGameObjects)
        {
            enemyClass = enemy.GetComponent<Enemy>();
            enemyClass.ifTurn = true;
            enemyClass.ifEndMove = true;
            Debug.Log("ifTurnTriggered");
            while (enemyClass.ifEndMove == true)
                yield return null;
            enemyClass.ifAttack = true;
            enemyClass.ifEndAttack = true;
            while ( enemyClass.ifEndAttack == true)
                yield return null;

        }
        if (CheckEnemyList())
        {
            RoundManager.Instance.UpdateGameState(GameState.Victory);
            Debug.Log("You Win");
        }
        else
        RoundManager.Instance.UpdateGameState(GameState.PlayerTurn);
    }

    IEnumerator PlayerActionCoroutine() 
    {
        Character characterClass;
        
        characterClass = player.GetComponent<Character>();
        if (CheckPlayerHealth())
        {
            RoundManager.Instance.UpdateGameState(GameState.Lose);
            Debug.Log("You Lose!");
            yield break; 
        }
        characterClass.ifTurn = true;
        characterClass.ifEndMove = true;
        while (characterClass.ifEndMove == true)
            yield return null;
        characterClass.ifAttack = true;
        while(characterClass.ifAttack == true)
            yield return null;
        RoundManager.Instance.UpdateGameState(GameState.EnemyTurn);
    }

    public bool CheckEnemyList() 
    {
        return enemyGameObjects.Count == 0;
    }

    private bool CheckPlayerHealth()
    {
        Character characterClass = player.GetComponent<Character>();
        return characterClass.health <= 0; 
    }

}
