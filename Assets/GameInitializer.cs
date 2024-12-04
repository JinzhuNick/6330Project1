using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();

            // 确保它不被销毁
            DontDestroyOnLoad(gameManager);
        }
    }
}
