using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject mainCameraGameObject;
    public Camera mainCamera;
    public float cameraDepth;
    public LayerMask layerMask;
    public bool isSelected;

    public void Awake()
    {
        Initialize();
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && isSelected) 
        {
            MovePlayer();
            isSelected = false;
        }
    }

    public void Initialize() 
    {
        mainCamera = Camera.main;
        cameraDepth = mainCamera.WorldToScreenPoint(transform.position).z;
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.y = 1f;
        return vec;
    }
    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera WorldCamera)
    {
        Vector3 worldPosition = WorldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    private async void OnMouseDown()
    {
        if (!isSelected) 
        {
            await Task.Delay(1);
            isSelected = true;
        }
    }

    private void MovePlayer()
    {
        this.GetComponent<Rigidbody>().isKinematic = true;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            transform.position = new Vector3(raycastHit.point.x, 1f, raycastHit.point.z);
        }
    }
}
