using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    public float rollDuration = 2.0f;  // 骰子旋转的持续时间
    public Vector3 rotationSpeed;  // 设置骰子旋转速度
    public float rotationMultiplier = 5.0f;  // 旋转速度倍率

    public bool isRolling = false;
    private float rollTime = 0;
    public int finalFaceValue = 1;  // 骰子最终朝上的面值

    // 预设每个骰子面对应的旋转角度
    private Quaternion[] diceFaces = new Quaternion[6];

    void Start()
    {
        // 初始化每个面的旋转角度（假设初始状态是 Z 轴向上）
        diceFaces[1] = Quaternion.Euler(0, 0, 0);      // 2点面向上
        diceFaces[3] = Quaternion.Euler(0, 0, 90);     // 4点面向上
        diceFaces[4] = Quaternion.Euler(0, 0, 180);    // 5点面向上
        diceFaces[2] = Quaternion.Euler(0, 0, -90);    // 3点面向上
        diceFaces[5] = Quaternion.Euler(90, 0, 0);     // 6点面向上
        diceFaces[0] = Quaternion.Euler(-90, 0, 0);    // 1点面向上
    }

    void Update()
    {
      

        if (isRolling)
        {
            // 持续旋转骰子，并根据旋转时间逐渐减速
            float rotationFactor = Mathf.Lerp(rotationMultiplier, 0, rollTime / rollDuration);
            transform.Rotate(rotationSpeed * rotationFactor * Time.deltaTime);

            rollTime += Time.deltaTime;
            if (rollTime >= rollDuration)
            {
                isRolling = false;
                // 当旋转结束时，让骰子随机落在某个面上
                SnapToFinalRotation();
            }
           
        }
    }

    // 开始掷骰子
    public void StartRoll()
    {
        isRolling = true;
        rollTime = 0;
        // 随机化初始旋转速度
        rotationSpeed = new Vector3(
            Random.Range(200, 400),
            Random.Range(200, 400),
            Random.Range(200, 400)
        );
    }

    // 将骰子锁定到随机的最终面
    void  SnapToFinalRotation()
    {
        // 随机选择一个面
        int randomFaceIndex = Random.Range(0, diceFaces.Length);
        // 将骰子的旋转角度设置为该面对应的角度
        transform.rotation = diceFaces[randomFaceIndex];
        // 保存最终面朝上的值（1到6）
        finalFaceValue = randomFaceIndex + 1;  // 骰子朝上的面值，1-6
        
        
    }
    
}

