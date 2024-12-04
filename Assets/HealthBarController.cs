using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    //public Transform cameraTransform;
    // Start is called before the first frame update

    public Transform cameraTransform;
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            cameraTransform.eulerAngles.y,
            transform.eulerAngles.z
        );
    }
}
