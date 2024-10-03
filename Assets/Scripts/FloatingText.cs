using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;      // 漂浮速度
    public float fadeDuration = 1f;    // 淡出持续时间
    public Vector3 offset = new Vector3(0, 2f, 0);   // 初始偏移
    public Vector3 randomOffset = new Vector3(0.1f, 0.1f, 0);

    private float timer = 0f;
    private TextMeshPro textMesh;
    private Color initialColor;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        initialColor = textMesh.color;

        // 添加随机偏移，使效果更自然
        transform.localPosition += offset;
        transform.localPosition += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );
    }

    void Update()
    {
        // 向上漂浮
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // 淡出
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(initialColor.a, 0, timer / fadeDuration);
        textMesh.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

        // 到达持续时间后销毁对象
        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }

    // 设置显示的文本
    public void SetText(string text)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
        textMesh.text = text;
    }
}