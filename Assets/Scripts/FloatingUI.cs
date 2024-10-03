using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float floatAmplitude = 0.1f; // 漂浮的幅度
    public float floatFrequency = 1f;   // 漂浮的频率

    private Vector3 initialPosition;

    void Start()
    {
        // 记录初始位置
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // 计算新的位置
        float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = initialPosition + new Vector3(0, yOffset, 0);
    }
}