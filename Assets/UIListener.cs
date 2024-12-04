using UnityEngine;
using UnityEngine.UI;

public class TurnUIController : MonoBehaviour
{
    public Text turnStatusText; 
    public Text turnNumberText; 

    private int turnNumber = 0; 

    private void Awake()
    {
        turnNumberText.text = "Turn\n1";
        RoundManager.OnStateChanged += OnGameStateChanged;

    }

    private void OnDestroy()
    {
        RoundManager.OnStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(RoundManager.GameState newState)
    {
        switch (newState)
        {
            case RoundManager.GameState.PlayerTurn:
                UpdateTurnStatus("Player Turn");
                break;
            case RoundManager.GameState.EnemyTurn:
                UpdateTurnStatus("Enemy Turn");
                break;
        }

        if (newState == RoundManager.GameState.PlayerTurn)
        {
            IncrementTurnNumber();
        }
    }

    private void UpdateTurnStatus(string status)
    {
        if (turnStatusText != null)
        {
            turnStatusText.text = status;
        }
    }

    private void IncrementTurnNumber()
    {
        turnNumber++;
        if (turnNumberText != null)
        {
        turnNumberText.text = $"Turn\n{turnNumber}"; 
        }
    }
}