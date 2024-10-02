using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Character : MonoBehaviour
{
    public int attackPower;
    public int maxHealth;
    public int health;
    public int movementRange;
    public float speed;  // 控制移动速度

    public bool ifTurn = false;  // 控制角色是否可以移动

    private GridManager gridManager;
    public GridCell currentCell;

    private List<GridCell> reachableCells = new List<GridCell>();  // 可到达的格子列表
    private List<GridCell> path = new List<GridCell>();  // 角色当前的移动路径

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        // 初始化角色所在的格子
        Vector3 position = transform.position;
        int x = Mathf.FloorToInt(position.x / gridManager.cellSize + 0.5f);
        int y = Mathf.FloorToInt(position.z / gridManager.cellSize + 0.5f);

        currentCell = gridManager.GetCell(x, y);
        if (currentCell != null)
        {
            currentCell.occupant = gameObject;
            transform.position = currentCell.GetCenterPosition();
        }
        else
        {
            Debug.LogError("角色初始化位置不在任何格子上！");
        }
    }

    void Update()
    {
        if (ifTurn)
        {
            // 高亮可移动的格子
            HighlightReachableCells();

            // 显示敌人的索敌范围
            ShowEnemyDetectionRanges();

            // 处理鼠标输入
            HandleMouseInput();
        }
    }

    // 高亮可到达的格子
    void HighlightReachableCells()
    {
        // 清除之前的高亮
        foreach (GridCell cell in reachableCells)
        {
            cell.cellState = CellState.Normal;
            cell.UpdateVisual();
        }
        reachableCells.Clear();

        // 使用广度优先搜索计算可到达的格子
        Queue<GridCell> frontier = new Queue<GridCell>();
        Dictionary<GridCell, int> costSoFar = new Dictionary<GridCell, int>();

        frontier.Enqueue(currentCell);
        costSoFar[currentCell] = 0;

        // 将当前格子加入可到达的格子列表，并设置状态
        reachableCells.Add(currentCell);
        currentCell.SetCellState(CellState.Highlighted);

        while (frontier.Count > 0)
        {
            GridCell current = frontier.Dequeue();

            foreach (GridCell next in gridManager.GetNeighbors(current))
            {
                if (!next.isWalkable || next.occupant != null)
                {
                    continue;
                }

                int newCost = costSoFar[current] + 1;
                if (newCost > movementRange)
                {
                    continue;
                }

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next);
                    reachableCells.Add(next);

                    next.cellState = CellState.Highlighted;
                    next.UpdateVisual();
                }
            }
        }
    }

    // 处理鼠标输入
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 发射一条射线检测鼠标点击的位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GridCell clickedCell = GetCellFromPosition(hit.point);
                if (clickedCell != null && reachableCells.Contains(clickedCell))
                {
                    // 计算路径并开始移动
                    path = FindPath(currentCell, clickedCell);
                    if (path != null && path.Count > 0)
                    {
                        StartCoroutine(MoveAlongPath());
                        // 结束当前回合
                        ifTurn = false;
                    }
                }
            }
        }
    }

    // 根据世界坐标获取对应的格子
    GridCell GetCellFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / gridManager.cellSize + 0.5f);
        int y = Mathf.FloorToInt(position.z / gridManager.cellSize + 0.5f);
        return gridManager.GetCell(x, y);
    }

    // 寻路算法（A*）
    List<GridCell> FindPath(GridCell start, GridCell goal)
    {
        List<GridCell> path = new List<GridCell>();

        PriorityQueue<GridCell> frontier = new PriorityQueue<GridCell>();
        frontier.Enqueue(start, 0);

        Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();
        Dictionary<GridCell, int> costSoFar = new Dictionary<GridCell, int>();

        cameFrom[start] = null;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            GridCell current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (GridCell next in gridManager.GetNeighbors(current))
            {
                if (!next.isWalkable || next.occupant != null)
                {
                    continue;
                }

                int newCost = costSoFar[current] + 1;

                if (newCost > movementRange)
                {
                    continue;
                }

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(goal, next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        // 构建路径
        GridCell temp = goal;
        while (temp != start)
        {
            if (cameFrom.ContainsKey(temp))
            {
                path.Add(temp);
                temp = cameFrom[temp];
            }
            else
            {
                // 无法到达目标
                return null;
            }
        }
        path.Reverse();
        return path;
    }

    // 启动协程，按照路径移动
    IEnumerator MoveAlongPath()
    {
        foreach (GridCell cell in path)
        {
            // 更新格子的占据信息
            currentCell.occupant = null;
            cell.occupant = gameObject;
            currentCell = cell;

            // 更新朝向
            //UpdateFacingDirection(transform.position, cell.GetCenterPosition());

            // 平滑移动到下一个格子
            Vector3 startPosition = transform.position;
            Vector3 endPosition = cell.GetCenterPosition();
            float elapsedTime = 0f;
            float journeyLength = Vector3.Distance(startPosition, endPosition);
            float journeyTime = journeyLength / speed;

            while (elapsedTime < journeyTime)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / journeyTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = endPosition;
        }

        // 移动结束后，清空路径和高亮以及敌人的索敌范围
        path.Clear();
        foreach (GridCell cell in reachableCells)
        {
            cell.cellState = CellState.Normal;
            cell.UpdateVisual();
        }
        reachableCells.Clear();
        ClearEnemyDetectionRanges();
    }

    // 清除敌人索敌范围
    void ClearEnemyDetectionRanges()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.ClearDetectionRange();
        }
    }

    // 更新角色朝向
    void UpdateFacingDirection(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector3 direction = toPosition - fromPosition;
        direction.y = 0;  // 忽略Y轴

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    // 计算启发式函数（曼哈顿距离）
    int Heuristic(GridCell a, GridCell b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // 显示所有敌人的索敌范围
    void ShowEnemyDetectionRanges()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.ShowDetectionRange();
        }
    }
}