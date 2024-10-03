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
            while ( enemyClass.ifEndMove == true)
                yield return null;
            enemyClass.ifAttack = true;
        }
        RoundManager.Instance.UpdateGameState(GameState.PlayerTurn);
    }

    IEnumerator PlayerActionCoroutine() 
    {
        Character characterClass;
        
        characterClass = player.GetComponent<Character>();

        characterClass.ifTurn = true;
        characterClass.ifEndMove = true;
        while (characterClass.ifEndMove == true)
            yield return null;
        characterClass.ifAttack = true;
        while(characterClass.ifAttack == true)
            yield return null;
        RoundManager.Instance.UpdateGameState(GameState.EnemyTurn);
    }

    public void DeleteEnemy(GameObject enemy) 
    {

    }
}
