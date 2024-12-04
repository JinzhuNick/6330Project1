using UnityEngine;

public class EndGameStateListener : MonoBehaviour
{

    public GameObject victoryPanel;
    public GameObject defeatPanel;

    private void Awake()
    {
        RoundManager.OnStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        RoundManager.OnStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(RoundManager.GameState newState)
    {
        if (newState == RoundManager.GameState.Victory)
        {
            ShowVictoryUI();
        }
        else if (newState == RoundManager.GameState.Lose)
        {
            ShowDefeatUI();
        }
    }

    private void ShowVictoryUI()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0;
    }

    private void ShowDefeatUI()
    {
        defeatPanel.SetActive(true);
        Time.timeScale = 0;
    }
}