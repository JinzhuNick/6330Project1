using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool useDice = false; // 记录玩家的选择

    private void Awake()
    {
        //GameManager 实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 不销毁
        }
        else
        {
            Destroy(gameObject); // 如果已经存在一个实例，销毁新的
        }
    }
}