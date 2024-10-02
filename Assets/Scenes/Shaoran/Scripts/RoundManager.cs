using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    public GameState State;

    public static event Action<GameState> OnStateChanged;

    private void Awake()
    {
        Instance = this;
    }
    public enum GameState
    {
        PlayerTurn,
        EnemyTurn,
        Victory,
        Lose
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.PlayerTurn:
                Debug.Log("PlayerTurn");
                break;
            case GameState.EnemyTurn:
                Debug.Log("EnemyTurn");
                break;
            case GameState.Victory:
                Debug.Log("Victory");
                break;
            case GameState.Lose:
                Debug.Log("Lose");
                break;
        }

        OnStateChanged?.Invoke(newState);
        Debug.Log("Game State has Changed to: " + newState);
    }
}
