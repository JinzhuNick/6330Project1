using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoundManager;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyGameObjects;
    public GameObject player;

    private void Awake()
    {
        RoundManager.OnStateChanged += RoundManagerOnStateChanged;
    }

    private void Start()
    {
        RoundManager.Instance.UpdateGameState(GameState.EnemyTurn);
    }

    private void OnDestroy()
    {
        RoundManager.OnStateChanged -= RoundManagerOnStateChanged;
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
            Debug.Log("ifTurnTriggered");
            while ( enemyClass.ifEndMove == true)
                yield return null;
        }
        RoundManager.Instance.UpdateGameState(GameState.PlayerTurn);
    }

    IEnumerator PlayerActionCoroutine() 
    {
        Character characterClass;
        
        characterClass = player.GetComponent<Character>();

        characterClass.ifTurn = true;
        while (characterClass.ifEndMove == true)
            yield return null;

        RoundManager.Instance.UpdateGameState(GameState.EnemyTurn);
    }
}
