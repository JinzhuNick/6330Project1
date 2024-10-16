using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    // 在按钮的 OnClick 事件中调用这个方法，参数表示是否使用骰子
    public void OnStartButtonClicked(bool isUseDice)
    {
        // 设置GameManager
        GameManager.Instance.useDice = isUseDice;

        //加载游戏场景
        SceneManager.LoadScene("test");
    }
}