using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // 玩家物体的Transform
    private bool isFollowingPlayer = true; // 摄像机是否跟随玩家
    private Vector3 offset; // 当前摄像机与玩家的偏移量
    private Vector3 initialOffset; // 初始摄像机与玩家的偏移量

    public float panSpeed = 20f; // 平移速度
    public float rotationSpeed = 5f; // 旋转速度
    public float zoomSpeed = 2f; // 缩放速度
    public float minZoom = 5f; // 最小缩放
    public float maxZoom = 20f; // 最大缩放

    private Vector3 lastMousePosition; // 上一帧的鼠标位置

    private void Start()
    {
        if (player != null)
        {
            initialOffset = transform.position - player.position;
            offset = initialOffset;
        }
    }

    private void Update()
    {
        HandlePanning();
        HandleReset();
        HandleRotation();
        HandleZoom();
        HandleFollowPlayer();
    }

    void HandlePanning()
    {
        float h = Input.GetAxis("Horizontal"); // 获取水平轴输入（A/D键）
        float v = Input.GetAxis("Vertical");   // 获取垂直轴输入（W/S键）

        if (h != 0 || v != 0)
        {
            isFollowingPlayer = false;

            // 计算摄像机的右方向和前方向
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            // 忽略Y轴方向
            right.y = 0;
            forward.y = 0;
            right.Normalize();
            forward.Normalize();

            // 计算移动向量
            Vector3 move = (right * h + forward * v) * panSpeed * Time.deltaTime;
            transform.position += move;
        }
    }

    void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isFollowingPlayer = true;
            if (player != null)
            {
                offset = initialOffset; // 重置偏移量为初始值
                transform.position = player.position + offset; // 更新摄像机位置
            }
        }
    }

    void HandleFollowPlayer()
    {
        if (isFollowingPlayer && player != null)
        {
            transform.position = player.position + offset;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            float angle = delta.x * rotationSpeed * Time.deltaTime;

            // 确定旋转的中心点
            Vector3 pivotPoint;
            if (isFollowingPlayer && player != null)
            {
                pivotPoint = player.position;
            }
            else
            {
                // 获取屏幕中心对应的世界坐标
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float enter;
                if (groundPlane.Raycast(ray, out enter))
                {
                    pivotPoint = ray.GetPoint(enter);
                }
                else
                {
                    pivotPoint = Vector3.zero;
                }
            }

            // 围绕中心点旋转摄像机
            transform.RotateAround(pivotPoint, Vector3.up, angle);

            // 保持俯视角度（x轴旋转45度）
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.x = 45f;
            transform.eulerAngles = eulerAngles;

            // 更新偏移量
            if (isFollowingPlayer && player != null)
            {
                offset = transform.position - player.position;
            }
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            Camera.main.orthographicSize -= scroll * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }
    }
}