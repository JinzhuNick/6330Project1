using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPUIControler : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // 记录初始旋转
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 每帧将旋转重置为初始值
        transform.rotation = initialRotation;
    }
}
